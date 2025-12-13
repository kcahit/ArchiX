namespace ArchiX.Library.Entities
{
    /// <summary>Either ServerName OR Cidr must be provided. EnvScope null => all environments.</summary>
    public class ConnectionServerWhitelist : BaseEntity
    {
        public string? ServerName { get; set; }

        public string? Cidr { get; set; }

        public int? Port { get; set; }

        public string? EnvScope { get; set; }

        public bool IsActive { get; set; }
    }
}
