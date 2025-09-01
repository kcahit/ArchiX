namespace ArchiX.Library.Result;

/// <summary>
/// Generic sonuç tipini temsil eder.
/// Başarı veya hata bilgisini taşır.
/// </summary>
/// <typeparam name="T">Başarılı durumda dönen değer tipi.</typeparam>
public sealed class Result<T>
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// İşlem başarısız mı?
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Başarılı ise dönen değer.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Başarısız ise hata bilgisi.
    /// </summary>
    public Error Error { get; }

    private Result(bool ok, T? value, Error error)
    {
        IsSuccess = ok;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Başarılı sonucu döndürür.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, Error.None);

    /// <summary>
    /// Başarısız sonucu döndürür.
    /// </summary>
    public static Result<T> Failure(Error error) => new(false, default, error);
}

/// <summary>
/// Değer dönmeyen sonuç tipini temsil eder.
/// Başarı veya hata bilgisini taşır.
/// </summary>
public sealed class Result
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// İşlem başarısız mı?
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Başarısız ise hata bilgisi.
    /// </summary>
    public Error Error { get; }

    private Result(bool ok, Error error)
    {
        IsSuccess = ok;
        Error = error;
    }

    /// <summary>
    /// Başarılı sonucu döndürür.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Başarısız sonucu döndürür.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);
}
