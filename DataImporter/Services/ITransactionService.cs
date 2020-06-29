using System.IO;
using System.Threading.Tasks;

namespace DataImporter.Services
{
    public interface ITransactionService
    {
        Task ImportTransactionsFromFile(Stream stream, string fileName);
    }
}
