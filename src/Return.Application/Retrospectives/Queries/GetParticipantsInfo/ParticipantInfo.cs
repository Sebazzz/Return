// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantInfo.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetParticipantsInfo {
    using System;
    using AutoMapper;
    using Common.Mapping;
    using Common.Models;
    using Domain.Entities;

#nullable disable
    public class ParticipantInfo : IMapFrom<Participant> {
        public string Name { get; set; }

        public ColorModel Color { get; set; }

        public void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            profile.CreateMap<Participant, ParticipantInfo>();
        }
    }
}
