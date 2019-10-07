// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Participant.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    using ValueObjects;

#nullable disable
    public class Participant {
        public int Id { get; set; }

        public ParticipantColor Color { get; set; }

        public Retrospective Retrospective { get; set; }

        public string Name { get; set; }

        public bool IsManager { get; set; }
    }
}
