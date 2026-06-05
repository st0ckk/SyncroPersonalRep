using System.Security.Cryptography;
using System.Text;

namespace SyncroBE.Infrastructure.Auth
{
    public class TotpService
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private const int TimeStepSeconds = 30;
        private const int Digits = 6;

        public (string secret, string otpauthUri) GenerateSetup(string email, string issuer)
        {
            var key = RandomNumberGenerator.GetBytes(20);
            var base32Secret = Base32Encode(key);
            var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
                      $"?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";
            return (base32Secret, uri);
        }

        public bool Verify(string base32Secret, string code)
        {
            try
            {
                var key = Base32Decode(base32Secret);
                var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TimeStepSeconds;

                // Validar ventana de ±2 pasos para tolerar desfase de reloj
                for (var offset = -2; offset <= 2; offset++)
                {
                    if (ComputeTotp(key, counter + offset) == code)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static string ComputeTotp(byte[] key, long counter)
        {
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

            using var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counterBytes);

            var offset = hash[^1] & 0x0F;
            var code = ((hash[offset] & 0x7F) << 24)
                     | ((hash[offset + 1] & 0xFF) << 16)
                     | ((hash[offset + 2] & 0xFF) << 8)
                     | (hash[offset + 3] & 0xFF);

            return (code % (int)Math.Pow(10, Digits)).ToString($"D{Digits}");
        }

        private static string Base32Encode(byte[] data)
        {
            var sb = new StringBuilder();
            var bits = 0;
            var buffer = 0;

            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bits += 8;
                while (bits >= 5)
                {
                    sb.Append(Base32Alphabet[(buffer >> (bits - 5)) & 31]);
                    bits -= 5;
                }
            }

            if (bits > 0)
                sb.Append(Base32Alphabet[(buffer << (5 - bits)) & 31]);

            return sb.ToString();
        }

        private static byte[] Base32Decode(string base32)
        {
            base32 = base32.ToUpperInvariant().TrimEnd('=');
            var result = new List<byte>();
            var bits = 0;
            var buffer = 0;

            foreach (var c in base32)
            {
                var val = Base32Alphabet.IndexOf(c);
                if (val < 0) continue;
                buffer = (buffer << 5) | val;
                bits += 5;
                if (bits >= 8)
                {
                    result.Add((byte)(buffer >> (bits - 8)));
                    bits -= 8;
                }
            }

            return result.ToArray();
        }
    }
}
