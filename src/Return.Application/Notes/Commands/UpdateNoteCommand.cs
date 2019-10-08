// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UpdateNoteCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notes.Commands {
    using MediatR;

#nullable disable

    public sealed class UpdateNoteCommand : IRequest {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
