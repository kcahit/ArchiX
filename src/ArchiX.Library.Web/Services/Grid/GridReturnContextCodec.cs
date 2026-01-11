using System.Text;
using System.Text.Json;

using ArchiX.Library.Web.ViewModels.Grid;

namespace ArchiX.Library.Web.Services.Grid;

public static class GridReturnContextCodec
{
    public static string Encode(GridReturnContextViewModel ctx)
    {
        var json = JsonSerializer.Serialize(ctx);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static bool TryDecode(string? value, out GridReturnContextViewModel? ctx)
    {
        ctx = null;
        if (string.IsNullOrWhiteSpace(value)) return false;

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            ctx = JsonSerializer.Deserialize<GridReturnContextViewModel>(json);
            return ctx is not null;
        }
        catch
        {
            ctx = null;
            return false;
        }
    }
}
