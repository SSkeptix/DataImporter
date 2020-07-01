using DataImporter.DataAccess.Models;
using DataImporter.DataAccess.Repositories;
using DataImporter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.DataAccess.Impl.MsSql.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext db;

        public TransactionRepository(AppDbContext db)
        {
            this.db = db;
        }

        public Task InsertTransaction(params Transaction[] transactions)
        {
            this.db.Transactions.AddRange(transactions);
            return this.db.SaveChangesAsync();
        }

        public async Task<SearchResult<Transaction>> SearchTransaction(TransactionSearchOptions searchOptions)
        {
            IQueryable<Transaction> query = this.db.Transactions;

            if (searchOptions.CurrencyCode.HasValue)
            {
                query = query.Where(x => x.CurrencyCode == searchOptions.CurrencyCode.Value);
            }

            if (searchOptions.Status.HasValue)
            {
                query = query.Where(x => x.Status == searchOptions.Status.Value);
            }

            if (searchOptions.FromDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= searchOptions.FromDate.Value);
            }

            if (searchOptions.ToDate.HasValue)
            {
                query = query.Where(x => x.TransactionDate <= searchOptions.ToDate.Value);
            }

            return new SearchResult<Transaction>
            {
                Count = await query.CountAsync(),
                Items = await query
                    .Skip(searchOptions.Skip)
                    .Take(searchOptions.Take)
                    .ToArrayAsync(),
            };
        }
    }
}
