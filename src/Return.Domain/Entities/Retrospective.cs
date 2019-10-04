// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Retrospective.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Services;
    using ValueObjects;

    /// <summary>
    /// A retrospective consists of notes created by participants. A retrospective has a unique identifier.
    /// </summary>
    public class Retrospective {
        private ICollection<Note>? _notes;
        private string? _id;

        /// <summary>
        /// Identifier (random string) of the retrospective
        /// </summary>
        /// <remarks>
        /// Note: I would actually like to use <see cref="RetroIdentifier"/> for this, but EF does not allow an owned type to act as PK.
        /// </remarks>
        public string Id => this._id ??= RetroIdentifierService.CreateNewInternal().StringId;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Title { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ICollection<Note> Notes => this._notes ??= new Collection<Note>();

        public DateTimeOffset CreationTimestamp { get; set; }
    }
}
