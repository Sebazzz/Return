// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantsInfoList.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospective.Queries.GetParticipantsInfo
{
    using System.Collections.Generic;

    public class ParticipantsInfoList
    {
        public List<ParticipantInfo> Participants { get; } = new List<ParticipantInfo>();
    }
}
