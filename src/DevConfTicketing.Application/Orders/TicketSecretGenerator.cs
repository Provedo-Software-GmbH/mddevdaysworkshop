using System.Security.Cryptography;

namespace DevConfTicketing.Application.Orders;

public static class TicketSecretGenerator
{
    public static string Generate(int lengthInBytes = 32) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(lengthInBytes));
}
