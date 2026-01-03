namespace ArchiX.Library.Web.ViewModels.Grid;

public record GridColumnDefinition(
    string Field,
    string Title,
    string DataType = "string",
    bool Sortable = true,
    bool Filterable = true,
    string? Format = null,
    string? Width = null
);
