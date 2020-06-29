using System;

namespace DataImporter.Entities
{
	public class Transaction
	{
		public string Id { get; set; }

		public decimal Amount { get; set; }

		public CurrencyCode CurrencyCode { get; set; }

		public DateTime TransactionDate { get; set; }

		public TransactionStatus Status { get; set; }
	}
}
