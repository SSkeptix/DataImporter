using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.FileHandler.Impl
{
    public class CsvFileReader : ICsvFileReader
    {
        private static readonly CsvConfiguration Configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
        };

        public async Task<T[]> ReadFile<T>(Stream sourceStream)
            where T: class
        {
            using (var textReader = new StreamReader(sourceStream))
            using (var csvReader = new CsvReader(textReader, Configuration))
            {
                var data = csvReader.GetRecordsAsync<T>();
                return await data.ToArrayAsync();
            }
        }
    }
}
