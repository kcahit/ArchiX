using ArchiX.Library.Context;
using ArchiX.Library.LanguagePacks;

using ArchiXTest.ApiWeb.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.DiagnosticsTests
{
    public class LocalizationControllerTests
    {
        private static AppDbContext CreateDb(out LocalizationController controller)
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"lang_api_{Guid.NewGuid()}")
                .Options;

            var db = new AppDbContext(opts);

            // Aktif (StatusId == 3) kayıtlar
            db.Set<LanguagePack>().AddRange(
            [
                new LanguagePack { Id = -1001, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals",     Culture = "tr-TR", DisplayName = "Eşittir",    Description = "Değer eşit olmalı",       StatusId = 3 },
                new LanguagePack { Id = -1002, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals",     Culture = "en-US", DisplayName = "Equals",     Description = "Value must be equal",      StatusId = 3 },
                new LanguagePack { Id = -1003, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals",  Culture = "tr-TR", DisplayName = "Eşit Değil", Description = "Değer eşit olmamalı",      StatusId = 3 },
                new LanguagePack { Id = -1004, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals",  Culture = "en-US", DisplayName = "Not Equal",  Description = "Value must not be equal",  StatusId = 3 },
                new LanguagePack { Id = -1005, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "tr-TR", DisplayName = "Başlar",     Description = "Başlangıç eşleşmesi",      StatusId = 3 },
                new LanguagePack { Id = -1006, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "en-US", DisplayName = "Starts With",Description = "Value starts with...",      StatusId = 3 },

                // Pasif (görünmemeli)
                new LanguagePack { Id = -1999, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Hidden",     Culture = "tr-TR", DisplayName = "Gizli",      Description = "Pasif kayıt",              StatusId = 2 }
            ]);
            db.SaveChanges();

            // LanguageService db'den aktifleri (disp:/list:) belleğe yükler
            var lang = new LanguageService(db: db);
            controller = new LocalizationController(lang);
            return db;
        }

        [Fact]
        public async Task DisplayName_trTR_Equals_returns_200_and_value()
        {
            using var db = CreateDb(out var ctrl);

            var result = await ctrl.GetDisplayName(
                itemType: "Operator",
                entityName: "FilterItem",
                fieldName: "Code",
                code: "Equals",
                culture: "tr-TR",
                ct: CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Eşittir", Assert.IsType<string>(ok.Value));
        }

        [Fact]
        public async Task DisplayName_returns_404_when_missing_or_inactive()
        {
            using var db = CreateDb(out var ctrl);

            var notFound = await ctrl.GetDisplayName("Operator", "FilterItem", "Code", "DoesNotExist", "tr-TR", CancellationToken.None);
            Assert.IsType<NotFoundResult>(notFound.Result);

            // Pasif kayıt => 404
            var hidden = await ctrl.GetDisplayName("Operator", "FilterItem", "Code", "Hidden", "tr-TR", CancellationToken.None);
            Assert.IsType<NotFoundResult>(hidden.Result);
        }

        [Fact]
        public async Task List_trTR_returns_200_and_three_items()
        {
            using var db = CreateDb(out var ctrl);

            var result = await ctrl.GetList(
                itemType: "Operator",
                entityName: "FilterItem",
                fieldName: "Code",
                culture: "tr-TR",
                ct: CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<LocalizationController.DisplayItem>>(ok.Value);
            var list = items.ToList();

            Assert.Equal(3, list.Count);
            Assert.Contains(list, x => x.DisplayName == "Eşittir");
            Assert.Contains(list, x => x.DisplayName == "Eşit Değil");
            Assert.Contains(list, x => x.DisplayName == "Başlar");
        }

        [Fact]
        public async Task List_unknown_culture_returns_200_and_empty_list()
        {
            using var db = CreateDb(out var ctrl);

            var result = await ctrl.GetList("Operator", "FilterItem", "Code", "de-DE", CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IEnumerable<LocalizationController.DisplayItem>>(ok.Value);
            Assert.Empty(items);
        }
    }
}
