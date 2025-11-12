using ArchiX.Library.Abstractions.ConnectionPolicy;

namespace ArchiX.Library.Runtime.ConnectionPolicy
{
    internal sealed class ConnectionPolicyEvaluator : IConnectionPolicyEvaluator
    {
        private readonly IConnectionPolicyProvider _provider;

        public ConnectionPolicyEvaluator(IConnectionPolicyProvider provider)
        {
            _provider = provider;
        }

        public ConnectionPolicyResult Evaluate(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is null/empty.", nameof(connectionString));

            var options = _provider.Current;
            var mode = NormalizeMode(options.Mode);

            // Mode = Off → her şeyi geçir
            if (mode == "Off")
            {
                var offInfo = Parse(connectionString);
                return new ConnectionPolicyResult("Off", "Allowed", null, offInfo.NormalizedServer);
            }

            // Bayrak kontrolleri (C-1 kapsamı)
            var info = Parse(connectionString);

            // RequireEncrypt
            if (options.RequireEncrypt && info.Encrypt != true)
                return Decide(mode, ConnectionPolicyReasonCodes.ENCRYPT_REQUIRED, info);

            // ForbidTrustServerCertificate
            if (options.ForbidTrustServerCertificate && info.TrustServerCertificate == true)
                return Decide(mode, ConnectionPolicyReasonCodes.TRUST_CERT_FORBIDDEN, info);

            // IntegratedSecurity
            if (!options.AllowIntegratedSecurity && info.IntegratedSecurity == true)
                return Decide(mode, ConnectionPolicyReasonCodes.FORBIDDEN_INTEGRATED_SECURITY, info);

            // C-1: whitelist henüz yok → başarılı
            return new ConnectionPolicyResult(mode, "Allowed", null, info.NormalizedServer);
        }

        private static ConnectionPolicyResult Decide(string mode, string reason, ConnInfo info)
        {
            var result = mode == "Warn" ? "Warn" : "Blocked";
            return new ConnectionPolicyResult(mode, result, reason, info.NormalizedServer);
        }

        private static string NormalizeMode(string? raw)
        {
            return raw?.Trim() switch
            {
                "Off" => "Off",
                "Warn" => "Warn",
                "Enforce" => "Enforce",
                _ => "Warn"
            };
        }

        // Basit parser: anahtarları case-insensitive okur, Server/Encrypt/Trust/IntegratedSecurity’yi çıkarır.
        private static ConnInfo Parse(string cs)
        {
            var kv = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in cs.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var idx = part.IndexOf('=', StringComparison.Ordinal);
                if (idx <= 0) continue;
                var key = part.AsSpan(0, idx).Trim().ToString();
                var val = part[(idx + 1)..].Trim();
                kv[key] = val;
            }

            // Server / Data Source
            var server = GetFirst(kv, ["Server", "Data Source", "Addr", "Address", "Network Address"]) ?? "unknown";
            string host = server;
            int? port = null;

            // Server=host,port veya host:port
            if (server.Contains(',', StringComparison.Ordinal))
            {
                var ss = server.Split(',', 2);
                host = ss[0].Trim();
                if (int.TryParse(ss[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p)) port = p;
            }
            else if (server.Contains(':', StringComparison.Ordinal))
            {
                var ss = server.Split(':', 2);
                host = ss[0].Trim();
                if (int.TryParse(ss[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p)) port = p;
            }

            // Flags
            bool? encrypt = TryBool(GetFirst(kv, ["Encrypt"]));
            bool? trust = TryBool(GetFirst(kv, ["TrustServerCertificate"]));
            bool? integrated = TryBool(GetFirst(kv, ["Integrated Security", "Trusted_Connection"]));

            var normalizedServer = port.HasValue ? $"{host}:{port}" : host;
            return new ConnInfo(normalizedServer, encrypt, trust, integrated);
        }

        private static string? GetFirst(Dictionary<string, string> kv, ReadOnlySpan<string> keys)
        {
            foreach (var k in keys)
                if (kv.TryGetValue(k, out var v))
                    return v;
            return null;
        }

        private static bool? TryBool(string? raw)
        {
            if (raw is null) return null;
            if (bool.TryParse(raw, out var b)) return b;
            // yes/no, 1/0
            if (string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(raw, "no", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase)) return false;
            return null;
        }

        private readonly record struct ConnInfo(
            string NormalizedServer,
            bool? Encrypt,
            bool? TrustServerCertificate,
            bool? IntegratedSecurity
        );
    }
}