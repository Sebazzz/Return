// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveStage.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Entities {
    public enum RetrospectiveStage {
        /// <summary>
        /// The retrospective is not started yet and waiting for a facilitator to appear
        /// </summary>
        NotStarted,

        /// <summary>
        /// The retrospective is underway: participants are currently writing down their findings. Notes are private to the creator.
        /// </summary>
        Writing,

        /// <summary>
        /// The retrospective is underway: written down notes are currently being discussed. Notes cannot be modified anymore. Notes are public.
        /// </summary>
        Discuss,

        /// <summary>
        /// The retrospective is underway: notes are currently being grouped by the facilitator.
        /// </summary>
        Grouping,

        /// <summary>
        /// The retrospective is underway: voting on the notes takes place.
        /// </summary>
        Voting,

        /// <summary>
        /// The retrospective has been finished
        /// </summary>
        Finished
    }
}
