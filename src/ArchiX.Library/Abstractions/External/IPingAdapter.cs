namespace ArchiX.Library.Abstractions.External;

/// <summary>Dýþ servis ping iþlemleri için sözleþme.</summary>
public interface IPingAdapter
{
 Task<string> GetStatusTextAsync(CancellationToken ct = default);
}
