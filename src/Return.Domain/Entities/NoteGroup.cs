// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteGroup.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    using System;
    using Abstractions;

    // These members are mandatory. Let's don't do nullability here for clarity sake.
#nullable disable

    /// <summary>
    /// Represents a collection of grouped notes
    /// </summary>
    public class NoteGroup : IIdPrimaryKey {
        public int Id { get; set; }
        public Retrospective Retrospective { get; set; }
        public NoteLane Lane { get; set; }
        public string Title { get; set; } = String.Empty;
    }
}
