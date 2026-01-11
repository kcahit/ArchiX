using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;

class Program
{
    static async Task Main()
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__ArchiXDb")
                 ?? throw new Exception("Conn yok: ConnectionStrings__ArchiXDb");
        var opt = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(cs)
            .EnableDetailedErrors()
            .Options;

        using var db = new AppDbContext(opt);
        await db.EnsureCoreSeedsAndBindAsync();

        var s = await db.Set<Statu>().CountAsync();
        var f = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var l = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        Console.WriteLine($"Seed OK -> Status={s}, FilterItems={f}, LanguagePacks={l}");
    }
}
