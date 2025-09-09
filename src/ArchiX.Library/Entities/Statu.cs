namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Sistemdeki kayıtların durumlarını temsil eden entity.
    /// </summary>
    public class Statu : BaseEntity
    {
        /// <summary>
        /// Durum kodu (örn: "APR", "DEL").
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Durum adı (örn: "Approved", "Deleted").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Durum açıklaması.
        /// </summary>
        public string? Description { get; set; }
    }
}
