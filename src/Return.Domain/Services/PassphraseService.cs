using System;
using System.Text;

namespace Return.Domain.Services {
    using System.Security.Cryptography;
    using Return.Common;

    public interface IPassphraseService {
        bool ValidatePassphrase(string passphrase, string hashed);

        string CreateHashedPassphrase(string passphrase);
    }

    public sealed class PassphraseService : IPassphraseService {
        public bool ValidatePassphrase(string passphrase, string hashedPassphrase) {
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));
            if (hashedPassphrase == null) throw new ArgumentNullException(nameof(hashedPassphrase));
            if (String.IsNullOrEmpty(passphrase)) throw new ArgumentException("Empty passphrase not allowed", nameof(passphrase));

            return String.Equals(
                this.CreateHashedPassphrase(passphrase),
                hashedPassphrase,
                StringComparison.OrdinalIgnoreCase
            );
        }

        public string CreateHashedPassphrase(string passphrase) {
            if (passphrase == null) throw new ArgumentNullException(nameof(passphrase));
            if (String.IsNullOrEmpty(passphrase)) throw new ArgumentException("Empty passphrase not allowed", nameof(passphrase));

            using var sha256 = new SHA256Managed();

            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) {
                sb.AppendFormat(Culture.Invariant, "{0:x2}", b);
            }

            return sb.ToString();
        }
    }
}
