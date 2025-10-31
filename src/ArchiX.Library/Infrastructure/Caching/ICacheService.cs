namespace ArchiX.Library.Infrastructure.Caching
{
    /// <summary>
    /// Uygulama genelinde kullanılacak önbellek sözleşmesi.
    /// Senkron temel API ve asenkron yardımcı API’leri içerir.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Anahtardan değeri okur. Yoksa <see langword="default"/> döner.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <returns>Değer ya da yoksa <see langword="default"/>.</returns>
        T? Get<T>(object key);

        /// <summary>
        /// Anahtara değeri yazar. Opsiyonel mutlak yaşam süresi tanımlanabilir.
        /// </summary>
        /// <typeparam name="T">Yazılacak değer türü.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="value">Yazılacak değer.</param>
        /// <param name="ttl">Mutlak yaşam süresi.</param>
        void Set<T>(object key, T value, TimeSpan? ttl = null);

        /// <summary>
        /// Değer yoksa <paramref name="factory"/> ile üretir, isteğe bağlı süre ile yazar ve döner.
        /// Değer varsa önbellekten döner. Üretilen değer <see langword="null"/> ise yazılmaz.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="factory">Değer üretici.</param>
        /// <param name="ttl">Mutlak yaşam süresi.</param>
        /// <returns>Önbellekten ya da üreticiden dönen değer.</returns>
        Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory, TimeSpan? ttl = null);

        /// <summary>
        /// Anahtarı ve ilişkili değeri siler.
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        void Remove(object key);

        /// <summary>
        /// Asenkron yazma. Mutlak ve/veya kayan süre tanımlanabilir.
        /// </summary>
        /// <typeparam name="T">Yazılacak değer türü.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="value">Yazılacak değer.</param>
        /// <param name="absoluteExpiration">Mutlak yaşam süresi.</param>
        /// <param name="slidingExpiration">Kayan yaşam süresi.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Tamamlandığında döner.</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asenkron okuma. Yoksa <see langword="default"/> döner.
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Değer ya da yoksa <see langword="default"/>.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asenkron silme.
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Tamamlandığında döner.</returns>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Anahtarın mevcut olup olmadığını kontrol eder.
        /// <para>Not: <see langword="null"/> değerli girişler mevcut sayılmaz.</para>
        /// </summary>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Varsa <see langword="true"/>, yoksa <see langword="false"/>.</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Yoksa <paramref name="factory"/> ile üretip yazar, varsa önbellekten döner.
        /// <para><paramref name="cacheNull"/> <see langword="true"/> ise <see langword="null"/> değerler de cache’lenir.</para>
        /// </summary>
        /// <typeparam name="T">Dönecek tür.</typeparam>
        /// <param name="key">Önbellek anahtarı.</param>
        /// <param name="factory">Değer üretici.</param>
        /// <param name="absoluteExpiration">Mutlak yaşam süresi.</param>
        /// <param name="slidingExpiration">Kayan yaşam süresi.</param>
        /// <param name="cacheNull"><see langword="null"/> değerlerin cache’lenip cache’lenmeyeceği.</param>
        /// <param name="ct">İptal belirteci.</param>
        /// <returns>Önbellekten ya da üreticiden dönen değer.</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, bool cacheNull = false, CancellationToken ct = default);
    }
}
