using DataImporter.Entities;
using DataImporter.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace DataImporter.Services.Impl
{
	public class TransactionService : ITransactionService
	{
		private readonly IFileParserFactory fileParserFactory;
		private readonly ILogger logger;
		private readonly ITransactionRepository transactionRepository;

		public TransactionService(
			IFileParserFactory fileParserFactory,
			ILogger logger,
			ITransactionRepository transactionRepository)
		{
			this.fileParserFactory = fileParserFactory;
			this.logger = logger;
			this.transactionRepository = transactionRepository;
		}

		public async Task ImportTransactionsFromFile(Stream stream, string fileName)
		{
			var parser = this.fileParserFactory.GetParser<Transaction>(fileName);
			var transactions = await parser.Parse(stream);

			try
			{
				await this.transactionRepository.InsertTransaction(transactions);
			}
			catch (Exception ex)
			{
				this.logger.LogWarning(ex, "Error writing to the database.");
				throw new DataException("Error writing to the database.", ex);
			}
		}
	}
}
