using DataImporter.DataAccess.Models;
using DataImporter.Domain.Entities;
using System.Threading.Tasks;

namespace DataImporter.DataAccess.Repositories
{
    public interface ITransactionRepository
    {
        Task InsertTransaction(params Transaction[] transactions);

        Task<SearchResult<Transaction>> SearchTransaction(TransactionSearchOptions searchOptions);
    }
}
