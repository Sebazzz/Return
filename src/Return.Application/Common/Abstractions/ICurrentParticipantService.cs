// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ICurrentParticipantService.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Abstractions
{
    public interface ICurrentParticipantService
    {
        int GetParticipantId();

       bool IsManager();
    }
}
