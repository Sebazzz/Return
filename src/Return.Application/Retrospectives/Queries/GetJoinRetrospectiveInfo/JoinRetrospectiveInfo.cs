// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : JoinRetrospectiveInfo.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetJoinRetrospectiveInfo {

    public sealed class JoinRetrospectiveInfo {
        public string Title { get; }

        public bool NeedsParticipantPassphrase { get; }
        public bool IsStarted { get; }
        public bool IsFinished { get; }


        public JoinRetrospectiveInfo(string title, bool needsParticipantPassphrase, bool isStarted, bool isFinished) {
            this.NeedsParticipantPassphrase = needsParticipantPassphrase;
            this.IsStarted = isStarted;
            this.IsFinished = isFinished;
            this.Title = title;
        }
    }
}
