// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : InitiateWritingStageCommandHandlerTests.cs
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
    public sealed class InitiateWritingStageCommandHandlerTests : RetrospectiveWorkflowCommandTestBase {
        [Test]
        public void InitiateWritingStageCommandHandler_InvalidRetroId_ThrowsNotFoundException() {
            // Given
            const string retroId = "not found surely :)";
            var handler = new InitiateWritingStageCommandHandler(this.Context, this.RetrospectiveStatusUpdateDispatcherMock, this.SystemClockMock);
            var request = new InitiateWritingStageCommand { RetroId = retroId, TimeInMinutes = 10 };

            // When
            TestDelegate action = () => handler.Handle(request, CancellationToken.None).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<NotFoundException>());
        }

        [Test]
        public async Task InitiateWritingStageCommandHandler_OnStatusChange_UpdatesRetroStageAndInvokesNotification() {
            // Given
            var handler = new InitiateWritingStageCommandHandler(this.Context, this.RetrospectiveStatusUpdateDispatcherMock, this.SystemClockMock);
            var request = new InitiateWritingStageCommand { RetroId = this.RetroId, TimeInMinutes = 10 };

            this.SystemClockMock.CurrentTimeOffset.Returns(DateTimeOffset.UnixEpoch);

            // When
            await handler.Handle(request, CancellationToken.None);

            this.RefreshObject();

            // Then
            Assert.That(this.Retrospective.CurrentStage, Is.EqualTo(RetrospectiveStage.Writing));

            Assert.That(this.Retrospective.WorkflowData.CurrentWorkflowInitiationTimestamp, Is.EqualTo(this.SystemClockMock.CurrentTimeOffset));
            Assert.That(this.Retrospective.WorkflowData.CurrentWorkflowTimeLimitInMinutes, Is.EqualTo(request.TimeInMinutes));

            await this.RetrospectiveStatusUpdateDispatcherMock.Received().DispatchUpdate(Arg.Any<Retrospective>(), CancellationToken.None);
        }
    }
}
