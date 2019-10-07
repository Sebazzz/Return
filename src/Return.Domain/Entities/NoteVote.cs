// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteVote.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities
{
    // These members are mandatory. Let's don't do nullability here for clarity sake.
#nullable disable
    public class NoteVote
    {
        public int Id { get; set; }
        public Note Note { get; set; }
        public Participant Participant { get; set; }
        public int VoteCount { get; set; } = 1;
    }
}
