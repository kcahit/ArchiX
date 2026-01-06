using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Connections;
using ArchiX.Library.Runtime.Reports;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RunTimeTests.Reports;

public sealed class ReportDatasetExecutorSentinelTests
{
    [Fact]
    public async Task FileDataset_PathTraversal_IsBlocked()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase(dbName));

        services.AddSingleton<ArchiX.Library.Abstractions.Connections.ISecretResolver, EnvSecretResolver>();
        services.AddSingleton<ArchiX.Library.Abstractions.Connections.IConnectionProfileProvider, ArchixParameterConnectionProfileProvider>();

        services.AddSingleton<IConnectionPolicyProvider>(_ => new TestConnectionPolicyProvider("Off"));
        services.AddSingleton<IConnectionPolicyAuditor>(_ => new NoOpAuditor());
        services.AddSingleton<IConnectionPolicyEvaluator>(sp =>
            new ArchiX.Library.Runtime.ConnectionPolicy.ConnectionPolicyEvaluator(
                sp.GetRequiredService<IConnectionPolicyProvider>(),
                sp.GetRequiredService<IConnectionPolicyAuditor>()));

        services.AddSingleton<ConnectionStringBuilderService>();

        services.AddArchiXReports();

        var sp = services.BuildServiceProvider();

        await using (var db = await sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();

            await ReportDatasetStartup.EnsureSeedAsync(db);

            var typeId = await db.ReportDatasetTypes
                .Where(t => t.Code == "ndjson")
                .Select(t => t.Id)
                .SingleAsync();

            db.ReportDatasets.Add(new ReportDataset
            {
                ReportDatasetTypeId = typeId,
                FileName = "..\\evil.ndjson",
                DisplayName = "Evil",
                SubPath = "..",
                StatusId = BaseEntity.ApprovedStatusId,
                CreatedBy = 0,
                LastStatusBy = 0
            });

            db.Parameters.Add(new Parameter
            {
                ApplicationId = 1,
                Group = "Reports",
                Key = "FileDatasetRoot",
                ParameterDataTypeId = 1,
                Description = "root",
                Value = "C:\\data",
                Template = "C:\\data",
                StatusId = BaseEntity.ApprovedStatusId,
                CreatedBy = 0,
                LastStatusBy = 0
            });

            await db.SaveChangesAsync();
        }

        var exec = sp.GetRequiredService<IReportDatasetExecutor>();

        await using var db2 = await sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync();
        var dsId = await db2.ReportDatasets.Select(x => x.Id).SingleAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await exec.ExecuteAsync(new ReportDatasetExecutionRequest(dsId)));
    }

    [Fact]
    public void Limits_MaxCells_DerivesAllowedRows_And_ClampCell_Works()
    {
        // Global defaults via options; this test does not require DB or SQL.
        var opt = Options.Create(new ReportDatasetLimitOptions
        {
            MaxCells = 10,     // 10 cells budget
            HardMaxRows = 999, // will be capped by MaxCells/cols
            HardMaxCols = 999,
            MaxCellChars = 3
        });

        var guard = new ReportDatasetLimitGuard(opt);

        var limits = guard.Resolve(null);

        // If there are 2 columns, MaxCells=10 => allowedRows = floor(10/2)=5
        Assert.Equal(5, guard.AllowedRowsFor(colCount: 2, limits));

        // If there are 3 columns, allowedRows = floor(10/3)=3
        Assert.Equal(3, guard.AllowedRowsFor(colCount: 3, limits));

        // Cell clamp
        Assert.Equal("abc", guard.ClampCell("abcdef", limits));
        Assert.Equal("ab", guard.ClampCell("ab", limits));
        Assert.Null(guard.ClampCell(null, limits));
    }

    private sealed class NoOpAuditor : IConnectionPolicyAuditor
    {
        public void TryWrite(string rawConnectionString, ConnectionPolicyResult result) { }
    }

    private sealed class TestConnectionPolicyProvider(string mode) : IConnectionPolicyProvider
    {
        public ConnectionPolicyOptions Current { get; } = new() { Mode = mode };

        public void ForceRefresh() { }
    }
}
