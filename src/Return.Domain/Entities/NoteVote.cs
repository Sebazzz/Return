// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteVote.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    using Abstractions;

    // These members are mandatory. Let's don't do nullability here for clarity sake.
#nullable disable
    public class NoteVote : IOwnedByParticipant {
        public int Id { get; set; }
        public Note Note { get; set; }
        public NoteGroup NoteGroup { get; set; }
        public Retrospective Retrospective { get; set; }
        public Participant Participant { get; set; }
        public int ParticipantId { get; set; }
    }
}
