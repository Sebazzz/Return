// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantUriCookieService.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using Application.Common.Models;
    using Common;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Logging;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "FxCop is trigger-happy. This is about data protection in the URI.")]
    public interface IParticipantUriCookieService {
        CurrentParticipantModel Decrypt(string uriCookie);
        string Encrypt(CurrentParticipantModel currentParticipant);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "FxCop is trigger-happy. This is about data protection in the URI.")]
    public class ParticipantUriCookieService : IParticipantUriCookieService {
        private const int Int32Length = sizeof(Int32);
        private const int Int16Length = sizeof(Int16);
        private const int BooleanLength = sizeof(Boolean);
        private const short StringNull = -1;
        private const int MaximumStringLength = 256;
        private const int MaxDurationHours = 2;
        private readonly ITimeLimitedDataProtector _dataProtector;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly ILogger<ParticipantUriCookieService> _logger;

        public ParticipantUriCookieService(IDataProtectionProvider dataProtectionProvider, ILogger<ParticipantUriCookieService> logger) {
            if (dataProtectionProvider == null) throw new ArgumentNullException(nameof(dataProtectionProvider));
            this._logger = logger;

            // The URI cookie contents is protected by a data protection mechanism.
            // Unprotected, the format is:
            // Int32 id | bool isManager | string name
            this._dataProtector = dataProtectionProvider.CreateProtector(nameof(ParticipantUriCookieService)).ToTimeLimitedDataProtector();
        }

        public unsafe CurrentParticipantModel Decrypt(string uriCookie) {
            if (uriCookie == null) throw new ArgumentNullException(nameof(uriCookie));
            if (String.IsNullOrEmpty(uriCookie)) throw new ArgumentException("Empty uri cookie", nameof(uriCookie));
            if (uriCookie.Length % 2 != 0) return default;

            var protectedBytes = new byte[uriCookie.Length / 2];

            try {
                int ptr = 0;
                for (int i = 0; i < uriCookie.Length; i += 2) {
                    protectedBytes[ptr++] = Byte.Parse(uriCookie[i..(i + 2)],
                        NumberStyles.AllowHexSpecifier,
                        Culture.Invariant);
                }
            }
            catch (FormatException ex) {
                this._logger.LogError(ex, $"Failed to decrypt cookie [{uriCookie}]");
                return default;
            }

            byte[] unprotectedBytes;

            try {
                unprotectedBytes = this._dataProtector.Unprotect(protectedBytes);
            }
            catch (CryptographicException ex) {
                this._logger.LogError(ex, $"Failed to decrypt cookie [{uriCookie}]");
                return default;
            }

            // Decrypt

            // .. string length and string
            int strLength = BitConverter.ToInt16(unprotectedBytes[(Int32Length + BooleanLength)..(Int32Length + BooleanLength + Int16Length)]);

            string? str = null;
            if (strLength != StringNull) {
                Span<char> chars = stackalloc char[strLength];
                chars.Fill(Char.MinValue);
                this._encoding.GetChars(unprotectedBytes[(Int32Length + BooleanLength + Int16Length)..], chars);

                str = chars.ToString();
            }

            return new CurrentParticipantModel(
                BitConverter.ToInt32(unprotectedBytes[0..Int32Length]),
                str,
                BitConverter.ToBoolean(unprotectedBytes[Int32Length..(Int32Length + BooleanLength)])
            );
        }

        public string Encrypt(CurrentParticipantModel currentParticipant) {
            if (currentParticipant.Name?.Length > MaximumStringLength) {
                throw new ArgumentException("Length of participant name is longer than expected", nameof(currentParticipant));
            }

            // Buffer for creating the unprotected string
            // We stackalloc this array, because we create a large enough one later on.
            Span<byte> unprotectedBytes = stackalloc byte[MaximumStringLength + Int32Length + BooleanLength + Int16Length];

            int pos;
            {
                FillSpan(BitConverter.GetBytes(currentParticipant.Id), ref unprotectedBytes);
                Span<byte> current = unprotectedBytes[4..];
                // pos = 4

                FillSpan(BitConverter.GetBytes(currentParticipant.IsManager), ref current);
                current = current[1..];
                // pos = 1 + 4 = 5
                pos = Int32Length + BooleanLength;

                if (currentParticipant.Name != null) {
                    // pos = 5 + 2 = 7
                    FillSpan(BitConverter.GetBytes((short)currentParticipant.Name.Length), ref current);
                    current = current[2..];
                    pos += Int16Length;

                    int bytes = this._encoding.GetBytes(currentParticipant.Name, current);
                    pos += bytes;
                }
                else {
                    // pos = 5 + 2 = 7
                    FillSpan(BitConverter.GetBytes(StringNull), ref current);
                    pos += Int16Length;
                }
            }

            // Protect the array
            var unprotectedByteArray = new byte[pos];
            unprotectedBytes.Slice(0, pos).CopyTo(unprotectedByteArray);

            byte[] protectedBytes = this._dataProtector.Protect(unprotectedByteArray, DateTimeOffset.Now.AddHours(MaxDurationHours));

            // ... Convert to string cookie
            Span<char> uriCookie = stackalloc char[protectedBytes.Length * 2];
            Span<char> uriCookiePtr = uriCookie;
            foreach (byte b in protectedBytes) {
                if (!b.TryFormat(uriCookiePtr, out int charsWritten, "X2", Culture.Invariant)) {
                    throw new InvalidOperationException("Failed to create protected string");
                }

                uriCookiePtr = uriCookiePtr[charsWritten..];
            }

            return uriCookie.ToString();
        }

        private static void FillSpan(byte[] array, ref Span<byte> span) {
            for (int idx = 0; idx < array.Length; idx++) {
                span[idx] = array[idx];
            }
        }
    }
}
