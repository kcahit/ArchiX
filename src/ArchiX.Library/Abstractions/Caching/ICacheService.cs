namespace ArchiX.Library.Abstractions.Caching
{
 /// <summary>
 /// Uygulama genelinde kullanýlacak önbellek sözleþmesi.
 /// Senkron temel API ve asenkron yardýmcý API’leri içerir.
 /// </summary>
 public interface ICacheService
 {
 /// <summary>
 /// Anahtardan deðeri okur. Yoksa <see langword="default"/> döner.
 /// </summary>
 /// <typeparam name="T">Dönecek tür.</typeparam>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <returns>Deðer ya da yoksa <see langword="default"/>.</returns>
 T? Get<T>(object key);

 /// <summary>
 /// Anahtara deðeri yazar. Opsiyonel mutlak yaþam süresi tanýmlanabilir.
 /// </summary>
 /// <typeparam name="T">Yazýlacak deðer türü.</typeparam>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <param name="value">Yazýlacak deðer.</param>
 /// <param name="ttl">Mutlak yaþam süresi.</param>
 void Set<T>(object key, T value, TimeSpan? ttl = null);

 /// <summary>
 /// Deðer yoksa <paramref name="factory"/> ile üretir, isteðe baðlý süre ile yazar ve döner.
 /// Deðer varsa önbellekten döner. Üretilen deðer <see langword="null"/> ise yazýlmaz.
 /// </summary>
 /// <typeparam name="T">Dönecek tür.</typeparam>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <param name="factory">Deðer üretici.</param>
 /// <param name="ttl">Mutlak yaþam süresi.</param>
 /// <returns>Önbellekten ya da üreticiden dönen deðer.</returns>
 Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> factory, TimeSpan? ttl = null);

 /// <summary>
 /// Anahtarý ve iliþkili deðeri siler.
 /// </summary>
 /// <param name="key">Önbellek anahtarý.</param>
 void Remove(object key);

 /// <summary>
 /// Asenkron yazma. Mutlak ve/veya kayan süre tanýmlanabilir.
 /// </summary>
 /// <typeparam name="T">Yazýlacak deðer türü.</typeparam>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <param name="value">Yazýlacak deðer.</param>
 /// <param name="absoluteExpiration">Mutlak yaþam süresi.</param>
 /// <param name="slidingExpiration">Kayan yaþam süresi.</param>
 /// <param name="cancellationToken">Ýptal belirteci.</param>
 /// <returns>Tamamlandýðýnda döner.</returns>
 Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

 /// <summary>
 /// Asenkron okuma. Yoksa <see langword="default"/> döner.
 /// </summary>
 /// <typeparam name="T">Dönecek tür.</typeparam>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <param name="cancellationToken">Ýptal belirteci.</param>
 /// <returns>Deðer ya da yoksa <see langword="default"/>.</returns>
 Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

 /// <summary>
 /// Asenkron silme.
 /// </summary>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <param name="cancellationToken">Ýptal belirteci.</param>
 /// <returns>Tamamlandýðýnda döner.</returns>
 Task RemoveAsync(string key, CancellationToken cancellationToken = default);

 /// <summary>
 /// Anahtarýn mevcut olup olmadýðýný kontrol eder.
 /// <para>Not: <see langword="null"/> deðerli giriþler mevcut sayýlmaz.</para>
 /// </summary>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <param name="cancellationToken">Ýptal belirteci.</param>
 /// <returns>Varsa <see langword="true"/>, yoksa <see langword="false"/>.</returns>
 Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

 /// <summary>
 /// Yoksa <paramref name="factory"/> ile üretip yazar, varsa önbellekten döner.
 /// <para><paramref name="cacheNull"/> <see langword="true"/> ise <see langword="null"/> deðerler de cache’lenir.</para>
 /// </summary>
 /// <typeparam name="T">Dönecek tür.</typeparam>
 /// <param name="key">Önbellek anahtarý.</param>
 /// <param name="factory">Deðer üretici.</param>
 /// <param name="absoluteExpiration">Mutlak yaþam süresi.</param>
 /// <param name="slidingExpiration">Kayan yaþam süresi.</param>
 /// <param name="cacheNull"><see langword="null"/> deðerlerin cache’lenip cache’lenmeyeceði.</param>
 /// <param name="ct">Ýptal belirteci.</param>
 /// <returns>Önbellekten ya da üreticiden dönen deðer.</returns>
 Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, bool cacheNull = false, CancellationToken ct = default);
 }
}
