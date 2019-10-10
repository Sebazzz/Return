// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : InitiateDiscussionStageCommandHandlerTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.RetrospectiveWorkflows.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.RetrospectiveWorkflows.Commands;
    using Domain.Entities;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public sealed class InitiateDiscussionStageCommandHandlerTests : RetrospectiveWorkflowCommandTestBase {
        [Test]
        public void InitiateDiscussionStageCommandHandler_InvalidRetroId_ThrowsNotFoundException() {
            // Given
            const string retroId = "not found surely :)";
            var handler = new InitiateDiscussionStageCommandHandler(this.Context, this.RetrospectiveStatusUpdateDispatcherMock);
            var request = new InitiateDiscussionStageCommand { RetroId = retroId };

            // When
            TestDelegate action = () => handler.Handle(request, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task InitiateDiscussionStageCommandHandler_OnStatusChange_UpdatesRetroStageAndInvokesNotification() {
            // Given
            var handler = new InitiateDiscussionStageCommandHandler(this.Context, this.RetrospectiveStatusUpdateDispatcherMock);
            var request = new InitiateDiscussionStageCommand { RetroId = this.RetroId };

            this.SystemClockMock.CurrentTimeOffset.Returns(DateTimeOffset.UnixEpoch);

            // When
            await handler.Handle(request, CancellationToken.None);

            this.RefreshObject();

            // Then
            Assert.That(this.Retrospective.CurrentStage, Is.EqualTo(RetrospectiveStage.Discuss));

            await this.RetrospectiveStatusUpdateDispatcherMock.Received().DispatchUpdate(Arg.Any<Retrospective>(), CancellationToken.None);
        }
    }
}
