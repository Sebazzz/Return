// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantInfo.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Retrospectives.Queries.GetParticipantsInfo {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoMapper;
    using Common.Mapping;
    using Common.Models;
    using Domain.Entities;

#nullable disable
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class ParticipantInfo : IMapFrom<Participant>, IEquatable<ParticipantInfo> {
        public int Id { get; set; }
        public string Name { get; set; }
        public ColorModel Color { get; set; }
        public bool IsFacilitator { get; set; }

        public void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            profile.CreateMap<Participant, ParticipantInfo>();
        }

        public bool Equals(ParticipantInfo other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ParticipantInfo)obj);
        }

        public override int GetHashCode() => this.Id.GetHashCode();
    }
}
