// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveNoteGroups.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Models {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Domain.Entities;
    using Mapping;

    public sealed class RetrospectiveNoteGroup : IMapFrom<NoteGroup> {
        public int Id { get; set; }

        public string Title { get; set; } = String.Empty;

        public IList<RetrospectiveNote> Notes { get; } = new List<RetrospectiveNote>();

        public void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            profile.CreateMap<NoteGroup, RetrospectiveNoteGroup>();

            ICollection<RetrospectiveNoteGroup> GroupNotes(ICollection<RetrospectiveNote> notes, ICollection<RetrospectiveNoteGroup> groups) {
                var grouped = notes.GroupBy(x => x.GroupId.GetValueOrDefault()).Join(groups, g => g.Key, g => g.Id, (rg, g) => new { NoteGroup = g, Note = rg });

                foreach (var grouping in grouped) {
                    foreach (RetrospectiveNote note in grouping.Note) {
                        grouping.NoteGroup.Notes.Add(note);
                        notes.Remove(note);
                    }
                }

                return groups;
            }

            profile.CreateMap<ICollection<RetrospectiveNote>, ICollection<RetrospectiveNoteGroup>>()
                .ConvertUsing(GroupNotes);
        }
    }
}
