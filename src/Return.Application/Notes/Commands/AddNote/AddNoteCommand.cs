// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AddNoteCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands.AddNote {
    using Common.Models;
    using MediatR;

    public sealed class AddNoteCommand : IRequest<RetrospectiveNote> {
        public string RetroId { get; }

        public int LaneId { get; }

        public AddNoteCommand(string retroId, int laneId) {
            this.RetroId = retroId;
            this.LaneId = laneId;
        }

        public override string ToString() => $"[{nameof(AddNoteCommand)}] RetroId: {this.RetroId}; LaneId: {this.LaneId}";
    }
}
