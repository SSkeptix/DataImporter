using DataImporter.FileHandler;
using System.Collections.Generic;
using System.Linq;

namespace DataImporter.Services.Impl
{
    public class FileParserFactory : IFileParserFactory
    {
        private readonly IFileExtensionParser fileExtensionParser;
        private readonly IEnumerable<IFileParser> fileParsers;

        public FileParserFactory(
            IFileExtensionParser fileExtensionParser,
            IEnumerable<IFileParser> fileParsers)
        {
            this.fileExtensionParser = fileExtensionParser;
            this.fileParsers = fileParsers;
        }

        public IFileParser<T> GetParser<T>(string filePath)
            where T : class
        {
            var fileExtension = this.fileExtensionParser.ParseFileExtension(filePath);

            var parser = fileParsers.Single(x => x.FileExtension == fileExtension && x.DataType == typeof(T));
            return (IFileParser<T>)parser;
        }
    }
}
