// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteLookup.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Votes.Queries {
    using System;
    using System.Collections.Generic;
    using Common.Models;

    public class VoteLookup {
        private readonly Func<VoteModel, int?> _keySelector;
        private readonly Comparison<VoteModel> _sorter;
        private readonly Dictionary<int, List<VoteModel>> _dictionary;

        public VoteLookup(Func<VoteModel, int?> keySelector, Comparison<VoteModel> sorter) {
            this._keySelector = keySelector;
            this._sorter = sorter;
            this._dictionary = new Dictionary<int, List<VoteModel>>();
        }

        internal void Initialize(List<VoteModel> source) {
            foreach (VoteModel voteModel in source) {
                int? key = this._keySelector.Invoke(voteModel);
                if (key == null) {
                    continue;
                }

                if (!this._dictionary.TryGetValue(key.Value, out List<VoteModel>? votes)) {
                    this._dictionary[key.Value] = votes = new List<VoteModel>(0);
                }
                votes.Add(voteModel);
            }

            foreach (List<VoteModel> list in this._dictionary.Values) {
                list.Sort(this._sorter);
            }
        }

        public Dictionary<int, List<VoteModel>>.KeyCollection Keys => this._dictionary.Keys;
        public Dictionary<int, List<VoteModel>>.ValueCollection Values => this._dictionary.Values;
        public bool Has(int key) => this._dictionary.ContainsKey(key);

        public List<VoteModel> Get(int key) {
            if (!this._dictionary.TryGetValue(key, out List<VoteModel>? votes)) {
                votes = new List<VoteModel>(0);
            }

            return votes;
        }

        public void Add(VoteModel notificationVote) {
            int? key = this._keySelector.Invoke(notificationVote);
            if (key == null) {
                return;
            }

            if (!this._dictionary.TryGetValue(key.Value, out List<VoteModel>? votes)) {
                this._dictionary[key.Value] = votes = new List<VoteModel>(0);
            }

            votes.Add(notificationVote);

            // Remove artificial uncast vote
            int uncastVoteIdx = votes.FindIndex(x => x.IsCast == false);
            if (uncastVoteIdx != -1) {
                votes.RemoveAt(uncastVoteIdx);
            }

            votes.Sort(this._sorter);
        }
        public void Remove(VoteModel notificationVote) {
            int? key = this._keySelector.Invoke(notificationVote);
            if (key == null) {
                return;
            }

            if (!this._dictionary.TryGetValue(key.Value, out List<VoteModel>? votes)) {
                return;
            }

            votes.RemoveAll(x => x.Id == notificationVote.Id);

            // Add artificial uncast vote
            VoteModel artificialVote = VoteModel.CreateEmptyFrom(notificationVote);
            if (this._keySelector.Invoke(artificialVote) != null) {
                votes.Add(artificialVote);
            }
        }
    }
}
