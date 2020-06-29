﻿using System;
using System.IO;
using System.Linq;

namespace DataImporter.FileHandler.Impl
{
    public class FileExtensionParser : IFileExtensionParser
    {
        public FileExtension ParseFileExtension(string filePath)
        {
            var fileExtensionString = Path.GetExtension(filePath);

            var avaliableFileExtensions = Enum.GetValues(typeof(FileExtension))
                .Cast<FileExtension>();

            var fileExtensionQuery = avaliableFileExtensions
                .Where(x => string.Equals(
                    fileExtensionString,
                    "." + x.ToString(),
                    StringComparison.InvariantCultureIgnoreCase));

            return fileExtensionQuery.Any()
                ? fileExtensionQuery.First()
                : throw new FileExtensionException("Unknown File Extension");
        }
    }
}
