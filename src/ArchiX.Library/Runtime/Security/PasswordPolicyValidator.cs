using System.Text.RegularExpressions;
using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Runtime.Security;

internal static partial class PasswordPolicyValidator
{
    public static IReadOnlyList<string> Validate(string password, PasswordPolicyOptions policy)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(password)) { errors.Add("EMPTY"); return errors; }
        if (password.Length < policy.MinLength) errors.Add("MIN_LENGTH");
        if (password.Length > policy.MaxLength) errors.Add("MAX_LENGTH");
        if (policy.RequireUpper && !HasUpper(password)) errors.Add("REQ_UPPER");
        if (policy.RequireLower && !HasLower(password)) errors.Add("REQ_LOWER");
        if (policy.RequireDigit && !HasDigit(password)) errors.Add("REQ_DIGIT");
        if (policy.RequireSymbol && !HasSymbol(password, policy)) errors.Add("REQ_SYMBOL");
        if (policy.MinDistinctChars > 0 && DistinctCount(password) < policy.MinDistinctChars) errors.Add("MIN_DISTINCT");
        if (policy.MaxRepeatedSequence > 0 && HasRepeatedSequence(password, policy.MaxRepeatedSequence)) errors.Add("REPEAT_SEQ");
        if (policy.BlockList.Length > 0 && policy.BlockList.Any(b => password.Contains(b, StringComparison.OrdinalIgnoreCase))) errors.Add("BLOCK_LIST");
        return errors;
    }

    private static bool HasUpper(string s) => s.Any(char.IsUpper);
    private static bool HasLower(string s) => s.Any(char.IsLower);
    private static bool HasDigit(string s) => s.Any(char.IsDigit);
    private static bool HasSymbol(string s, PasswordPolicyOptions p)
    {
        foreach (var ch in s)
        {
            if (!char.IsLetterOrDigit(ch))
            {
                if (string.IsNullOrEmpty(p.AllowedSymbols) || p.AllowedSymbols.Contains(ch)) return true;
            }
        }
        return false;
    }

    private static int DistinctCount(string s)
    {
        Span<int> counts = stackalloc int[256];
        foreach (var c in s)
        {
            if (c < 256) counts[c] = 1;
        }
        var total = 0;
        foreach (var v in counts) if (v == 1) total++;
        return total;
    }

    private static bool HasRepeatedSequence(string s, int maxRepeat)
    {
        // Simple regex for repeated char sequences longer than maxRepeat
        var pattern = $"(.)\\1{{{maxRepeat},}}"; // char repeated >= maxRepeat+1 times
        return Regex.IsMatch(s, pattern);
    }
}
