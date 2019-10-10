// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Note.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    using System;
    using Abstractions;

    // These members are mandatory. Let's don't do nullability here for clarity sake.
#nullable disable

    /// <summary>
    /// A note is a note made by a participant in the retrospective. 
    /// </summary>
    public class Note : IOwnedByParticipant, IIdPrimaryKey {
        public int Id { get; set; }

        public Retrospective Retrospective { get; set; }

        public string Text { get; set; }

        public NoteLane Lane { get; set; }

        public Participant Participant { get; set; }
        public int ParticipantId { get; set; }

        public DateTimeOffset CreationTimestamp { get; set; }

#nullable enable
        public NoteGroup? Group { get; set; }
        public int? GroupId { get; set; }
    }
}
