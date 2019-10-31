// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : TestCaseBuilder.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.NoteGroups.Commands;
    using Application.Notes.Commands.AddNote;
    using Application.Notes.Commands.MoveNote;
    using Application.Notes.Commands.UpdateNote;
    using Application.Notifications.VoteChanged;
    using Application.PredefinedParticipantColors.Queries.GetAvailablePredefinedParticipantColors;
    using Application.Retrospectives.Commands.JoinRetrospective;
    using Application.Retrospectives.Queries.GetParticipantsInfo;
    using Application.Votes.Commands;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Return.Common;

    public sealed class TestCaseBuilder {
        private readonly IServiceScope _scope;
        private readonly string _retrospectiveId;
        private readonly Queue<Func<Task>> _actions;
        private readonly Dictionary<string, ParticipantInfo> _participators;
        private readonly FriendlyIdEntityDictionary _entityIds;

        private (Type Type, int Id) _lastAddedItem;

        public TestCaseBuilder(IServiceScope scope, string retrospectiveId) {
            this._scope = scope;
            this._retrospectiveId = retrospectiveId;
            this._actions = new Queue<Func<Task>>();
            this._participators = new Dictionary<string, ParticipantInfo>(StringComparer.InvariantCultureIgnoreCase);
            this._entityIds = new FriendlyIdEntityDictionary();
        }

        public TestCaseBuilder HasExistingParticipant(string participantName) {
            this._actions.Enqueue(async () => {
                TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] attempting to record presence of existing participant [{participantName}]");

                var dbContext = this._scope.ServiceProvider.GetRequiredService<IReturnDbContext>();
                Participant participant = await dbContext.Participants.FirstAsync(x => x.Name == participantName && x.Retrospective.UrlId.StringId == this._retrospectiveId);

                this._participators.Add(participant.Name, new ParticipantInfo {
                    Id = participant.Id,
                    Name = participant.Name,
                    Color = new ColorModel(), // Doesn't matter
                    IsFacilitator = participant.IsFacilitator
                });

                TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] recorded presence of existing participant [{participantName}] with ID #{participant.Id}");
            });
            return this;
        }

        public TestCaseBuilder WithParticipant(string name, bool isFacilitator, string passphrase = null) {
            string RandomByte() {
                return TestContext.CurrentContext.Random.NextByte().ToString("X2", Culture.Invariant);
            }

            AvailableParticipantColorModel availableParticipantColor = null;
            this._actions.Enqueue(async () => {
                this._scope.SetNoAuthenticationInfo();
                var yellowColor = new ColorModel {
                    R = Color.Yellow.R,
                    G = Color.Yellow.G,
                    B = Color.Yellow.B,
                };

                IList<AvailableParticipantColorModel> response = await this._scope.Send(new GetAvailablePredefinedParticipantColorsQuery(this._retrospectiveId));
                availableParticipantColor = response.FirstOrDefault(x => !x.HasSameColors(yellowColor)); // Yellow is a bad color for testing
            });

            return this.EnqueueMediatorAction(() => new JoinRetrospectiveCommand {
                Name = name,
                Color = availableParticipantColor?.HexString ?? (RandomByte() + RandomByte() + RandomByte()),
                JoiningAsFacilitator = isFacilitator,
                Passphrase = passphrase,
                RetroId = this._retrospectiveId
            }, p => {
                if (this._participators.ContainsKey(p.Name)) {
                    Assert.Inconclusive($"Trying to register existing participant: {p.Name}");
                }

                this.RecordAddedId<ParticipantInfo>(p.Id);
                this._participators.Add(p.Name, p);
            });
        }

        public TestCaseBuilder WithNote(KnownNoteLane laneId, string participantName, string text = null) {
            if (text == null) {
                text = TestContext.CurrentContext.Random.GetString();
            }

            RetrospectiveNote addedNote = null;
            this.EnqueueMediatorAction(participantName, () => new AddNoteCommand(this._retrospectiveId, (int)laneId),
                n => {
                    this.RecordAddedId<RetrospectiveNote>(n.Id);
                    addedNote = n;
                });

            if (!String.IsNullOrEmpty(text)) {
                this.EnqueueMediatorAction(participantName, () => new UpdateNoteCommand {
                    Id = addedNote.Id,
                    Text = text
                }, _ => Task.CompletedTask);
            }

            return this;
        }

        public TestCaseBuilder WithVoteOnNoteGroup(string participantName, int noteGroupId) =>
            this.EnqueueMediatorAction(participantName, () => CastVoteCommand.ForNoteGroup(noteGroupId, VoteMutationType.Added),
                _ => { });

        public TestCaseBuilder WithVoteOnNote(string participantName, int noteId) =>
            this.EnqueueMediatorAction(participantName, () => CastVoteCommand.ForNote(noteId, VoteMutationType.Added),
                _ => { });

        public TestCaseBuilder OutputId(Action<int> callback) {
            this._actions.Enqueue(() => {
                if (this._lastAddedItem == default) {
                    throw new InvalidOperationException("A call to OutputId should follow a call to an entity creating action");
                }

                TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Outputting last added item {this._lastAddedItem.Type} with ID #{this._lastAddedItem.Id} to callback ({callback.GetMethodInfo().Name})");
                callback.Invoke(this._lastAddedItem.Id);

                return Task.CompletedTask;
            });

            return this;
        }

        public TestCaseBuilder Callback(Action<TestCaseBuilder> callback) {
            this._actions.Enqueue(() => {
                callback(this);
                return Task.CompletedTask;
            });

            return this;
        }

        /// <summary>
        /// Call after entity creation actions
        /// </summary>
        /// <param name="friendlyId"></param>
        /// <returns></returns>
        public TestCaseBuilder WithId(string friendlyId) {
            this._actions.Enqueue(() => {
                if (this._lastAddedItem == default) {
                    throw new InvalidOperationException("A call to WithId should follow a call to an entity creating action");
                }

                this._entityIds.Set(friendlyId, this._lastAddedItem.Type, this._lastAddedItem.Id);

                return Task.CompletedTask;
            });

            return this;
        }

        public TestCaseBuilder WithNoteGroup(string participantName, KnownNoteLane laneId, string text = null) {
            if (text == null) {
                text = TestContext.CurrentContext.Random.GetString();
            }

            RetrospectiveNoteGroup addedNoteGroup = null;
            this.EnqueueMediatorAction(participantName, () => new AddNoteGroupCommand(this._retrospectiveId, (int)laneId),
                ng => {
                    this.RecordAddedId<RetrospectiveNoteGroup>(ng.Id);
                    addedNoteGroup = ng;
                });

            if (!String.IsNullOrEmpty(text)) {
                this.EnqueueMediatorAction(participantName,
                    () => new UpdateNoteGroupCommand(this._retrospectiveId, addedNoteGroup.Id, text)
                    , _ => Task.CompletedTask);
            }

            return this;
        }

        public TestCaseBuilder AddNoteToNoteGroup(string participantName, string noteId, string noteGroupId) =>
            this.EnqueueMediatorAction(participantName, () => {
                int dbNoteId = this._entityIds.Get(noteId, typeof(RetrospectiveNote));
                int? dbNoteGroupId = noteGroupId != null
                    ? this._entityIds.Get(noteGroupId, typeof(RetrospectiveNoteGroup))
                    : (int?)null;
                return new MoveNoteCommand(dbNoteId, dbNoteGroupId);
            },
                _ => { });

        public TestCaseBuilder AddNoteToNoteGroup(string participantName, int dbNoteId, int? dbNoteGroupId) =>
            this.EnqueueMediatorAction(participantName, () => new MoveNoteCommand(dbNoteId, dbNoteGroupId),
                _ => { });

        public TestCaseBuilder WithRetrospectiveStage(RetrospectiveStage stage) => this.EnqueueRetrospectiveAction(r => r.CurrentStage = stage);

        private ParticipantInfo GetParticipatorInfo(string name) {
            if (!this._participators.TryGetValue(name, out ParticipantInfo val)) {
                Assert.Inconclusive($"Test case error: participantName {name} not found");
                return null;
            }

            return val;
        }

        public async Task Build() {
            int actionNumber = 1;
            while (this._actions.TryDequeue(out Func<Task> action)) {
                try {
                    await action();
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"Error execution action #{actionNumber}: {action}", ex);
                }

                actionNumber++;
            }
        }

        private void RecordAddedId<T>(int id) {
            TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Recording last added item: [{typeof(T)}] with ID #{id}");
            this._lastAddedItem = (typeof(T), id);
        }

        private TestCaseBuilder EnqueueRetrospectiveAction(Action<Retrospective> action) {
            this._actions.Enqueue(() => this._scope.SetRetrospective(this._retrospectiveId, action));

            return this;
        }

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(Func<IRequest<TResponse>> requestFunc, Func<TResponse, Task> responseProcessor) => this.EnqueueMediatorAction<TResponse>(null, requestFunc, responseProcessor);

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(string participantName, Func<IRequest<TResponse>> requestFunc, Func<TResponse, Task> responseProcessor) {
            this._actions.Enqueue(async () => {
                IRequest<TResponse> request = requestFunc();

                if (participantName == null) {
                    TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Executing request [{request}] with no participant");

                    this._scope.SetNoAuthenticationInfo();
                }
                else {
                    TestContext.WriteLine($"[{nameof(TestCaseBuilder)}] Executing request [{request}] with participant {participantName}");

                    ParticipantInfo participantInfo = this.GetParticipatorInfo(participantName);
                    this._scope.SetAuthenticationInfo(new CurrentParticipantModel(participantInfo.Id, participantInfo.Name, participantInfo.IsFacilitator));
                }

                try {
                    TResponse response = await this._scope.Send(request, CancellationToken.None);
                    await responseProcessor.Invoke(response);
                }
                catch (Exception ex) {
                    throw new InvalidOperationException($"Action failed [{request}] with participant {participantName}: {ex.Message}", ex);
                }
            });

            return this;
        }

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(string participantName, Func<IRequest<TResponse>> requestFunc, Action<TResponse> responseProcessor) =>
            this.EnqueueMediatorAction(participantName, requestFunc, r => {
                responseProcessor.Invoke(r);
                return Task.CompletedTask;
            });

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(Func<IRequest<TResponse>> requestFunc, Action<TResponse> responseProcessor) =>
            this.EnqueueMediatorAction(requestFunc, r => {
                responseProcessor.Invoke(r);
                return Task.CompletedTask;
            });

        private sealed class FriendlyIdEntityDictionary {
            private readonly Dictionary<string, (Type Type, int Id)> _dataStore;

            public FriendlyIdEntityDictionary() {
                this._dataStore = new Dictionary<string, (Type, int)>(StringComparer.Ordinal);
            }

            public int Get(string friendlyId, Type type) {
                if (!this._dataStore.TryGetValue(friendlyId, out (Type Type, int Id) item)) {
                    throw new ArgumentException($"Entity {type} with id '{friendlyId}' is not found");
                }

                return item.Id;
            }

            public void Set(string friendlyId, Type type, int itemId) {
                try {
                    this._dataStore[friendlyId] = (type, itemId);
                }
                catch (ArgumentException) {
                    throw new ArgumentException($"Entity {type} with id '{friendlyId}' is already exists");
                }
            }
        }
    }
}
