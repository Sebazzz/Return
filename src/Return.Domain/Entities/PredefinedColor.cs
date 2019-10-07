// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PredefinedParticipantColor.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using ValueObjects;

    public class PredefinedParticipantColor {
        public PredefinedParticipantColor(string name) {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Color = new ParticipantColor();
        }

        public PredefinedParticipantColor(string name, ParticipantColor participantColor) {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Color = participantColor;
        }

        public int Id { get; set; }

        public string Name { get; }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public ParticipantColor Color { get; set; }
    }
}
