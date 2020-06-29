using AutoFixture;
using DataImporter.FileHandler;
using DataImporter.Services;
using DataImporter.Services.Impl;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace DataImporter.Tests.Services.Impl
{
    public class FileParserFactoryTests
    {
        private readonly Mock<IFileExtensionParser> fileExtensionParser;
        private readonly Mock<IFileParser<TestEntityA>> csvTestEntityAParser;
        private readonly Mock<IFileParser<TestEntityA>> xmlTestEntityAParser;
        private readonly Mock<IFileParser<TestEntityB>> csvTestEntityBParser;
        private readonly Mock<IFileParser<TestEntityB>> xmlTestEntityBParser;

        private readonly IFixture fixture;
        private readonly FileParserFactory subject;

        public FileParserFactoryTests()
        {
            this.fixture = new Fixture();

            this.fileExtensionParser = new Mock<IFileExtensionParser>();

            this.csvTestEntityAParser = new Mock<IFileParser<TestEntityA>>();
            this.csvTestEntityAParser.Setup(x => x.DataType).Returns(typeof(TestEntityA));
            this.csvTestEntityAParser.Setup(x => x.FileExtension).Returns(FileExtension.Csv);

            this.xmlTestEntityAParser = new Mock<IFileParser<TestEntityA>>();
            this.xmlTestEntityAParser.Setup(x => x.DataType).Returns(typeof(TestEntityA));
            this.xmlTestEntityAParser.Setup(x => x.FileExtension).Returns(FileExtension.Xml);

            this.csvTestEntityBParser = new Mock<IFileParser<TestEntityB>>();
            this.csvTestEntityBParser.Setup(x => x.DataType).Returns(typeof(TestEntityB));
            this.csvTestEntityBParser.Setup(x => x.FileExtension).Returns(FileExtension.Csv);

            this.xmlTestEntityBParser = new Mock<IFileParser<TestEntityB>>();
            this.xmlTestEntityBParser.Setup(x => x.DataType).Returns(typeof(TestEntityB));
            this.xmlTestEntityBParser.Setup(x => x.FileExtension).Returns(FileExtension.Xml);

            var parsers = new List<IFileParser>
            {
                csvTestEntityAParser.Object,
                xmlTestEntityAParser.Object,
                csvTestEntityBParser.Object,
                xmlTestEntityBParser.Object,
            };

            this.subject = new FileParserFactory(fileExtensionParser.Object, parsers);
        }

        [Theory]
        [InlineData(FileExtension.Csv)]
        [InlineData(FileExtension.Xml)]
        public void GetParser_ReturnTestEntityBParser(FileExtension fileExtension)
        {
            // Arrange
            var fileName = this.fixture.Create<string>();
            fileExtensionParser
                .Setup(x => x.ParseFileExtension(fileName))
                .Returns(fileExtension);

            // Act
            var parser = this.subject.GetParser<TestEntityB>(fileName);

            // Assert
            Assert.Equal(typeof(TestEntityB), parser.DataType);
            Assert.Equal(fileExtension, parser.FileExtension);
        }

        public class TestEntityA { }
        public class TestEntityB { }
    }
}
