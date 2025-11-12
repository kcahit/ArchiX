using ArchiX.Library.Abstractions.ConnectionPolicy;

namespace ArchiX.Library.Runtime.ConnectionPolicy
{
    internal sealed class ConnectionPolicyEvaluator : IConnectionPolicyEvaluator
    {
        private readonly IConnectionPolicyProvider _provider;
        private readonly IConnectionPolicyAuditor _auditor;

        public ConnectionPolicyEvaluator(IConnectionPolicyProvider provider)
        {
            _provider = provider;
            _auditor = new NoOpAuditor();
        }

        public ConnectionPolicyEvaluator(IConnectionPolicyProvider provider, IConnectionPolicyAuditor auditor)
        {
            _provider = provider;
            _auditor = auditor;
        }

        public ConnectionPolicyResult Evaluate(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is null/empty.", nameof(connectionString));

            var options = _provider.Current;
            var mode = NormalizeMode(options.Mode);

            if (mode == "Off")
            {
                var offInfo = Parse(connectionString);
                return Return(connectionString, new ConnectionPolicyResult("Off", "Allowed", null, offInfo.NormalizedServer));
            }

            var info = Parse(connectionString);

            if (options.RequireEncrypt && info.Encrypt != true)
                return Return(connectionString, Decide(mode, ConnectionPolicyReasonCodes.ENCRYPT_REQUIRED, info));

            if (options.ForbidTrustServerCertificate && info.TrustServerCertificate == true)
                return Return(connectionString, Decide(mode, ConnectionPolicyReasonCodes.TRUST_CERT_FORBIDDEN, info));

            if (!options.AllowIntegratedSecurity && info.IntegratedSecurity == true)
                return Return(connectionString, Decide(mode, ConnectionPolicyReasonCodes.FORBIDDEN_INTEGRATED_SECURITY, info));

            if (options.IsWhitelistEmpty)
            {
                return Return(connectionString, Decide(mode, ConnectionPolicyReasonCodes.WHITELIST_EMPTY, info));
            }

            if (!IsWhitelisted(info, options))
            {
                return Return(connectionString, Decide(mode, ConnectionPolicyReasonCodes.SERVER_NOT_WHITELISTED, info));
            }

            return Return(connectionString, new ConnectionPolicyResult(mode, "Allowed", null, info.NormalizedServer));
        }

        private ConnectionPolicyResult Return(string raw, ConnectionPolicyResult result)
        {
            _auditor.TryWrite(raw, result);
            return result;
        }

        private static ConnectionPolicyResult Decide(string mode, string reason, ConnInfo info)
        {
            var result = mode == "Warn" ? "Warn" : "Blocked";
            return new ConnectionPolicyResult(mode, result, reason, info.NormalizedServer);
        }

        private static string NormalizeMode(string? raw) =>
            raw?.Trim() switch
            {
                "Off" => "Off",
                "Warn" => "Warn",
                "Enforce" => "Enforce",
                _ => "Warn"
            };

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

            var serverRaw = GetFirst(kv, ["Server", "Data Source", "Addr", "Address", "Network Address"]) ?? "unknown";
            string host = serverRaw;
            int? port = null;

            if (serverRaw.Length > 0 && serverRaw[0] == '[')
            {
                var close = serverRaw.IndexOf(']');
                if (close > 0)
                {
                    var inside = serverRaw[1..close].Trim();
                    host = inside;
                    var rest = serverRaw[(close + 1)..].TrimStart();
                    if (rest.Length > 0 && (rest[0] == ':' || rest[0] == ','))
                    {
                        var pstr = rest[1..].Trim();
                        if (int.TryParse(pstr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p))
                            port = p;
                    }
                }
            }
            else
            {
                var bsIdx = serverRaw.IndexOf('\\');
                if (bsIdx > 0)
                {
                    var rootHost = serverRaw[..bsIdx].Trim();
                    var instance = serverRaw[(bsIdx + 1)..].Trim();

                    var delimIdx = instance.IndexOfAny([',', ':']);
                    if (delimIdx >= 0)
                    {
                        var after = instance[(delimIdx + 1)..].Trim();
                        if (int.TryParse(after, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p))
                            port = p;
                        instance = instance[..delimIdx].Trim();
                    }
                    host = $"{rootHost}\\{instance}";
                }
                else if (serverRaw.Contains(',', StringComparison.Ordinal))
                {
                    var ss = serverRaw.Split(',', 2);
                    host = ss[0].Trim();
                    if (int.TryParse(ss[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p)) port = p;
                }
                else if (serverRaw.Contains(':', StringComparison.Ordinal))
                {
                    var ss = serverRaw.Split(':', 2);
                    host = ss[0].Trim();
                    if (int.TryParse(ss[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var p)) port = p;
                }
            }

            bool? encrypt = TryBool(GetFirst(kv, ["Encrypt"]));
            bool? trust = TryBool(GetFirst(kv, ["TrustServerCertificate"]));
            bool? integrated = TryBool(GetFirst(kv, ["Integrated Security", "Trusted_Connection"]));

            string normalizedHost = host;
            if (System.Net.IPAddress.TryParse(host, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                normalizedHost = $"[{host}]";
            }
            var normalizedServer = port.HasValue ? $"{normalizedHost}:{port}" : normalizedHost;

            var baseHost = host.Contains('\\') ? host.Split('\\', 2)[0] : host;

            return new ConnInfo(normalizedServer, host, baseHost, port, encrypt, trust, integrated);
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
            if (string.Equals(raw, "yes", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(raw, "no", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase)) return false;
            return null;
        }

        private static bool IsWhitelisted(ConnInfo info, ConnectionPolicyOptions options) =>
            MatchAllowedHosts(info, options.AllowedHosts) || MatchAllowedCidrs(info, options.AllowedCidrs);

        private static bool MatchAllowedHosts(ConnInfo info, string[] allowedHosts)
        {
            if (allowedHosts.Length == 0) return false;

            foreach (var raw in allowedHosts)
            {
                var entry = raw?.Trim();
                if (string.IsNullOrEmpty(entry)) continue;

                if (entry.Contains(':', StringComparison.Ordinal))
                {
                    if (string.Equals(info.NormalizedServer, entry, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                else if (entry.Contains('\\', StringComparison.Ordinal))
                {
                    if (string.Equals(info.Host, entry, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                else
                {
                    if (string.Equals(info.BaseHost, entry, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        private static bool MatchAllowedCidrs(ConnInfo info, string[] allowedCidrs)
        {
            if (allowedCidrs.Length == 0) return false;
            if (!System.Net.IPAddress.TryParse(info.BaseHost, out var ip))
                return false;

            foreach (var raw in allowedCidrs)
            {
                var entry = raw?.Trim();
                if (string.IsNullOrEmpty(entry)) continue;
                if (IsIpInCidr(ip, entry)) return true;
            }
            return false;
        }

        private static bool IsIpInCidr(System.Net.IPAddress ip, string cidrOrIp)
        {
            if (!cidrOrIp.Contains('/', StringComparison.Ordinal))
            {
                if (System.Net.IPAddress.TryParse(cidrOrIp, out var exact))
                    return ip.Equals(exact);
                return false;
            }

            var parts = cidrOrIp.Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) return false;
            if (!System.Net.IPAddress.TryParse(parts[0], out var prefix)) return false;
            if (!int.TryParse(parts[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var maskLen)) return false;
            if (ip.AddressFamily != prefix.AddressFamily) return false;

            var ipBytes = ip.GetAddressBytes();
            var prefixBytes = prefix.GetAddressBytes();
            var totalBits = ipBytes.Length * 8;

            if (maskLen < 0 || maskLen > totalBits) return false;

            var fullBytes = maskLen / 8;
            var remainingBits = maskLen % 8;

            for (int i = 0; i < fullBytes; i++)
                if (ipBytes[i] != prefixBytes[i]) return false;

            if (remainingBits == 0) return true;

            byte mask = (byte)~(0xFF >> remainingBits);
            return (ipBytes[fullBytes] & mask) == (prefixBytes[fullBytes] & mask);
        }

        private sealed class NoOpAuditor : IConnectionPolicyAuditor
        {
            public void TryWrite(string rawConnectionString, ConnectionPolicyResult result) { }
        }

        private readonly record struct ConnInfo(
            string NormalizedServer,
            string Host,
            string BaseHost,
            int? Port,
            bool? Encrypt,
            bool? TrustServerCertificate,
            bool? IntegratedSecurity
        );
    }
}