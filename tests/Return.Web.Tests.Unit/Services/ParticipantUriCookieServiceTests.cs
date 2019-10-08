// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ParticipantUriCookieServiceTests.cs
//  Project         : Return.Web.Tests.Unit
// ******************************************************************************

namespace Return.Web.Tests.Unit.Services {
    using System;
    using Application.Common.Models;
    using Infrastructure;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Logging.Abstractions;
    using NUnit.Framework;
    using Web.Services;

    [TestFixture]
    public sealed class ParticipantUriCookieServiceTests {
        private readonly IParticipantUriCookieService _participantUriCookieService;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "NullLoggerFactory does nothing")]
        public ParticipantUriCookieServiceTests() {
            this._participantUriCookieService = new ParticipantUriCookieService(
                new MachineSystemClock(),
                new NullLogger<ParticipantUriCookieService>());
        }

        [Test]
        public void ParticipantUriCookieService_DecryptNull_ThrowsArgumentNullException() {
            // Given
            const string value = null;

            // When
            TestDelegate action = () => this._participantUriCookieService.Decrypt(value);

            // Then
            Assert.That(action, Throws.ArgumentNullException);
        }

        [Test]
        public void ParticipantUriCookieService_DecryptEmpty_ThrowsArgumentException() {
            // Given
            string value = String.Empty;

            // When
            TestDelegate action = () => this._participantUriCookieService.Decrypt(value);

            // Then
            Assert.That(action, Throws.ArgumentException);
        }

        [Test]
        public void ParticipantUriCookieService_DecryptInvalidLength_ReturnsEmpty() {
            // Given
            string value = "123";

            // When
            CurrentParticipantModel result = this._participantUriCookieService.Decrypt(value);

            // Then
            Assert.That(result, Is.EqualTo(default(CurrentParticipantModel)));
        }

        [Test]
        public void ParticipantUriCookieService_DecryptInvalid_ReturnsEmpty() {
            // Given
            string value = "XXYYABCDEF";

            // When
            CurrentParticipantModel result = this._participantUriCookieService.Decrypt(value);

            // Then
            Assert.That(result, Is.EqualTo(default(CurrentParticipantModel)));
        }

        [Test]
        public void ParticipantUriCookieService_Encrypt_ThrowsExceptionOnLongName() {
            // Given
            var input = new CurrentParticipantModel(1, new string('X', 257), false);

            // When
            TestDelegate action = () => this._participantUriCookieService.Encrypt(input);

            // Then
            Assert.That(action, Throws.ArgumentException);
        }

        [Test]
        public void ParticipantUriCookieService_RoundtripEmpty() {
            // Given
            CurrentParticipantModel input = default;

            // When
            string cookie = this._participantUriCookieService.Encrypt(input);
            CurrentParticipantModel output = this._participantUriCookieService.Decrypt(cookie);

            // Then
            AssertRoundtripResult(ref output, ref input, cookie);
        }

        [Test]
        [TestCaseSource(nameof(RoundtripTestCases))]
        public void ParticipantUriCookieService_RoundtripTests(CurrentParticipantModel testCase) {
            // Given
            CurrentParticipantModel input = testCase;

            // When
            string cookie = this._participantUriCookieService.Encrypt(input);
            CurrentParticipantModel output = this._participantUriCookieService.Decrypt(cookie);

            // Then
            AssertRoundtripResult(ref output, ref input, cookie);
        }

        [Test]
        [TestCaseSource(nameof(RoundtripTestCases))]
        public void ParticipantUriCookieService_RoundtripTests_Tamper(CurrentParticipantModel testCase) {
            // Given
            CurrentParticipantModel input = testCase;

            // When
            string cookie = this._participantUriCookieService.Encrypt(input);
            cookie = cookie.Substring(0, 4) + "AA" + cookie.Substring(6);
            CurrentParticipantModel output = this._participantUriCookieService.Decrypt(cookie);

            // Then
            Assert.That(output, Is.EqualTo(default(CurrentParticipantModel)));
        }

        private static readonly CurrentParticipantModel[] RoundtripTestCases =
        {
            new CurrentParticipantModel(-11, "John", false),
            new CurrentParticipantModel(588353, "LOOOONG JOHN THE BOSS MANAGER", true),
            new CurrentParticipantModel(1, "J", false),
            new CurrentParticipantModel(1, "Got Emo💕ji?", false),
            new CurrentParticipantModel(1, new string('!', 256), false),
        };

        private static void AssertRoundtripResult(ref CurrentParticipantModel output, ref CurrentParticipantModel input, string cookie) {
            TestContext.Out.WriteLine($"[{input.Id},{input.IsManager},{input.Name}] resulted in cookie of length {cookie.Length}: {cookie}");

            Assert.That(cookie, Is.EqualTo(Uri.EscapeDataString(cookie)), "Cookie is not URI safe by default");

            Assert.That(output.Id, Is.EqualTo(input.Id), $"Difference in input/output: {nameof(input.Id)}");

            Assert.That(output.IsManager, Is.EqualTo(input.IsManager), $"Difference in input/output: {nameof(input.IsManager)}");

            Assert.That(output.Name, Is.EqualTo(input.Name), $"Difference in input/output: {nameof(input.Name)}");

            Assert.That(cookie, Has.Length.LessThan(256 * 2));
        }
    }
}
