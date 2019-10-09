// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IOwnedByParticipant.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Abstractions {
    public interface IOwnedByParticipant {
        int ParticipantId { get; }
    }
}
