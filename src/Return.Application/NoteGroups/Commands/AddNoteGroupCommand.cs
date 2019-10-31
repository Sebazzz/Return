// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AddNoteGroupCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.NoteGroups.Commands {
    using Common.Models;
    using Domain.Entities;
    using MediatR;

    public sealed class AddNoteGroupCommand : IRequest<RetrospectiveNoteGroup> {
        public string RetroId { get; }
        public int LaneId { get; }

        public AddNoteGroupCommand(string retroId, int laneId) {
            this.RetroId = retroId;
            this.LaneId = laneId;
        }
        public override string ToString() => $"[{nameof(AddNoteGroupCommand)}] {this.RetroId} on lane {(KnownNoteLane)this.LaneId}";
    }
}
