// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AutoResettingBooleanTests.cs
//  Project         : Return.Web.Tests.Unit
// ******************************************************************************

namespace Return.Web.Tests.Unit.Services {
    using NUnit.Framework;
    using Web.Services;

    [TestFixture]
    public sealed class AutoResettingBooleanTests {
        [Test]
        public void AutoResettingBoolean_NoIntervention_GetsInitialValue() {
            // Given
            const bool initialValue = true;
            var resettingBoolean = new AutoResettingBoolean(initialValue);

            // When / then
            for (int i = 0; i < 10; i++) {
                Assert.That(resettingBoolean.GetValue, Is.EqualTo(initialValue));
            }
        }

        [Test]
        public void AutoResettingBoolean_WhenSet_ResetsToInitialValue() {
            // Given
            const bool initialValue = true;
            var resettingBoolean = new AutoResettingBoolean(initialValue);

            // When
            resettingBoolean.Set();
            bool first = resettingBoolean.GetValue();
            bool second = resettingBoolean.GetValue();

            // Then
            Assert.That(first, Is.False);
            Assert.That(second, Is.True);
        }
    }
}
