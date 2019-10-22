// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : TestCaseBuilder.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Notes.Commands.AddNote;
    using Application.Notes.Commands.UpdateNote;
    using Application.Retrospectives.Commands.CreateRetrospective;
    using Application.Retrospectives.Commands.JoinRetrospective;
    using Application.Retrospectives.Queries.GetParticipantsInfo;
    using Domain.Entities;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Return.Common;

    public sealed class TestCaseBuilder {
        private readonly IServiceScope _scope;
        private readonly string _retrospectiveId;
        private readonly string _facilitatorPassword;
        private readonly Queue<Func<Task>> _actions;
        private readonly Dictionary<string, ParticipantInfo> _participators;


        public TestCaseBuilder(IServiceScope scope, string retrospectiveId, string facilitatorPassword) {
            this._scope = scope;
            this._retrospectiveId = retrospectiveId;
            this._facilitatorPassword = facilitatorPassword;
            this._actions = new Queue<Func<Task>>();
            this._participators = new Dictionary<string, ParticipantInfo>(StringComparer.InvariantCultureIgnoreCase);
        }

        public TestCaseBuilder WithParticipator(string name, bool facilitator) {
            string RandomByte() {
                return TestContext.CurrentContext.Random.NextByte().ToString("X2", Culture.Invariant);
            }

            return this.EnqueueMediatorAction(() => new JoinRetrospectiveCommand {
                Name = name,
                Color = RandomByte() + RandomByte() + RandomByte(),
                JoiningAsFacilitator = facilitator,
                Passphrase = facilitator ? this._facilitatorPassword : null,
                RetroId = this._retrospectiveId
            },
                p => {
                    if (this._participators.ContainsKey(p.Name)) {
                        Assert.Inconclusive($"Trying to register existing participator: {p.Name}");
                    }

                    this._participators.Add(p.Name, p);
                });
        }

        public TestCaseBuilder WithNote(KnownNoteLane laneId, string participatorName, string text = null) {
            if (text == null) {
                text = TestContext.CurrentContext.Random.GetString();
            }

            RetrospectiveNote addedNote = null;
            this.EnqueueMediatorAction(participatorName, () => new AddNoteCommand(this._retrospectiveId, (int)laneId), n => addedNote = n);

            if (!String.IsNullOrEmpty(text)) {
                this.EnqueueMediatorAction(participatorName, () => new UpdateNoteCommand {
                    Id = addedNote.Id,
                    Text = text
                },
                    _ => Task.CompletedTask);
            }

            return this;
        }

        public TestCaseBuilder WithRetrospectiveStage(RetrospectiveStage stage) => this.EnqueueRetrospectiveAction(r => r.CurrentStage = stage);

        private ParticipantInfo GetParticipatorInfo(string name) {
            if (!this._participators.TryGetValue(name, out ParticipantInfo val)) {
                Assert.Inconclusive($"Test case error: participator {name} not found");
                return null;
            }

            return val;
        }

        public async Task Build() {
            while (this._actions.TryDequeue(out Func<Task> action)) {
                await action();
            }
        }

        private TestCaseBuilder EnqueueRetrospectiveAction(Action<Retrospective> action) {
            this._actions.Enqueue(() => this._scope.SetRetrospective(this._retrospectiveId, action));

            return this;
        }

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(Func<IRequest<TResponse>> requestFunc, Func<TResponse, Task> responseProcessor) => this.EnqueueMediatorAction<TResponse>(null, requestFunc, responseProcessor);

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(string participator, Func<IRequest<TResponse>> requestFunc, Func<TResponse, Task> responseProcessor) {
            this._actions.Enqueue(async () => {
                if (participator == null) {
                    this._scope.SetNoAuthenticationInfo();
                }
                else {
                    ParticipantInfo participantInfo = this.GetParticipatorInfo(participator);
                    this._scope.SetAuthenticationInfo(new CurrentParticipantModel(participantInfo.Id, participantInfo.Name, participantInfo.IsFacilitator));
                }

                IRequest<TResponse> request = requestFunc();
                TResponse response = await this._scope.Send(request, CancellationToken.None);
                await responseProcessor.Invoke(response);
            });

            return this;
        }

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(string participator, Func<IRequest<TResponse>> requestFunc, Action<TResponse> responseProcessor) =>
            this.EnqueueMediatorAction(participator, requestFunc, r => {
                responseProcessor.Invoke(r);
                return Task.CompletedTask;
            });

        private TestCaseBuilder EnqueueMediatorAction<TResponse>(Func<IRequest<TResponse>> requestFunc, Action<TResponse> responseProcessor) =>
            this.EnqueueMediatorAction(requestFunc, r => {
                responseProcessor.Invoke(r);
                return Task.CompletedTask;
            });
    }
}
