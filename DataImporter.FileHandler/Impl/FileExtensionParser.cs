using System;
using System.IO;

namespace DataImporter.FileHandler.Impl
{
    public class FileExtensionParser : IFileExtensionParser
    {
        public FileExtension ParseFileExtension(string filePath)
        {
            var fileExtensionString = Path.GetExtension(filePath);

            return (!string.IsNullOrEmpty(fileExtensionString)
                    && Enum.TryParse<FileExtension>(fileExtensionString.Substring(1), true, out var fileExtension))
                ? fileExtension
                : throw new FileExtensionException("Unknown File Extension");
        }
    }
}
