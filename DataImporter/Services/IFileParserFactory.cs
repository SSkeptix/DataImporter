namespace DataImporter.Services
{
	public interface IFileParserFactory
	{
		IFileParser<T> GetParser<T>(string filePath) where T : class;
	}
}
