using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Pages.Tools.Dataset;

using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Pages.Tools.Dataset;

public sealed class DatasetRecordPageSingleRowEndpointTests
{
    [Fact]
    public async Task OnPostRunAsync_Should_Return_Ok_When_RowCount_Is_1()
    {
        var executor = new FakeExecutor(columns: new[] { "id", "name" }, rows: new List<IReadOnlyList<object?>> { new object?[] { 1, "A" } });
        var optionsSvc = new FakeOptionsService(allowId: 1);
        var page = new DatasetRecordPageModel(executor, optionsSvc);

        var result = await page.OnPostRunAsync(reportDatasetId: 1, rowId: "1", returnContext: null, ct: default);

        Assert.IsType<OkResult>(result);
        Assert.NotNull(page.Customer);
        Assert.Equal(1, page.Customer!.Id);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_BadRequest_When_RowCount_Is_0()
    {
        var executor = new FakeExecutor(columns: new[] { "id" }, rows: new List<IReadOnlyList<object?>>());
        var optionsSvc = new FakeOptionsService(allowId: 1);
        var page = new DatasetRecordPageModel(executor, optionsSvc);

        var result = await page.OnPostRunAsync(reportDatasetId: 1, rowId: "1", returnContext: null, ct: default);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_BadRequest_When_RowCount_Is_Greater_Than_1()
    {
        var executor = new FakeExecutor(columns: new[] { "id" }, rows: new List<IReadOnlyList<object?>> { new object?[] { 1 }, new object?[] { 2 } });
        var optionsSvc = new FakeOptionsService(allowId: 1);
        var page = new DatasetRecordPageModel(executor, optionsSvc);

        var result = await page.OnPostRunAsync(reportDatasetId: 1, rowId: "1", returnContext: null, ct: default);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_BadRequest_When_ReportDatasetId_Is_Zero_Or_Less()
    {
        var executor = new FakeExecutor(columns: new[] { "id" }, rows: new List<IReadOnlyList<object?>> { new object?[] { 1 } });
        var optionsSvc = new FakeOptionsService(allowId: 1);
        var page = new DatasetRecordPageModel(executor, optionsSvc);

        var result = await page.OnPostRunAsync(reportDatasetId: 0, rowId: "1", returnContext: null, ct: default);

        Assert.IsType<BadRequestResult>(result);
    }

    private sealed class FakeExecutor : IReportDatasetExecutor
    {
        private readonly IReadOnlyList<string> _columns;
        private readonly IReadOnlyList<IReadOnlyList<object?>> _rows;

        public FakeExecutor(IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<object?>> rows)
        {
            _columns = columns;
            _rows = rows;
        }

        public Task<ReportDatasetExecutionResult> ExecuteAsync(ReportDatasetExecutionRequest request, CancellationToken ct = default)
            => Task.FromResult(new ReportDatasetExecutionResult(_columns, _rows));
    }

    private sealed class FakeOptionsService : ArchiX.Library.Web.Abstractions.Reports.IReportDatasetOptionService
    {
        private readonly int _allowId;

        public FakeOptionsService(int allowId)
        {
            _allowId = allowId;
        }

        public Task<IReadOnlyList<ArchiX.Library.Web.ViewModels.Grid.ReportDatasetOptionViewModel>> GetApprovedOptionsAsync(CancellationToken ct = default)
        {
            IReadOnlyList<ArchiX.Library.Web.ViewModels.Grid.ReportDatasetOptionViewModel> list =
            [
                new ArchiX.Library.Web.ViewModels.Grid.ReportDatasetOptionViewModel(_allowId, "Allowed")
            ];

            return Task.FromResult(list);
        }
    }
}
