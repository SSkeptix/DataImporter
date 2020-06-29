using DataImporter.Entities;
using DataImporter.Models;
using Infrastructure;
using System.Threading.Tasks;

namespace DataImporter.Repositories
{
    public interface ITransactionRepository
    {
        Task InsertTransaction(params Transaction[] transactions);

        Task<SearchResult<Transaction>> SearchTransaction(TransactionSearchOptions searchOptions);
    }
}
