using System.IO;
using System.Threading.Tasks;

namespace DataImporter.FileHandler
{
	public interface ICsvFileReader
	{
		Task<T[]> ReadFile<T>(Stream sourceStream) where T : class;
	}
}
