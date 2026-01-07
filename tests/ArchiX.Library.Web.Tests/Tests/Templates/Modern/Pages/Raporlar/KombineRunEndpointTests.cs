using ArchiX.Library.Abstractions.Reports;
using ArchiX.Library.Web.Templates.Modern.Pages.Raporlar;

using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Templates.Modern.Pages.Raporlar;

public sealed class KombineRunEndpointTests
{
    [Fact]
    public async Task OnPostRunAsync_Should_Return_Ok_When_Executor_Succeeds()
    {
        var executor = new FakeOkExecutor();
        var page = new KombineModel(executor);

        var result = await page.OnPostRunAsync(reportDatasetId: 1, ct: default);

        Assert.IsType<OkResult>(result);
        Assert.NotEmpty(page.Columns);
        Assert.NotEmpty(page.Rows);
    }

    [Fact]
    public async Task OnPostRunAsync_Should_Return_BadRequest_When_Executor_Throws()
    {
        var executor = new FakeThrowingExecutor();
        var page = new KombineModel(executor);

        var result = await page.OnPostRunAsync(reportDatasetId: 1, ct: default);

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
}
