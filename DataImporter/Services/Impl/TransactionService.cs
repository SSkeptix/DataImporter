using DataImporter.Entities;
using DataImporter.Repositories;
using System.IO;
using System.Threading.Tasks;

namespace DataImporter.Services.Impl
{
	public class TransactionService : ITransactionService
	{
		private readonly IFileParserFactory fileParserFactory;
		private readonly ITransactionRepository transactionRepository;

		public TransactionService(
			IFileParserFactory fileParserFactory,
			ITransactionRepository transactionRepository)
		{
			this.fileParserFactory = fileParserFactory;
			this.transactionRepository = transactionRepository;
		}

		public async Task ImportTransactionsFromFile(Stream stream, string fileName)
		{
			var parser = this.fileParserFactory.GetParser<Transaction>(fileName);
			var transactions = await parser.Parse(stream);
			await this.transactionRepository.InsertTransaction(transactions);
		}
	}
}
