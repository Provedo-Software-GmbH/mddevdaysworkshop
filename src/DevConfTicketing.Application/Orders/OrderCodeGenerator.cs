using System.Security.Cryptography;

namespace DevConfTicketing.Application.Orders;

public static class OrderCodeGenerator
{
    private static readonly char[] Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public static string Generate(string prefix = "MDDD")
    {
        Span<char> code = stackalloc char[4];
        for (var i = 0; i < code.Length; i++)
        {
            code[i] = Chars[RandomNumberGenerator.GetInt32(Chars.Length)];
        }

        return $"{prefix}-{new string(code)}";
    }
}
