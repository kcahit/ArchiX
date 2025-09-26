// File: src/ArchiX.Library/External/IPingAdapter.cs
#nullable enable

namespace ArchiX.Library.External
{
    /// <summary>Dış servis ping işlemleri için sözleşme.</summary>
    public interface IPingAdapter
    {
        /// <summary>/status çağrısını yapar ve yanıt gövdesini düz metin döner.</summary>
        /// <param name="ct">İptal belirteci.</param>
        Task<string> GetStatusTextAsync(CancellationToken ct = default);
    }
}
