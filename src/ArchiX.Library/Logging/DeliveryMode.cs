namespace ArchiX.Library.Logging
{
    /// <summary>
    /// Logların hangi ortama yazılacağını belirten modları temsil eder.
    /// </summary>
    public enum DeliveryMode
    {
        /// <summary>
        /// Sadece veritabanına yaz.
        /// </summary>
        DbOnly = 1,

        /// <summary>
        /// Sadece JSON dosyasına yaz.
        /// </summary>
        JsonOnly = 2,

        /// <summary>
        /// Hem veritabanına hem JSON dosyasına yaz.
        /// </summary>
        DbAndJson = 3
    }
}
