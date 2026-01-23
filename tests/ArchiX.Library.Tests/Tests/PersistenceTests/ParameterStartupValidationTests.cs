using ArchiX.Library.Abstractions.Caching;
using ArchiX.Library.Context;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.Parameters;
using ArchiX.Library.Services.Parameters;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ArchiX.Library.Tests.Tests.PersistenceTests;

/// <summary>
/// #57 Startup validation testleri.
/// Kritik parametrelerin varlığını kontrol eder.
/// </summary>
public sealed class ParameterStartupValidationTests : IClassFixture<ParameterStartupValidationFixture>
{
    private readonly ParameterStartupValidationFixture _fixture;

    public ParameterStartupValidationTests(ParameterStartupValidationFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// 11.F.5: Kritik parametreler varsa startup başarılı olmalı.
    /// </summary>
    [Fact]
    public async Task Startup_should_succeed_when_all_critical_parameters_present()
    {
        // Arrange
        using var provider = _fixture.CreateSeededProvider();
        using var scope = provider.CreateScope();
        var paramService = scope.ServiceProvider.GetRequiredService<IParameterService>();

        // Act: Kritik parametreleri oku
        var uiTimeout = await paramService.GetParameterAsync<ArchiX.Library.Web.Configuration.UiTimeoutOptions>(
            "UI", "TimeoutOptions", applicationId: 1);
        
        var httpPolicies = await paramService.GetParameterAsync<ArchiX.Library.Infrastructure.Http.HttpPoliciesOptions>(
            "HTTP", "HttpPoliciesOptions", applicationId: 1);
        
        var attemptLimiter = await paramService.GetParameterAsync<ArchiX.Library.Abstractions.Security.AttemptLimiterOptions>(
            "Security", "AttemptLimiterOptions", applicationId: 1);

        // Assert
        uiTimeout.Should().NotBeNull();
        uiTimeout!.SessionTimeoutSeconds.Should().Be(645);

        httpPolicies.Should().NotBeNull();
        httpPolicies!.RetryCount.Should().Be(2);

        attemptLimiter.Should().NotBeNull();
        attemptLimiter!.MaxAttempts.Should().Be(5);
    }

    /// <summary>
    /// 11.F.5: JSON parse hatası olursa InvalidOperationException fırlatmalı.
    /// </summary>
    [Fact]
    public async Task Startup_should_fail_when_parameter_json_invalid()
    {
        // Arrange
        using var provider = _fixture.CreateSeededProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var paramService = scope.ServiceProvider.GetRequiredService<IParameterService>();

        // Geçersiz JSON yaz
        var param = await db.Parameters.FirstAsync(p => p.Group == "UI" && p.Key == "TimeoutOptions");
        var paramApp = await db.ParameterApplications.FirstAsync(pa => pa.ParameterId == param.Id);
        paramApp.Value = "{ invalid json }";
        await db.SaveChangesAsync();

        // Cache'i temizle (eğer cache'lenmişse)
        paramService.InvalidateCache("UI", "TimeoutOptions");

        // Act & Assert
        var act = async () => await paramService.GetParameterAsync<ArchiX.Library.Web.Configuration.UiTimeoutOptions>(
            "UI", "TimeoutOptions", applicationId: 1);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deserialize*");
    }

}

public sealed class ParameterStartupValidationFixture : IDisposable
{
    public ServiceProvider CreateSeededProvider() => BuildProvider(seed: true);
    public ServiceProvider CreateEmptyProvider() => BuildProvider(seed: false);

    private static ServiceProvider BuildProvider(bool seed)
    {
        var services = new ServiceCollection();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName), ServiceLifetime.Singleton);

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddLogging(builder => builder.AddConsole());

        services.AddSingleton(new ParameterRefreshOptions());
        services.AddScoped<IParameterService, ParameterService>();

        var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (seed)
            SeedManually(db);

        return sp;
    }

    private static void SeedManually(AppDbContext db)
    {
        // UI/TimeoutOptions
        var p1 = new ArchiX.Library.Entities.Parameter
        {
            Group = "UI",
            Key = "TimeoutOptions",
            ParameterDataTypeId = 15,
            StatusId = ArchiX.Library.Entities.BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = true,
            RowId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Parameters.Add(p1);
        db.SaveChanges();
        db.ParameterApplications.Add(new ArchiX.Library.Entities.ParameterApplication
        {
            ParameterId = p1.Id,
            ApplicationId = 1,
            Value = "{\"sessionTimeoutSeconds\":645,\"sessionWarningSeconds\":45,\"tabRequestTimeoutMs\":30000}",
            StatusId = ArchiX.Library.Entities.BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = true,
            RowId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        // HTTP/HttpPoliciesOptions
        var p2 = new ArchiX.Library.Entities.Parameter
        {
            Group = "HTTP",
            Key = "HttpPoliciesOptions",
            ParameterDataTypeId = 15,
            StatusId = ArchiX.Library.Entities.BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = true,
            RowId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Parameters.Add(p2);
        db.SaveChanges();
        db.ParameterApplications.Add(new ArchiX.Library.Entities.ParameterApplication
        {
            ParameterId = p2.Id,
            ApplicationId = 1,
            Value = "{\"retryCount\":2,\"baseDelayMs\":200,\"timeoutSeconds\":30}",
            StatusId = ArchiX.Library.Entities.BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = true,
            RowId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        // Security/AttemptLimiterOptions
        var p3 = new ArchiX.Library.Entities.Parameter
        {
            Group = "Security",
            Key = "AttemptLimiterOptions",
            ParameterDataTypeId = 15,
            StatusId = ArchiX.Library.Entities.BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = true,
            RowId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Parameters.Add(p3);
        db.SaveChanges();
        db.ParameterApplications.Add(new ArchiX.Library.Entities.ParameterApplication
        {
            ParameterId = p3.Id,
            ApplicationId = 1,
            Value = "{\"window\":600,\"maxAttempts\":5,\"cooldownSeconds\":300}",
            StatusId = ArchiX.Library.Entities.BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = true,
            RowId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        db.SaveChanges();
    }

    public void Dispose()
    {
    }
}

