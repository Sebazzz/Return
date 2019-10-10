// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SecurityValidatorTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Common.Security {
    using System;
    using System.Threading.Tasks;
    using Application.Common.Abstractions;
    using Application.Common.Models;
    using Application.Common.Security;
    using Domain.Entities;
    using NUnit.Framework;
    using NUnit.Framework.Internal;
    using Support;

    [TestFixture]
    public sealed class SecurityValidatorTests {
        private readonly ISecurityValidator _securityValidator;
        private readonly MockCurrentParticipantService _currentParticipantService;

        public SecurityValidatorTests() {
            this._currentParticipantService = new MockCurrentParticipantService();
            this._securityValidator = new SecurityValidator(this._currentParticipantService, TestLogger.For<SecurityValidator>());
        }

        [SetUp]
        public void SetUp() => this._currentParticipantService.Reset();

        [Test]
        public void SecurityValidator_DisallowsOperationOnNote_WrongParticipant() {
            // Given
            Retrospective retro = GetRetrospectiveInStage(RetrospectiveStage.Writing);
            var note = new Note { ParticipantId = 1 };
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(2, String.Empty, false));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, note).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<OperationSecurityException>());
        }

        [Test]
        public void SecurityValidator_DisallowsOperationOnNote_UnauthenticatedParticipant() {
            // Given
            Retrospective retro = GetRetrospectiveInStage(RetrospectiveStage.Writing);
            var note = new Note { ParticipantId = 1 };
            this._currentParticipantService.Reset();
            Assume.That(this._currentParticipantService.GetParticipant().Result, Is.EqualTo(default(CurrentParticipantModel)));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, note).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<OperationSecurityException>());
        }

        [Test]
        public void SecurityValidator_AllowsOperationOnNote_CorrectParticipant() {
            // Given
            Retrospective retro = GetRetrospectiveInStage(RetrospectiveStage.Writing);
            var note = new Note { ParticipantId = 212 };
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(212, String.Empty, false));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, note).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.Nothing);
        }

        [Test]
        [TestCase(RetrospectiveStage.NotStarted)]
        [TestCase(RetrospectiveStage.Discuss)]
        [TestCase(RetrospectiveStage.Grouping)]
        [TestCase(RetrospectiveStage.Voting)]
        [TestCase(RetrospectiveStage.Finished)]
        public void SecurityValidator_DisallowsOperationsOnNote_InStages(RetrospectiveStage stage) {
            // Given
            Retrospective retro = GetRetrospectiveInStage(stage);
            var note = new Note { ParticipantId = 252 };
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(252, String.Empty, false));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, note).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<OperationSecurityException>());
        }

        [Test]
        public void SecurityValidator_DisallowsOperationOnNoteGroup_NotManager() {
            // Given
            Retrospective retro = GetRetrospectiveInStage(RetrospectiveStage.Grouping);
            var noteGroup = new NoteGroup { Title = "G" };
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(2, String.Empty, false));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, noteGroup).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<OperationSecurityException>());
        }

        [Test]
        public void SecurityValidator_DisallowsOperationOnNoteGroup_UnauthenticatedParticipant() {
            // Given
            Retrospective retro = GetRetrospectiveInStage(RetrospectiveStage.Grouping);
            var noteGroup = new NoteGroup { Title = "G" };
            this._currentParticipantService.Reset();
            Assume.That(this._currentParticipantService.GetParticipant().Result, Is.EqualTo(default(CurrentParticipantModel)));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, noteGroup).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<OperationSecurityException>());
        }

        [Test]
        public void SecurityValidator_AllowsOperationOnNote_IsManager() {
            // Given
            Retrospective retro = GetRetrospectiveInStage(RetrospectiveStage.Grouping);
            var noteGroup = new NoteGroup { Title = "G" };
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(212, String.Empty, true));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, noteGroup).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.Nothing);
        }

        [Test]
        [TestCase(RetrospectiveStage.NotStarted)]
        [TestCase(RetrospectiveStage.Discuss)]
        [TestCase(RetrospectiveStage.Writing)]
        [TestCase(RetrospectiveStage.Voting)]
        [TestCase(RetrospectiveStage.Finished)]
        public void SecurityValidator_DisallowsOperationsOnNoteGroup_InStages(RetrospectiveStage stage) {
            // Given
            Retrospective retro = GetRetrospectiveInStage(stage);
            var noteGroup = new NoteGroup { Title = "G" };
            this._currentParticipantService.SetParticipant(new CurrentParticipantModel(252, String.Empty, true));

            // When
            TestDelegate action = () => this._securityValidator.EnsureOperation(retro, SecurityOperation.AddOrUpdate, noteGroup).GetAwaiter().GetResult();

            // Then
            Assert.That(action, Throws.InstanceOf<OperationSecurityException>());
        }

        private static Retrospective GetRetrospectiveInStage(RetrospectiveStage retrospectiveStage) {
            return new Retrospective {
                CurrentStage = retrospectiveStage
            };
        }

        private sealed class MockCurrentParticipantService : ICurrentParticipantService {
            private CurrentParticipantModel _currentParticipant;

            public ValueTask<CurrentParticipantModel> GetParticipant() => new ValueTask<CurrentParticipantModel>(this._currentParticipant);

            public void SetParticipant(CurrentParticipantModel participant) => this._currentParticipant = participant;

            public void Reset() => this._currentParticipant = default;
        }
    }
}
