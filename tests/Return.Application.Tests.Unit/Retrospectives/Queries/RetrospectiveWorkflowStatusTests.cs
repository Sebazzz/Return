// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveWorkflowStatusTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Retrospectives.Queries {
    using System;
    using Application.Retrospectives.Queries.GetRetrospectiveStatus;
    using NUnit.Framework;

    [TestFixture]
    public static class RetrospectiveWorkflowStatusTests {
        [Test]
        public static void RetrospectiveWorkflowStatus_HasReachedTimeLimit_ReturnsTrueOnLaterDate() {
            // Given
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var workflowStatus = new RetrospectiveWorkflowStatus {
                InitiationTimestamp = new DateTimeOffset(2017, 07, 30, 13, 10, 0, TimeSpan.FromHours(2)),
                TimeLimitInMinutes = 10
            };

            // When
            bool result = workflowStatus.HasReachedTimeLimit(now);

            // Then
            Assert.That(result, Is.True);
        }

        [Test]
        public static void RetrospectiveWorkflowStatus_HasReachedTimeLimit_ReturnsFalseOnDateInBounds() {
            // Given
            var now = new DateTimeOffset(2017, 07, 30, 13, 15, 0, TimeSpan.FromHours(2));
            var workflowStatus = new RetrospectiveWorkflowStatus {
                InitiationTimestamp = new DateTimeOffset(2017, 07, 30, 13, 10, 0, TimeSpan.FromHours(2)),
                TimeLimitInMinutes = 10
            };

            // When
            bool result = workflowStatus.HasReachedTimeLimit(now);

            // Then
            Assert.That(result, Is.False);
        }

        [Test]
        public static void RetrospectiveWorkflowStatus_GetTimeLeft_ReturnsCorrectTime() {
            // Given
            var now = new DateTimeOffset(2017, 07, 30, 13, 15, 0, TimeSpan.FromHours(2));
            var workflowStatus = new RetrospectiveWorkflowStatus {
                InitiationTimestamp = new DateTimeOffset(2017, 07, 30, 13, 10, 0, TimeSpan.FromHours(2)),
                TimeLimitInMinutes = 10
            };

            // When
            TimeSpan timeLeft = workflowStatus.GetTimeLeft(now);

            // Then
            Assert.That(timeLeft, Is.EqualTo(TimeSpan.FromMinutes(5)));
        }

        [Test]
        public static void RetrospectiveWorkflowStatus_GetTimeLeft_ReturnsNoNegativeTime() {
            // Given
            DateTimeOffset now = DateTimeOffset.UtcNow;
            var workflowStatus = new RetrospectiveWorkflowStatus {
                InitiationTimestamp = new DateTimeOffset(2017, 07, 30, 13, 10, 0, TimeSpan.FromHours(2)),
                TimeLimitInMinutes = 10
            };

            // When
            TimeSpan timeLeft = workflowStatus.GetTimeLeft(now);

            // Then
            Assert.That(timeLeft, Is.EqualTo(TimeSpan.Zero));
        }
    }
}
