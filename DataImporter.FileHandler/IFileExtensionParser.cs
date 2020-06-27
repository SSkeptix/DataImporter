namespace DataImporter.FileHandler
{
	interface IFileExtensionParser
	{
		FileExtension ParseFileExtension(string filePath);
	}
}
