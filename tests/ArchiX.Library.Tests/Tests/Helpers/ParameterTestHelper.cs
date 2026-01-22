using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Tests.Tests.Helpers;

/// <summary>
/// Test dosyalarında Parameter/ParameterApplication seed helper metodları
/// </summary>
public static class ParameterTestHelper
{
    /// <summary>
    /// Security/PasswordPolicy parametresini seed eder (yeni master/detail şema)
    /// </summary>
    public static async Task SeedPasswordPolicyAsync(AppDbContext db, int appId = 1, string? customJson = null)
    {
        var param = await db.Parameters
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Group == "Security" && p.Key == "PasswordPolicy");

        if (param == null)
        {
            param = new Parameter
            {
                Group = "Security",
                Key = "PasswordPolicy",
                ParameterDataTypeId = 15,
                Description = "Test password policy",
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Parameters.Add(param);
            await db.SaveChangesAsync();
        }

        var appValue = param.Applications.FirstOrDefault(a => a.ApplicationId == appId);
        if (appValue == null)
        {
            var json = customJson ?? @"{""version"":1}";
            
            db.ParameterApplications.Add(new ParameterApplication
            {
                ParameterId = param.Id,
                ApplicationId = appId,
                Value = json,
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// ConnectionStrings parametresini seed eder
    /// </summary>
    public static async Task SeedConnectionStringsAsync(AppDbContext db, int appId = 1, string? customJson = null)
    {
        var param = await db.Parameters
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Group == "ConnectionStrings" && p.Key == "ConnectionStrings");

        if (param == null)
        {
            param = new Parameter
            {
                Group = "ConnectionStrings",
                Key = "ConnectionStrings",
                ParameterDataTypeId = 15,
                Description = "Test connection strings",
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Parameters.Add(param);
            await db.SaveChangesAsync();
        }

        var appValue = param.Applications.FirstOrDefault(a => a.ApplicationId == appId);
        if (appValue == null)
        {
            var json = customJson ?? @"{""Test"":{""Provider"":""SqlServer""}}";
            
            db.ParameterApplications.Add(new ParameterApplication
            {
                ParameterId = param.Id,
                ApplicationId = appId,
                Value = json,
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Genel parametre seed helper
    /// </summary>
    public static async Task SeedParameterAsync(AppDbContext db, string group, string key, int appId, string value)
    {
        var param = await db.Parameters
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Group == group && p.Key == key);

        if (param == null)
        {
            param = new Parameter
            {
                Group = group,
                Key = key,
                ParameterDataTypeId = 15,
                Description = $"Test {group}/{key}",
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Parameters.Add(param);
            await db.SaveChangesAsync();
        }

        var appValue = param.Applications.FirstOrDefault(a => a.ApplicationId == appId);
        if (appValue == null)
        {
            db.ParameterApplications.Add(new ParameterApplication
            {
                ParameterId = param.Id,
                ApplicationId = appId,
                Value = value,
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
