using System.IO;

namespace DataImporter.FileHandler
{
	public interface IXmlFileReader
	{
		T ReadFile<T>(Stream sourceStream) where T : class;
	}
}
