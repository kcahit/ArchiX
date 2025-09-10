using System.Globalization;

using ArchiX.Library.Context;
using ArchiX.Library.LanguagePacks;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.DiagnosticsTests
{
    /// <summary>
    /// LanguageService + LanguagePack çok dillilik testleri.
    /// </summary>
    public class LanguageServiceTests
    {
        private static AppDbContext CreateDb(out LanguageService svc)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"langpacks_{Guid.NewGuid()}")
                .Options;

            var db = new AppDbContext(opts);

            db.Set<LanguagePack>().AddRange(
            [
                new LanguagePack { Id = -1001, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "tr-TR", DisplayName = "Eşittir", Description = "Değer eşit olmalı", StatusId = 3 },
                new LanguagePack { Id = -1002, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "en-US", DisplayName = "Equals", Description = "Value must be equal", StatusId = 3 },
                new LanguagePack { Id = -1003, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "tr-TR", DisplayName = "Eşit Değil", Description = "Değer eşit olmamalı", StatusId = 3 },
                new LanguagePack { Id = -1004, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "en-US", DisplayName = "Not Equal", Description = "Value must not be equal", StatusId = 3 },
                new LanguagePack { Id = -1005, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "tr-TR", DisplayName = "Başlar", Description = "Başlangıç eşleşmesi", StatusId = 3 },
                new LanguagePack { Id = -1006, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "en-US", DisplayName = "Starts With", Description = "Value starts with given text", StatusId = 3 },
                new LanguagePack { Id = -1999, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Hidden", Culture = "tr-TR", DisplayName = "Gizli", Description = "Pasif kayıt", StatusId = 2 }
            ]);

            db.SaveChanges();
            svc = new LanguageService(db: db);
            return db;
        }

        [Theory]
        [InlineData("tr-TR", "Equals", "Eşittir")]
        [InlineData("en-US", "Equals", "Equals")]
        [InlineData("tr-TR", "NotEquals", "Eşit Değil")]
        [InlineData("en-US", "NotEquals", "Not Equal")]
        [InlineData("tr-TR", "StartsWith", "Başlar")]
        [InlineData("en-US", "StartsWith", "Starts With")]
        public async Task GetDisplayNameAsync_should_return_correct_translation(string culture, string code, string expected)
        {
            using var db = CreateDb(out var svc);

            var text = await svc.GetDisplayNameAsync(
                itemType: "Operator",
                entityName: "FilterItem",
                fieldName: "Code",
                code: code,
                culture: culture);

            Assert.Equal(expected, text);
        }

        [Fact]
        public async Task GetDisplayNameAsync_should_return_null_when_not_found_or_not_active()
        {
            using var db = CreateDb(out var svc);

            var notFound = await svc.GetDisplayNameAsync("Operator", "FilterItem", "Code", "DoesNotExist", "tr-TR");
            Assert.Null(notFound);

            var hidden = await svc.GetDisplayNameAsync("Operator", "FilterItem", "Code", "Hidden", "tr-TR");
            Assert.Null(hidden);
        }

        [Fact]
        public async Task GetListAsync_should_return_active_culture_specific_list()
        {
            using var db = CreateDb(out var svc);

            var listTr = await svc.GetListAsync("Operator", "FilterItem", "Code", "tr-TR");

            Assert.NotNull(listTr);
            Assert.Equal(3, listTr.Count);

            var namesTr = listTr.Select(x => x.DisplayName).ToList();
            Assert.Contains("Eşittir", namesTr);
            Assert.Contains("Eşit Değil", namesTr);
            Assert.Contains("Başlar", namesTr);
            Assert.DoesNotContain("Gizli", namesTr);

            var listEn = await svc.GetListAsync("Operator", "FilterItem", "Code", "en-US");
            Assert.Equal(3, listEn.Count);
            var namesEn = listEn.Select(x => x.DisplayName).ToList();
            Assert.Contains("Equals", namesEn);
            Assert.Contains("Not Equal", namesEn);
            Assert.Contains("Starts With", namesEn);
        }

        [Fact]
        public void Returns_Key_When_Not_Found()
        {
            var service = new LanguageService(new CultureInfo("tr-TR"));
            var result = service.T("olmayan_anahtar");
            Assert.Equal("olmayan_anahtar", result);
        }

        [Fact]
        public void Returns_Seeded_Value()
        {
            var seed = new Dictionary<string, string> { { "hello", "merhaba" } };
            var service = new LanguageService(seed, new CultureInfo("tr-TR"));
            var result = service.T("hello");
            Assert.Equal("merhaba", result);
        }

        [Fact]
        public void Returns_Formatted_Value()
        {
            var seed = new Dictionary<string, string> { { "welcome", "hoşgeldin {0}" } };
            var service = new LanguageService(seed, new CultureInfo("tr-TR"));
            var result = service.T("welcome", "Cahit");
            Assert.Equal("hoşgeldin Cahit", result);
        }
    }
}
