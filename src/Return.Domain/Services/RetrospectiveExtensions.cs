// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveExtensions.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Services {
    using System;
    using Entities;

    public static class RetrospectiveExtensions {
        public static bool IsStarted(this Retrospective retrospective) {
            if (retrospective == null) throw new ArgumentNullException(nameof(retrospective));

            switch (retrospective.CurrentStage) {
                case RetrospectiveStage.NotStarted:
                case RetrospectiveStage.Finished:
                    return false;
                case RetrospectiveStage.Writing:
                case RetrospectiveStage.Discuss:
                case RetrospectiveStage.Grouping:
                case RetrospectiveStage.Voting:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(retrospective));
            }
        }
    }
}
