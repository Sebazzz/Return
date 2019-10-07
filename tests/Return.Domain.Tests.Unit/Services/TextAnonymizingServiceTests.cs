// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : TextAnonymizingServiceTests.cs
//  Project         : Return.Domain.Tests.Unit
// ******************************************************************************

namespace Return.Domain.Tests.Unit.Services {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Domain.Services;
    using NUnit.Framework;

    [TestFixture]
    public sealed class TextAnonymizingServiceTests {
        [Test]
        public void TextAnonymizingService_ThrowsArgumentNullException_OnNullArgument() {
            // Given
            ITextAnonymizingService service = new TextAnonymizingService();

            // When
            TestDelegate action = () => service.AnonymizeText(null);

            // Then
            Assert.That(action, Throws.ArgumentNullException);
        }

        [Test]
        public void TextAnonymizingService_HandlesEmptyString() {
            // Given
            ITextAnonymizingService service = new TextAnonymizingService();
            string input = String.Empty;

            // When
            string result = service.AnonymizeText(input);

            // Then
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void TextAnonymizingService_HandlesSingleWord() {
            // Given
            ITextAnonymizingService service = new TextAnonymizingService();
            string input = "traffic";

            // When
            string result = service.AnonymizeText(input);

            // Then
            AssertInputResult(input, result);
        }


        [Test]
        public void TextAnonymizingService_HandlesTwoWords() {
            // Given
            ITextAnonymizingService service = new TextAnonymizingService();
            string input = "Some waiting";

            // When
            string result = service.AnonymizeText(input);

            // Then
            AssertInputResult(input, result);
        }

        [Test]
        public void TextAnonymizingService_HandlesMultipleWords() {
            // Given
            ITextAnonymizingService service = new TextAnonymizingService();
            string input = "I was waiting in traffic";

            // When
            string result = service.AnonymizeText(input);

            // Then
            AssertInputResult(input, result);
        }

        private static void AssertInputResult(string input, string result) {
            Trace.Write($"Input: {input}");
            Trace.Write($"Result: {input}");

            Assert.That(result, Has.Length.EqualTo(input.Length));
            Assert.That(result, Is.Not.EqualTo(input));

            static IEnumerable<int> SpaceIndices(string str) {
                return str.Select((ch, idx) => ch == ' ' ? idx : -1).Where(x => x >= 0);
            }

            Assert.That(result, Is.Not.EqualTo(input));
            Assert.That(SpaceIndices(result),
                Is.EquivalentTo(SpaceIndices(input)),
                $"Indices of spaces in result '{result}' is not equal to input '{input}'");
        }
    }
}
