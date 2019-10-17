// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : GetShowcaseQuery.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Showcase.Queries {
    using System;
    using System.Collections.Generic;
    using Common.Models;
    using Retrospectives.Queries.GetRetrospectiveStatus;
    using Votes.Queries;

    public sealed class GetShowcaseQueryResult {
        public ShowcaseData Showcase { get; }

        public GetShowcaseQueryResult(ShowcaseData showcase) {
            this.Showcase = showcase;
        }
    }

    public sealed class ShowcaseData {
        public List<ShowcaseItem> Items { get; } = new List<ShowcaseItem>();

        internal void Sort() => this.Items.Sort(ShowcaseItem.ShowcaseItemComparer);
    }

    public sealed class ShowcaseItem {
        public RetrospectiveNote? Note { get; }

        public RetrospectiveNoteGroup? NoteGroup { get; }

        public RetrospectiveLane Lane { get; }

        public List<VoteModel> Votes { get; }

        public ShowcaseItem(RetrospectiveNote note, RetrospectiveLane lane, RetrospectiveVoteStatus voteStatus) {
            if (voteStatus == null) throw new ArgumentNullException(nameof(voteStatus));
            this.Note = note ?? throw new ArgumentNullException(nameof(note));
            this.Lane = lane ?? throw new ArgumentNullException(nameof(lane));
            this.Votes = voteStatus.VotesByNote.Get(note.Id);
        }

        public ShowcaseItem(RetrospectiveNoteGroup noteGroup, RetrospectiveLane lane, RetrospectiveVoteStatus voteStatus) {
            if (voteStatus == null) throw new ArgumentNullException(nameof(voteStatus));
            this.NoteGroup = noteGroup ?? throw new ArgumentNullException(nameof(noteGroup));
            this.Lane = lane ?? throw new ArgumentNullException(nameof(lane));
            this.Votes = voteStatus.VotesByNoteGroup.Get(noteGroup.Id);
        }

        private sealed class ShowcaseItemRelationalComparer : IComparer<ShowcaseItem> {
            public int Compare(ShowcaseItem x, ShowcaseItem y) {
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (y == null) throw new ArgumentNullException(nameof(y));

                int comparison = y.Votes.Count.CompareTo(x.Votes.Count);
                if (comparison != 0) {
                    return comparison;
                }

                comparison = x.Lane.Id.CompareTo(y.Lane.Id);
                if (comparison != 0) {
                    return comparison;
                }

                comparison = (x.NoteGroup != null).CompareTo(y.NoteGroup != null);
                if (comparison != 0) {
                    return comparison;
                }

                static string GetName(ShowcaseItem item) {
                    return item.Note?.Text ?? item.NoteGroup?.Title ?? String.Empty;
                }

                return String.CompareOrdinal(GetName(x), GetName(y));
            }
        }

        internal static IComparer<ShowcaseItem> ShowcaseItemComparer { get; } = new ShowcaseItemRelationalComparer();
    }
}
