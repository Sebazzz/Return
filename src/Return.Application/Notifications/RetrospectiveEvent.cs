// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveEvent.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Notifications {
    public sealed class RetrospectiveEvent<T> {
        public string RetroId { get; }

        public T Argument { get; }

        public RetrospectiveEvent(string retroId, T argument) {
            this.RetroId = retroId;
            this.Argument = argument;
        }
    }
}
