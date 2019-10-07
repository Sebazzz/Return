// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantColor.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.ValueObjects {
    using System.Collections.Generic;
    using System.Drawing;
    using Common;

    public class ParticipantColor : ValueObject {
        public ParticipantColor() {
        }

        public ParticipantColor(byte r, byte g, byte b) {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static implicit operator ParticipantColor(Color color) => FromColor(color);
        public static ParticipantColor FromColor(Color color) => new ParticipantColor(color.R, color.G, color.B);

        public string ToHex() => $"{this.R:X2}{this.G:X2}{this.B:X2}";

        protected override IEnumerable<object> GetAtomicValues() {
            yield return this.R;
            yield return this.G;
            yield return this.B;
        }
    }
}
