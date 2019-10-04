// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetroIdentifierService.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Services {
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using ValueObjects;

    public interface IRetroIdentifierService {
        bool IsValid(string str);

        /// <summary>
        ///     Creates a new log message id
        /// </summary>
        /// <returns></returns>
        RetroIdentifier CreateNew();
    }

    /// <summary>
    ///     Represents the identifier of an uploaded file
    /// </summary>
    public sealed class RetroIdentifierService : IRetroIdentifierService {
        private static int _mask = ~1;

        private static readonly char[] Chars =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public bool IsValid(string str) {
            if (str == null) throw new ArgumentNullException(nameof(str));

            foreach (char ch in str) {
                if (Array.IndexOf(array: Chars, value: ch) == -1) {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        ///     Creates a new log message id
        /// </summary>
        /// <returns></returns>
        public RetroIdentifier CreateNew() => CreateNewInternal();

        internal static RetroIdentifier CreateNewInternal() {
            Interlocked.Increment(location: ref _mask);
            return new RetroIdentifier(MakeStringRepresentation(Guid.NewGuid()));
        }

        private static unsafe string MakeStringRepresentation(Guid id) {
            // Convert Guid to bytes
            var buffer = new GuidBuffer(guid: id);
            byte* bytes = buffer.buffer;

            // Target string
            int size = sizeof(Guid) * 2;
            char* result = stackalloc char[size + 1 /* \0 terminator */];
            char* start = result;

            for (int i = 0; i < sizeof(Guid); i++) {
                byte src = *bytes;
                int carry = 0;

                {
                    int index = src % Chars.Length;

                    char current = Chars[index];

                    *result = current;
                    result++;

                    carry = (src - index) / Chars.Length - 1;
                }

                if (carry > 0) {
                    int index = carry % Chars.Length;

                    char current = Chars[index];

                    *result = current;
                    result++;
                }

                bytes++;
            }

            return new string(value: start);
        }

        [StructLayout(layoutKind: LayoutKind.Explicit)]
        private unsafe struct GuidBuffer {
            [FieldOffset(offset: 0)] public fixed byte buffer[16];

            [FieldOffset(offset: 0)] private readonly Guid Guid;

            public GuidBuffer(Guid guid) : this() {
                this.Guid = guid;
            }
        }
    }
}
