using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XsdToCsv.Tests
{
    [TestClass]
    public class TestCsvXsdLineBuilder
    {
        [TestMethod]
        public void BuildHeader_ReturnsHeader()
        {
            // Arrange
            var sut = new CsvXsdLineBuilder("  ", ";");

            // Act
            var header = sut.BuildHeader();

            // Assert
            header.ShouldBeEquivalentTo("Elementnaam;Multi;Type;Omschrijving;Voorbeeld\n");
        }

        [TestMethod]
        public void BuildLine_ReturnsOneNumberForMultiplicityWhenMinOccursEqualsMaxOccurs()
        {
            // Arrange
            var sut = new CsvXsdLineBuilder("  ", ";");

            // Act
            var line = sut.BuildLine(0, "name", 1, 1, "type");

            // Assert
            line.ShouldBeEquivalentTo("\"name\";\"1\";\"type\";;\n");
        }

        [TestMethod]
        public void BuildLine_ReturnsRangeForMultiplicityWhenMinOccursInequalsMaxOccurs()
        {
            // Arrange
            var sut = new CsvXsdLineBuilder("  ", ";");

            // Act
            var line = sut.BuildLine(0, "name", 1, 2, "type");

            // Assert
            line.ShouldBeEquivalentTo("\"name\";\"1..2\";\"type\";;\n");
        }

        [TestMethod]
        public void BuildLine_ReturnsRightSideAsteriskForMultiplicity()
        {
            // Arrange
            var sut = new CsvXsdLineBuilder("  ", ";");

            // Act
            var line = sut.BuildLine(0, "name", 1, Decimal.MaxValue, "type");

            // Assert
            line.ShouldBeEquivalentTo("\"name\";\"1..*\";\"type\";;\n");
        }

        [TestMethod]
        public void BuildLine_ReturnsSingleAsteriskForMultiplicity()
        {
            // Arrange
            var sut = new CsvXsdLineBuilder("  ", ";");

            // Act
            var line = sut.BuildLine(0, "name", 0, Decimal.MaxValue, "type");

            // Assert
            line.ShouldBeEquivalentTo("\"name\";\"*\";\"type\";;\n");
        }
    }
}