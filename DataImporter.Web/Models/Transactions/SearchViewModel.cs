using DataImporter.Web.Helpers;

namespace DataImporter.Web.Models.Transactions
{
    public class SearchViewModel
    {
        public SearchOptionsModel SearchOptions { get; set; }

        public TransactionModel[] Transactions { get; set; }

        public Pagination Pagination { get; set; }
    }
}
