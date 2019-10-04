// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Note.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    using System;

    // These members are mandatory. Let's don't do nullability here for clarity sake.
#nullable disable

    /// <summary>
    /// A note is a note made by a participant in the retrospective. 
    /// </summary>
    public class Note {
        public int Id { get; set; }

        public Retrospective Retrospective { get; set; }

        public string Text { get; set; }

        public NoteLane Lane { get; set; }

        public Participant Participant { get; set; }

        public DateTimeOffset CreationTimestamp { get; set; }
    }
}
