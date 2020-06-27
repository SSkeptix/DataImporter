using DataImporter.FileHandler;
using DataImporter.FileHandler.Impl;
using System;
using Xunit;

namespace DataImporter.Tests.FileHandler.Impl
{
	public class FileExtensionParserTests
	{
		private readonly FileExtensionParser fileExtensionParser;

		public FileExtensionParserTests()
		{
			this.fileExtensionParser = new FileExtensionParser();
		}

		[Theory]
		[InlineData(FileExtension.Xml, "fileName.xml")]
		[InlineData(FileExtension.Csv, "fileName.csv")]
		[InlineData(FileExtension.Csv, "fileName.CSV")]
		[InlineData(FileExtension.Csv, "fileName.Csv")]
		[InlineData(FileExtension.Csv, "file Name.csv")]
		[InlineData(FileExtension.Csv, "fileName .csv")]
		[InlineData(FileExtension.Csv, " fileName.csv")]
		[InlineData(FileExtension.Csv, "filePath/fileName.csv")]
		[InlineData(FileExtension.Csv, "file Path/file  Name .csv")]
		[InlineData(FileExtension.Csv, @"filePath\fileName.csv")]
		[InlineData(FileExtension.Csv, @"file Path \file Name .csv")]
		public void ParseFileExtension_ExtensionIsSupported_ReturnProperExtension(FileExtension expectedFileExtension, string filePath)
		{
			// Act
			var fileExtension = this.fileExtensionParser.ParseFileExtension(filePath);

			// Assert
			Assert.Equal(expectedFileExtension, fileExtension);
		}

		[Theory]
		[InlineData(@"fileName")]
		[InlineData(@"file Name")]
		[InlineData(@"filePath\fileName")]
		[InlineData(@"file Path \file Name")]
		[InlineData(@"filePath\fileName.")]
		[InlineData(@"filePath\fileName .")]
		[InlineData(@"filePath\fileName.json")]
		[InlineData(@"file Path \file Name .json")]
		[InlineData(@"filePath/fileName")]
		[InlineData(@"file Path /file Name")]
		[InlineData(@"filePath/fileName.")]
		[InlineData(@"filePath/fileName .")]
		[InlineData(@"filePath/fileName.json")]
		[InlineData(@"file Path /file Name .json")]
		public void ParseFileExtension_ExtensionIsNotSupported_ThrowArgumentException(string filePath)
		{
			// Act & Assert
			Assert.Throws<ArgumentException>(() => this.fileExtensionParser.ParseFileExtension(filePath));
		}
	}
}
