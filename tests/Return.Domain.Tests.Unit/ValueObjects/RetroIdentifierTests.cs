// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetroIdentifierTests.cs
//  Project         : Return.Domain.Tests.Unit
// ******************************************************************************

namespace Return.Domain.Tests.Unit.ValueObjects {
    using System.Collections.Generic;
    using Domain.Services;
    using Domain.ValueObjects;
    using NUnit.Framework;

    [TestFixture]
    public sealed class RetroIdentifierTests {
        private readonly IRetroIdentifierService _retroIdentifierService = new RetroIdentifierService();

        [Test]
        [Retry(1)]
        public void RetroIdentifier_CreateNew_ReturnsRandomId() {
            // Given
            var generatedIds = new HashSet<string>(1000);

            // When / then
            for (int count = 1000; count > 0; count--) {
                RetroIdentifier identifier = this._retroIdentifierService.CreateNew();

                Assert.That(generatedIds.Add(identifier.StringId), Is.True, $"Non-unique identifier created: {identifier}");
            }
        }

        [Test]
        [Repeat(100)]
        public void RetroIdentifier_CreateNew_CreatesValidId() {
            // Given
            RetroIdentifier retroIdentifier = this._retroIdentifierService.CreateNew();

            // When
            bool isValid = this._retroIdentifierService.IsValid(retroIdentifier.StringId);

            // Then
            Assert.IsTrue(isValid, $"Id {retroIdentifier} is not valid");
        }


        [Test]
        [Repeat(100)]
        public void RetroIdentifier_CreateNew_CreatesIdOfLengthLessThanOrEqualTo32() {
            // Given / when
            RetroIdentifier retroIdentifier = this._retroIdentifierService.CreateNew();

            // Then
            Assert.That(retroIdentifier.StringId, Has.Length.LessThanOrEqualTo(32), $"Id {retroIdentifier} is not valid");
        }
    }
}
