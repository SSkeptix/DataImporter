using DataImporter.Domain.Enums;
using System;

namespace DataImporter.Web.Models.Transactions
{
    public class SearchOptionsModel
    {
        public CurrencyCode? Currency { get; set; }

        public TransactionStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;
    }
}
