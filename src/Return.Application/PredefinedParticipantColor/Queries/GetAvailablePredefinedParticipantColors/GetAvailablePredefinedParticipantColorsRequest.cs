// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantColor.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.PredefinedParticipantColor.Queries.GetAvailablePredefinedParticipantColors
{
    using System.Collections.Generic;
    using MediatR;

    public class GetAvailablePredefinedParticipantColorsRequest : IRequest<IList<AvailableParticipantColorModel>>
    {
        public string RetrospectiveId { get; }

        public GetAvailablePredefinedParticipantColorsRequest(string retrospectiveId)
        {
            this.RetrospectiveId = retrospectiveId;
        }
    }

}
