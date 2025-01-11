using OtpNet;
using System.Text;

namespace Helper.Helpers;

public class OtpService(string secretKey)
{
    public static readonly Dictionary<long, string> OtpStore = [];
    private readonly byte[] _secretKey = Encoding.UTF8.GetBytes(secretKey);

    public string GenerateOtp(int expiryInSeconds = 300)
    {
        try
        {
            var totp = new Totp(_secretKey, step: expiryInSeconds);
            return totp.ComputeTotp();

        }
        catch
        {
            Console.WriteLine($"{_secretKey} - {expiryInSeconds}");
            throw;
        }
    }

    public bool ValidateOtp(string otp, int expiryInSeconds = 300)
    {
        var totp = new Totp(_secretKey, step: expiryInSeconds);
        return totp.VerifyTotp(otp, out _, new(1, 1));
    }
}
