using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.Pages.Tools.Dataset;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Pages.Tools.Dataset;

public sealed class DatasetRecordPageModelTests
{
    [Fact]
    public void OnPostUpdate_Should_Return_BadRequest_When_HasRecordOperations_Is_Zero()
    {
        var page = new DatasetRecordPageModel(new FakeExecutor(), new FakeOptionsService());

        var result = page.OnPostUpdate(hasRecordOperations: 0);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void OnPostUpdate_Should_Return_Ok_When_HasRecordOperations_Is_One()
    {
        var page = new DatasetRecordPageModel(new FakeExecutor(), new FakeOptionsService());

        var result = page.OnPostUpdate(hasRecordOperations: 1);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void OnPostDelete_Should_Return_BadRequest_When_HasRecordOperations_Is_Zero()
    {
        var page = new DatasetRecordPageModel(new FakeExecutor(), new FakeOptionsService());

        var result = page.OnPostDelete(hasRecordOperations: 0, rowId: "1");

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void OnPostDelete_Should_Return_BadRequest_When_RowId_Is_Missing_In_New_Mode()
    {
        var page = new DatasetRecordPageModel(new FakeExecutor(), new FakeOptionsService());

        var result = page.OnPostDelete(hasRecordOperations: 1, rowId: null);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public void OnPostDelete_Should_Return_Ok_When_HasRecordOperations_Is_One_And_RowId_Present()
    {
        var page = new DatasetRecordPageModel(new FakeExecutor(), new FakeOptionsService());

        var result = page.OnPostDelete(hasRecordOperations: 1, rowId: "1");

        Assert.IsType<OkResult>(result);
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
