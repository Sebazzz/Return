// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : TextAnonymizingService.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Services {
    using System;
    using System.Globalization;

    public interface ITextAnonymizingService {
        string AnonymizeText(string input);
    }

    public sealed class TextAnonymizingService : ITextAnonymizingService {
        private readonly Random _random = new Random();

        public string AnonymizeText(string input) {
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Prepare buffer
            Span<char> newText = stackalloc char[input.Length];
            input.AsSpan().CopyTo(newText);

            // Find every word and just mix up the characters
            Span<char> current = newText;
            while (true) {
                int nextSpaceIndex = current.IndexOf(' ');
                if (nextSpaceIndex == -1) {
                    Shuffle(this._random, ref current);
                    break;
                }

                Span<char> part = current[..nextSpaceIndex];
                Shuffle(this._random, ref part);

                int nextIndex = nextSpaceIndex + 1;
                current = current[nextIndex..];
            }

            return new string(newText);
        }

        private static void Shuffle(Random random, ref Span<char> part) {
            if (part.IsEmpty) {
                return;
            }

            bool firstCharIsUpper = Char.IsUpper(part[0]);

            for (int idx = 0; idx < part.Length; idx++) {
                int toIdx = random.Next(0, part.Length);

                (part[idx], part[toIdx]) = (part[toIdx], firstCharIsUpper ? Char.ToLower(part[idx], CultureInfo.CurrentCulture) : part[idx]);
            }

            if (firstCharIsUpper) {
                part[0] = Char.ToUpper(part[0], CultureInfo.CurrentCulture);
            }
        }
    }
}
