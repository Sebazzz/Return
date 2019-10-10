// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveLaneContent.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveLanes.Queries {
    using System.Collections.Generic;
    using Common.Models;

    public class RetrospectiveLaneContent {
        public List<RetrospectiveNote> Notes { get; } = new List<RetrospectiveNote>();

        public List<RetrospectiveNoteGroup> Groups { get; } = new List<RetrospectiveNoteGroup>();
    }
}
