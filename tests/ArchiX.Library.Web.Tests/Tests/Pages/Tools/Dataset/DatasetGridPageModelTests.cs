using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.Pages.Tools.Dataset;
using ArchiX.Library.Web.Services.Grid;
using ArchiX.Library.Web.ViewModels.Grid;

using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Pages.Tools.Dataset;

public sealed class DatasetGridPageModelTests
{
    [Fact]
    public async Task OnGetAsync_Should_Default_IsFormOpenEnabled_To_Zero_When_Not_Provided()
    {
        var page = new DatasetGridPageModel(new FakeExecutor(), new FakeOptionsService());

        await page.OnGetAsync(reportDatasetId: null, returnContext: null, isFormOpenEnabled: null, ct: default);

        Assert.Equal(0, page.IsFormOpenEnabled);
    }

    [Fact]
    public async Task OnGetAsync_Should_Normalize_IsFormOpenEnabled_To_One_Only_When_Query_Is_One()
    {
        var page1 = new DatasetGridPageModel(new FakeExecutor(), new FakeOptionsService());
        await page1.OnGetAsync(reportDatasetId: null, returnContext: null, isFormOpenEnabled: 1, ct: default);
        Assert.Equal(1, page1.IsFormOpenEnabled);

        var page2 = new DatasetGridPageModel(new FakeExecutor(), new FakeOptionsService());
        await page2.OnGetAsync(reportDatasetId: null, returnContext: null, isFormOpenEnabled: 2, ct: default);
        Assert.Equal(0, page2.IsFormOpenEnabled);

        var page3 = new DatasetGridPageModel(new FakeExecutor(), new FakeOptionsService());
        await page3.OnGetAsync(reportDatasetId: null, returnContext: null, isFormOpenEnabled: -1, ct: default);
        Assert.Equal(0, page3.IsFormOpenEnabled);
    }

    [Fact]
    public async Task OnGetAsync_Should_Ignore_Invalid_ReturnContext_FailSafe()
    {
        var page = new DatasetGridPageModel(new FakeExecutor(), new FakeOptionsService());

        await page.OnGetAsync(reportDatasetId: null, returnContext: "not-base64", isFormOpenEnabled: null, ct: default);

        Assert.Null(page.RestoredSearch);
        Assert.Null(page.RestoredPage);
        Assert.Null(page.RestoredItemsPerPage);
    }

    [Fact]
    public async Task OnGetAsync_Should_Restore_Context_When_Valid_ReturnContext_Provided()
    {
        var page = new DatasetGridPageModel(new FakeExecutor(), new FakeOptionsService());

        var ctx = GridReturnContextCodec.Encode(new GridReturnContextViewModel("q", 3, 25));

        await page.OnGetAsync(reportDatasetId: null, returnContext: ctx, isFormOpenEnabled: null, ct: default);

        Assert.Equal("q", page.RestoredSearch);
        Assert.Equal(3, page.RestoredPage);
        Assert.Equal(25, page.RestoredItemsPerPage);
    }

    private sealed class FakeExecutor : IReportDatasetExecutor
    {
        public Task<ReportDatasetExecutionResult> ExecuteAsync(ReportDatasetExecutionRequest request, CancellationToken ct = default)
            => Task.FromResult(new ReportDatasetExecutionResult(Array.Empty<string>(), new List<IReadOnlyList<object?>>()));
    }

    private sealed class FakeOptionsService : IReportDatasetOptionService
    {
        public Task<IReadOnlyList<ReportDatasetOptionViewModel>> GetApprovedOptionsAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<ReportDatasetOptionViewModel>>([]);
    }
}
