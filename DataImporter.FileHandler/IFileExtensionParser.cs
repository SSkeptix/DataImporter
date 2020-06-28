namespace DataImporter.FileHandler
{
	public interface IFileExtensionParser
	{
		FileExtension ParseFileExtension(string filePath);
	}
}
