using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.Pages.Tools.Dataset;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Pages.Tools.Dataset;

public sealed class DatasetKombinePageModelTests
{
    [Fact]
    public async Task OnPostRunAsync_Should_Return_BadRequest_When_ReportDatasetId_Invalid()
    {
        var page = new DatasetKombinePageModel(new FakeExecutor(), new FakeOptionsService([]));

        var result = await page.OnPostRunAsync(reportDatasetId: 0, ct: default);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_BadRequest_When_Dataset_Not_Approved()
    {
        var page = new DatasetKombinePageModel(new FakeExecutor(), new FakeOptionsService([]));

        var result = await page.OnPostRunAsync(reportDatasetId: 123, ct: default);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_Page_When_Dataset_Approved()
    {
        IReadOnlyList<ReportDatasetOptionViewModel> opts = [new ReportDatasetOptionViewModel(1, "x")];
        var page = new DatasetKombinePageModel(new FakeExecutor(), new FakeOptionsService(opts));

        var result = await page.OnPostRunAsync(reportDatasetId: 1, ct: default);

        Assert.IsType<PageResult>(result);
    }

    private sealed class FakeExecutor : IReportDatasetExecutor
    {
        public Task<ReportDatasetExecutionResult> ExecuteAsync(ReportDatasetExecutionRequest request, CancellationToken ct = default)
        {
            var cols = new[] { "id", "name" };
            var rows = new List<IReadOnlyList<object?>> { new List<object?> { 1, "a" } };
            return Task.FromResult(new ReportDatasetExecutionResult(cols, rows));
        }
    }

    private sealed class FakeOptionsService : IReportDatasetOptionService
    {
        private readonly IReadOnlyList<ReportDatasetOptionViewModel> _opts;

        public FakeOptionsService(IReadOnlyList<ReportDatasetOptionViewModel> opts) => _opts = opts;

        public Task<IReadOnlyList<ReportDatasetOptionViewModel>> GetApprovedOptionsAsync(CancellationToken ct = default)
            => Task.FromResult(_opts);
    }
}
