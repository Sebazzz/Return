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
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore")]
    public class Retrospective {
        private ICollection<Note>? _notes;
        private ICollection<NoteGroup>? _noteGroups;
        private ICollection<NoteVote>? _noteVotes;
        private ICollection<Participant>? _participants;

        private readonly RetroIdentifier _urlId = RetroIdentifierService.CreateNewInternal();
        private RetrospectiveOptions? _options;
        private RetrospectiveWorkflowData? _workflowData;

        public int Id { get; set; }

        /// <summary>
        /// Identifier (random string) of the retrospective
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public RetroIdentifier UrlId => this._urlId;

        /// <summary>
        /// Gets or sets the current stage of the retrospective
        /// </summary>
        public RetrospectiveStage CurrentStage { get; set; }

        /// <summary>
        /// Gets the optional hashed passphrase necessary to access the retrospective lobby
        /// </summary>
        public string? HashedPassphrase { get; set; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Title { get; set; }

        /// <summary>
        /// Gets the passphrase used for the facilitator to log into the retrospective lobby
        /// </summary>
        public string FacilitatorHashedPassphrase { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public ICollection<Note> Notes => this._notes ??= new Collection<Note>();
        public ICollection<Participant> Participants => this._participants ??= new Collection<Participant>();
        public ICollection<NoteGroup> NoteGroup => this._noteGroups ??= new Collection<NoteGroup>();
        public ICollection<NoteVote> NoteVotes => this._noteVotes ??= new Collection<NoteVote>();


        public DateTimeOffset CreationTimestamp { get; set; }

        public RetrospectiveOptions Options {
            get => this._options ??= new RetrospectiveOptions();
            set => this._options = value;
        }

        public RetrospectiveWorkflowData WorkflowData {
            get => this._workflowData ??= new RetrospectiveWorkflowData();
            set => this._workflowData = value;
        }
    }

    public class RetrospectiveOptions {
        public int MaximumNumberOfVotes { get; set; } = 5;
    }

    public class RetrospectiveWorkflowData {
        public DateTimeOffset CurrentWorkflowInitiationTimestamp { get; set; }

        public int CurrentWorkflowTimeLimitInMinutes { get; set; }
    }
}
