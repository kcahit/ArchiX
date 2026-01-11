using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.Templates.Modern.Pages.Raporlar;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Templates.Modern.Pages.Raporlar;

public sealed class GridListeRunEndpointTests
{
    [Fact]
    public async Task OnPostRunAsync_Should_Return_Page_When_Executor_Succeeds()
    {
        var executor = new FakeOkExecutor();
        var optionsSvc = new FakeOptionsService(allowId: 1);

        var page = new GridListeModel(executor, optionsSvc);

        var result = await page.OnPostRunAsync(reportDatasetId: 1, ct: default);

        Assert.IsType<PageResult>(result);
        Assert.NotEmpty(page.Columns);
        Assert.NotEmpty(page.Rows);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_Page_When_Executor_Throws()
    {
        var executor = new FakeThrowingExecutor();
        var optionsSvc = new FakeOptionsService(allowId: 1);

        var page = new GridListeModel(executor, optionsSvc);

        var result = await page.OnPostRunAsync(reportDatasetId: 1, ct: default);

        Assert.IsType<PageResult>(result);

        // Hata durumunda fail-closed: ekrana boş grid basılsın
        Assert.Empty(page.Columns);
        Assert.Empty(page.Rows);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_BadRequest_When_ReportDatasetId_Is_Zero_Or_Less()
    {
        var executor = new FakeOkExecutor();
        var optionsSvc = new FakeOptionsService(allowId: 1);

        var page = new GridListeModel(executor, optionsSvc);

        var result = await page.OnPostRunAsync(reportDatasetId: 0, ct: default);

        Assert.IsType<BadRequestResult>(result);
    }

    private sealed class FakeOkExecutor : IReportDatasetExecutor
    {
        public Task<ReportDatasetExecutionResult> ExecuteAsync(ReportDatasetExecutionRequest request, CancellationToken ct = default)
        {
            var cols = new[] { "id", "name" };
            var rows = new List<IReadOnlyList<object?>>
            {
                new object?[] { 1, "A" }
            };

            return Task.FromResult(new ReportDatasetExecutionResult(cols, rows));
        }
    }

    private sealed class FakeThrowingExecutor : IReportDatasetExecutor
    {
        public Task<ReportDatasetExecutionResult> ExecuteAsync(ReportDatasetExecutionRequest request, CancellationToken ct = default)
            => throw new InvalidOperationException("boom");
    }

    private sealed class FakeOptionsService : IReportDatasetOptionService
    {
        private readonly int _allowId;

        public FakeOptionsService(int allowId)
        {
            _allowId = allowId;
        }

        public Task<IReadOnlyList<ReportDatasetOptionViewModel>> GetApprovedOptionsAsync(CancellationToken ct = default)
        {
            IReadOnlyList<ReportDatasetOptionViewModel> list =
            [
                new ReportDatasetOptionViewModel(_allowId, "Allowed")
            ];

            return Task.FromResult(list);
        }
    }
}
