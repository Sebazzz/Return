// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantUriCookieService.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Services {
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Intrinsics;
    using System.Runtime.Intrinsics.X86;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "This is not meant for strong security.")]
    public class ParticipantUriCookieService : IParticipantUriCookieService {
        private const int Int32Length = sizeof(Int32);
        private const int Int16Length = sizeof(Int16);
        private const int BooleanLength = sizeof(Boolean);
        private const int ChecksumLength = 128 / 8;
        private const short StringNull = -1;
        private const int MaximumStringLength = 256;

        private const int MaxDurationHours = 2;

        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly ISystemClock _systemClock;
        private readonly byte[] _baseMachineKey;
        private readonly ILogger<ParticipantUriCookieService> _logger;

        public ParticipantUriCookieService(ISystemClock systemClock, ILogger<ParticipantUriCookieService> logger) {
            this._systemClock = systemClock;
            this._logger = logger;
            this._baseMachineKey = Encoding.UTF8.GetBytes(Environment.OSVersion.VersionString + Environment.MachineName + Environment.UserName + Environment.Version);

            // The URI cookie contents is protected by a data protection mechanism.
            // Unprotected, the format is:
            // byte[16] checksum | Int32 id | bool isManager | Int16 nameLength | string name
        }

        public unsafe CurrentParticipantModel Decrypt(string uriCookie) {
            if (uriCookie == null) throw new ArgumentNullException(nameof(uriCookie));
            if (String.IsNullOrEmpty(uriCookie)) throw new ArgumentException("Empty uri cookie", nameof(uriCookie));
            if (uriCookie.Length % 2 != 0) return default;

            // Create bytes array
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

            // Validate checksum
            Span<byte> computedHash = stackalloc byte[ChecksumLength];

            bool isValidChecksum = false;
            for (int offset = 0; offset >= MaxDurationHours * -1; offset--) {
                var key = new byte[64];
                this.CreateKey(key, offset);
                using var hmac = new HMACMD5(key);

                try {
                    Span<byte> currentHash = protectedBytes[0..ChecksumLength];
                    if (!hmac.TryComputeHash(protectedBytes[(ChecksumLength + 1)..], computedHash, out int bytesWritten)) {
                        throw new InvalidOperationException("Failed to decrypt cookie: cannot write hash");
                    }

                    if (bytesWritten != ChecksumLength) {
                        throw new InvalidOperationException(
                            $"Failed to decrypt cookie: cannot write hash [{bytesWritten} != {ChecksumLength}]");
                    }

                    isValidChecksum = true;
                    for (int i = 0; i < ChecksumLength; i++) {
                        if (computedHash[i] != currentHash[i]) {
                            Debug.WriteLine($"[{String.Join("|", computedHash.ToArray())}] != [{String.Join("|", currentHash.ToArray())}]");
                            isValidChecksum = false;
                            break;
                        }
                    }
                }
                catch (InvalidOperationException ex) {
                    this._logger.LogError(ex, $"Failed to decrypt cookie [{uriCookie}]");
                    return default;
                }

                if (isValidChecksum) {
                    break;
                }
            }

            if (!isValidChecksum) {
                return default;
            }

            byte[] unprotectedBytes = protectedBytes;

            // .. string length and string
            Span<byte> current = unprotectedBytes[ChecksumLength..];
            int id = BitConverter.ToInt32(current);
            current = current[Int32Length..];

            bool isManager = BitConverter.ToBoolean(current);
            current = current[BooleanLength..];

            int strLength = BitConverter.ToInt16(current);
            current = current[Int16Length..];

            string? str = null;
            if (strLength != StringNull) {
                Span<char> chars = stackalloc char[strLength];
                chars.Fill(Char.MinValue);
                this._encoding.GetChars(current, chars);

                str = chars.ToString();
            }

            return new CurrentParticipantModel(
                id,
                str,
                isManager
            );
        }

        public string Encrypt(CurrentParticipantModel currentParticipant) {
            if (currentParticipant.Name?.Length > MaximumStringLength) {
                throw new ArgumentException("Length of participant name is longer than expected", nameof(currentParticipant));
            }

            // Buffer for creating the unprotected string
            // We stackalloc this array, because we create a large enough one later on.
            Span<byte> buffer = stackalloc byte[MaximumStringLength + Int32Length + BooleanLength + Int16Length + ChecksumLength];

            int pos = ChecksumLength;
            {
                Span<byte> current = buffer[ChecksumLength..];
                FillSpan(BitConverter.GetBytes(currentParticipant.Id), ref current);
                current = current[Int32Length..];
                pos += Int32Length;

                FillSpan(BitConverter.GetBytes(currentParticipant.IsManager), ref current);
                current = current[BooleanLength..];
                pos += BooleanLength;

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
            var key = new byte[64];
            this.CreateKey(key);

            using (var hmac = new HMACMD5(key)) {
                if (!hmac.TryComputeHash(buffer[(ChecksumLength + 1)..], buffer, out int bytesWritten)) {
                    throw new InvalidOperationException("Failed to create protected string");
                }

                if (ChecksumLength != bytesWritten) {
                    throw new InvalidOperationException($"Failed to create protected string [{ChecksumLength} != {bytesWritten}]");
                }
            }

            var protectedBytes = new byte[pos];
            buffer.Slice(0, pos).CopyTo(protectedBytes);

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

        private static void FillSpan(Span<byte> array, ref Span<byte> span) {
            for (int idx = 0; idx < array.Length; idx++) {
                span[idx] = array[idx];
            }
        }

        private void CreateKey(Span<byte> bytes64, int hourOffset = 0) {
            DateTime date = this._systemClock.Now;
            var dateWithHour = new DateTime(date.Year, date.Month, date.Day, date.Hour + hourOffset, 0, 0);

            int minLength = Math.Min(this._baseMachineKey.Length, bytes64.Length);
            FillSpan(this._baseMachineKey[..minLength], ref bytes64);
            FillSpan(BitConverter.GetBytes(dateWithHour.ToFileTimeUtc()), ref bytes64);
        }
    }
}
