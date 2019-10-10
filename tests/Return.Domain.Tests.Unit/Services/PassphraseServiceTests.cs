// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PassphraseServiceTests.cs
//  Project         : Return.Domain.Tests.Unit
// ******************************************************************************

namespace Return.Domain.Tests.Unit.Services {
    using System;
    using Domain.Services;
    using NUnit.Framework;
    using Return.Common;

    [TestFixture]
    public sealed class PassphraseServiceTests {
        private readonly IPassphraseService _passphraseService = new PassphraseService();

        [Test]
        public void PassphraseService_NullArgument_ThrowsArgumentNullException() {
            // Given
            string passphrase = null;

            // When
            TestDelegate action = () => this._passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(action, Throws.ArgumentNullException);
        }

        [Test]
        public void PassphraseService_EmptyArgument_ThrowsArgumentException() {
            // Given
            string passphrase = String.Empty;

            // When
            TestDelegate action = () => this._passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(action, Throws.ArgumentException);
        }

        [Test]
        public void PassphraseService_Passphrase_Creates64LengthString() {
            // Given
            string passphrase = "test";

            // When
            string hashed = this._passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(hashed, Has.Length.EqualTo(64));
        }

        [Test]
        [Repeat(10)]
        public void PassphraseService_Passphrase_CreatesValidPassphrase() {
            // Given
            string passphrase = TestContext.CurrentContext.Random.NextGuid() + "_" + TestContext.CurrentContext.Random.NextGuid();

            // When
            string hashed = this._passphraseService.CreateHashedPassphrase(passphrase);

            // Then
            Assert.That(hashed, Is.Not.EqualTo(passphrase));
            Assert.That(this._passphraseService.ValidatePassphrase(passphrase, hashed), Is.True, $"Unable to validate passphrase [{passphrase}]");
        }
    }
}
