// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteGroupCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.NoteGroups.Commands {
    using Domain.Entities;
    using MediatR;

    public sealed class UpdateNoteGroupCommand : IRequest {
        public string RetroId { get; }

        public int Id { get; }

        public string Name { get; }

        public UpdateNoteGroupCommand(string retroId, int id, string name) {
            this.RetroId = retroId;
            this.Id = id;
            this.Name = name;
        }
        public override string ToString() => $"[{nameof(AddNoteGroupCommand)}] {this.RetroId} on lane #{(KnownNoteLane)this.Id} - content: {this.Name}";
    }
}
