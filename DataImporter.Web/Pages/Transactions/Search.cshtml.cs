using DataImporter.Entities;
using DataImporter.Models;
using DataImporter.Repositories;
using DataImporter.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Web.Pages.Transactions
{
    public class SearchModel : PageModel
    {
        private readonly ITransactionRepository transactionRepository;

        public SearchModel(ITransactionRepository transactionRepository)
        {
            this.transactionRepository = transactionRepository;
        }

        [BindProperty(SupportsGet = true)]
        public CurrencyCode? Currency { get; set; }

        [BindProperty(SupportsGet = true)]
        public TransactionStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public TransactionModel[] Transactions { get; private set; }

        public Pagination Pagination { get; private set; }

        public async Task OnGet()
        {
            this.PageNumber = Math.Max(this.PageNumber, 1);

            const int takeCount = 10;

            var result = await this.transactionRepository.SearchTransaction(
                new TransactionSearchOptions
                {
                    CurrencyCode = this.Currency,
                    Status = this.Status,
                    FromDate = this.FromDate,
                    ToDate = this.ToDate,
                    Skip = (this.PageNumber - 1) * takeCount,
                    Take = takeCount,
                });

            this.Transactions = result.Items.Select(x =>
                new TransactionModel
                {
                    Id = x.Id,
                    Payment = $"{x.Amount:0.00} {x.CurrencyCode}",
                    Status = x.Status.ToString().Substring(0, 1),
                })
                .ToArray();

            this.Pagination = new Pagination((page) => $"filter({page})", result.Count, this.PageNumber, takeCount);
        }

        public class TransactionModel
        {
            public string Id { get; set; }

            public string Payment { get; set; }

            public string Status { get; set; }
        }
    }
}