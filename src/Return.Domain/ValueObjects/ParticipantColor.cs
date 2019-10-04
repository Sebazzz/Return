// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantColor.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.ValueObjects {
    using System.Collections.Generic;
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

        public string ToHex() => $"{this.R:00}{this.G:00}{this.B:00}";

        protected override IEnumerable<object> GetAtomicValues() {
            yield return this.R;
            yield return this.G;
            yield return this.B;
        }
    }
}
