using System;

namespace DataImporter.FileHandler
{
    public class FileExtensionException : Exception
    {
        public FileExtensionException()
        { }

        public FileExtensionException(string message)
            : base(message)
        { }

        public FileExtensionException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
