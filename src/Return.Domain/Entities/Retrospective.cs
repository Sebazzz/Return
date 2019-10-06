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
    using System.Diagnostics.CodeAnalysis;
    using Services;
    using ValueObjects;

    /// <summary>
    /// A retrospective consists of notes created by participants. A retrospective has a unique identifier.
    /// </summary>
    public class Retrospective {
        private ICollection<Note>? _notes;
        private RetroIdentifier? _urlId;

        public int Id { get; set; }

        /// <summary>
        /// Identifier (random string) of the retrospective
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public RetroIdentifier UrlId {
            get => this._urlId ??= RetroIdentifierService.CreateNewInternal();
            set => this._urlId = value;
        }

        /// <summary>
        /// Gets the optional hashed passphrase necessary to access the retrospective lobby
        /// </summary>
        public string? HashedPassphrase { get; set; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Title { get; set; }

        /// <summary>
        /// Gets the passphrase used for the manager to log into the retrospective lobby
        /// </summary>
        public string ManagerHashedPassphrase { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ICollection<Note> Notes => this._notes ??= new Collection<Note>();

        public DateTimeOffset CreationTimestamp { get; set; }
    }
}
