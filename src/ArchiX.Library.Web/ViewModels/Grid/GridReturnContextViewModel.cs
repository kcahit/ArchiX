namespace ArchiX.Library.Web.ViewModels.Grid;

public sealed record GridReturnContextViewModel(
    string? Search,
    int? Page,
    int? ItemsPerPage);
