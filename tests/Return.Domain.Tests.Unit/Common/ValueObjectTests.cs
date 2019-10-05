// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ValueObjectTests.cs
//  Project         : Return.Domain.Tests.Unit
// ******************************************************************************

namespace Return.Domain.Tests.Unit.Common
{
    using System.Collections.Generic;
    using Domain.Common;
    using NUnit.Framework;

    [TestFixture]
    public class ValueObjectTests
    {
        private class Point : ValueObject
        {
            private Point()
            {
            }

            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public int X { get; }
            public int Y { get; }

            protected override IEnumerable<object> GetAtomicValues()
            {
                yield return this.X;
                yield return this.Y;
            }
        }

        [Test]
        public void Equals_GivenDifferentValues_ShouldReturnFalse()
        {
            var point1 = new Point(x: 1, y: 2);
            var point2 = new Point(x: 2, y: 1);

            Assert.False(point1.Equals(obj: point2));
        }

        [Test]
        public void Equals_GivenMatchingValues_ShouldReturnTrue()
        {
            var point1 = new Point(x: 1, y: 2);
            var point2 = new Point(x: 1, y: 2);

            Assert.True(point1.Equals(obj: point2));
        }
    }
}
