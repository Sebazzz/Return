// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ColorModelTests.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Common.Models {
    using System.Drawing;
    using Application.Common.Models;
    using NUnit.Framework;

    [TestFixture]
    public static class ColorModelTests {
        [Test]
        public static void ColorModel_HasSameColors_ReturnsTrue() {
            // Given
            var derivedColor = new DerivedColorModel(Color.BlueViolet);
            var color = new ColorModel { R = derivedColor.R, B = derivedColor.B, G = derivedColor.G };

            // When
            bool result = color.HasSameColors(derivedColor);

            // Then
            Assert.That(result, Is.True);
        }

        [Test]
        public static void ColorModel_HasSameColors_ReturnsFalse() {
            // Given
            var derivedColor = new DerivedColorModel(Color.BlueViolet);
            var color = new ColorModel { R = derivedColor.R, B = derivedColor.B, G = derivedColor.G };

            derivedColor.G = 0;

            // When
            bool result = color.HasSameColors(derivedColor);

            // Then
            Assert.That(result, Is.False);
        }

        private sealed class DerivedColorModel : ColorModel {
            public string KnownName { get; }

            public DerivedColorModel(Color color) {
                this.KnownName = color.Name;
                this.R = color.R;
                this.G = color.G;
                this.B = color.B;
            }
        }
    }
}
