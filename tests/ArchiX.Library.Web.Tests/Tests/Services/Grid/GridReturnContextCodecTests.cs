using ArchiX.Library.Web.Services.Grid;
using ArchiX.Library.Web.ViewModels.Grid;

using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Services.Grid;

public sealed class GridReturnContextCodecTests
{
    [Fact]
    public void TryDecode_Should_Be_CaseInsensitive_For_Json_Properties()
    {
        // JS side produces camelCase: search/page/itemsPerPage
        var json = "{\"search\":\"abc\",\"page\":3,\"itemsPerPage\":25}";
        var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

        var ok = GridReturnContextCodec.TryDecode(b64, out var ctx);

        Assert.True(ok);
        Assert.NotNull(ctx);
        Assert.Equal("abc", ctx!.Search);
        Assert.Equal(3, ctx.Page);
        Assert.Equal(25, ctx.ItemsPerPage);
    }

    [Fact]
    public void TryDecode_Should_Return_False_On_Invalid_Base64()
    {
        var ok = GridReturnContextCodec.TryDecode("not-base64", out var ctx);
        Assert.False(ok);
        Assert.Null(ctx);
    }

    [Fact]
    public void Encode_Then_TryDecode_Should_Roundtrip()
    {
        var encoded = GridReturnContextCodec.Encode(new GridReturnContextViewModel("q", 2, 50));
        var ok = GridReturnContextCodec.TryDecode(encoded, out var decoded);

        Assert.True(ok);
        Assert.Equal("q", decoded!.Search);
        Assert.Equal(2, decoded.Page);
        Assert.Equal(50, decoded.ItemsPerPage);
    }
}
