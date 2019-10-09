// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveWorkflowCommandTestBase.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.RetrospectiveWorkflows.Commands {
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.RetrospectiveWorkflows.Common;
    using Domain.Entities;
    using NSubstitute;
    using NUnit.Framework;
    using Return.Common;
    using Support;

    public abstract class RetrospectiveWorkflowCommandTestBase : CommandTestBase {
#nullable disable
        protected Retrospective Retrospective { get; private set; }
        protected string RetroId { get; private set; }
        protected IRetrospectiveStatusUpdateDispatcher RetrospectiveStatusUpdateDispatcherMock { get; set; }
        protected ISystemClock SystemClockMock { get; set; }
#nullable restore

        [OneTimeSetUp]
        public async Task OneTimeSetup() {
            var retro = new Retrospective {
                Title = "Yet another test",
                Participants =
                {
                    new Participant { Name = "John", Color = Color.BlueViolet },
                    new Participant { Name = "Jane", Color = Color.Aqua },
                },
                HashedPassphrase = "abef",
                CurrentStage = RetrospectiveStage.NotStarted
            };

            this.RetroId = retro.UrlId.StringId;
            this.Retrospective = retro;
            this.ConfigureRetrospective(retro);

            this.Context.Retrospectives.Add(retro);
            await this.Context.SaveChangesAsync(CancellationToken.None);
        }

        [SetUp]
        public void SetUp() {
            this.RetrospectiveStatusUpdateDispatcherMock = Substitute.For<IRetrospectiveStatusUpdateDispatcher>();
            this.SystemClockMock = Substitute.For<ISystemClock>();
        }

        protected void RefreshObject() {
            using IReturnDbContext newEditContext = this.Context.CreateForEditContext();
            this.Retrospective = newEditContext.Retrospectives.Find(this.Retrospective.Id);
        }

        protected virtual void ConfigureRetrospective(Retrospective retrospective) { }
    }
}
