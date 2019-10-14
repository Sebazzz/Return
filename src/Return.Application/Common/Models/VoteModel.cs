// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteModel.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Models {
    using System;
    using AutoMapper;
    using Domain.Entities;
    using Mapping;
    using Notifications.VoteChanged;

#nullable disable

    public sealed class VoteModel : IMapFrom<NoteVote> {
        public int Id { get; set; }

        public int ParticipantId { get; set; }

        public string ParticipantName { get; set; }

        public ColorModel ParticipantColor { get; set; }

        public bool IsCast => !(this.NoteId == null && this.NoteGroupId == null);

        public int? NoteId { get; set; }
        public int? NoteGroupId { get; set; }

        public int LaneId { get; set; }

        public void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            profile.CreateMap<NoteVote, VoteModel>()
                .ForMember(x => x.LaneId, m => m.MapFrom((nv, vm) => nv.Note?.Lane.Id ?? nv.NoteGroup?.Lane.Id))
                .ForMember(x => x.NoteGroupId, m => m.MapFrom(x => x.NoteGroup.Id))
                .ForMember(x => x.ParticipantId, m => m.MapFrom(x => x.Participant.Id))
                ;
            ;
        }
        internal static VoteModel CreateEmptyFrom(VoteModel notificationVote) =>
            new VoteModel {
                ParticipantId = notificationVote.ParticipantId,
                ParticipantColor = notificationVote.ParticipantColor,
                ParticipantName = notificationVote.ParticipantName
            };
    }
}
