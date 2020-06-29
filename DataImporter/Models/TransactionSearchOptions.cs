using DataImporter.Entities;
using System;

namespace DataImporter.Models
{
	public class TransactionSearchOptions
	{
		public CurrencyCode? CurrencyCode { get; set; }

		public TransactionStatus? Status { get; set; }

		public DateTime? FromDate { get; set; }

		public DateTime? ToDate { get; set; }

		public int Skip { get; set; } = 0;

		public int Take { get; set; } = 20;
	}
}
