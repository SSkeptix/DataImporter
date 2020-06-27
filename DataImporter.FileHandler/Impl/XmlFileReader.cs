using System.IO;
using System.Xml.Serialization;

namespace DataImporter.FileHandler.Impl
{
	public class XmlFileReader
	{
		public T ReadFile<T>(Stream sourceStream)
			where T: class
		{
			var xmlSerializer = new XmlSerializer(typeof(T));
			return (xmlSerializer.Deserialize(sourceStream) as T);
		}
	}
}
