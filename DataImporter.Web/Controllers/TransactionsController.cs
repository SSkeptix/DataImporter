using DataImporter.DataAccess.Models;
using DataImporter.DataAccess.Repositories;
using DataImporter.FileHandler;
using DataImporter.Models;
using DataImporter.Services;
using DataImporter.Web.Helpers;
using DataImporter.Web.Models.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Web.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ILogger<TransactionsController> logger;
        private readonly ITransactionRepository transactionRepository;
        private readonly ITransactionService transactionService;

        public TransactionsController(
            ILogger<TransactionsController> logger,
            ITransactionService transactionService,
            ITransactionRepository transactionRepository)
        {
            this.logger = logger;
            this.transactionService = transactionService;
            this.transactionRepository = transactionRepository;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportPost()
        {
            try
            {
                var file = this.Request.Form.Files.First(x => x.Length > 0);

                using (var stream = file.OpenReadStream())
                {
                    await this.transactionService.ImportTransactionsFromFile(stream, file.FileName);
                }

                return new OkResult();
            }
            catch (ParsingException ex)
            {
                var msg = ex.Message
                    + (ex.InnerException != null
                        ? " Details: " + ex.InnerException.Message
                        : string.Empty)
                    + ".";

                this.logger.LogTrace(ex, $"Error Parsing data.");
                return new BadRequestObjectResult(new { Error = HtmlHelper.ToHtml(msg) });
            }
            catch (FileExtensionException ex)
            {
                this.logger.LogWarning(ex, $"Error {typeof(TransactionsController).FullName} {nameof(this.ImportPost)}()");
                return new BadRequestObjectResult(new { Error = "Unsupported file type." });
            }
            catch (DataException ex)
            {
                this.logger.LogWarning(ex, $"Error {typeof(TransactionsController).FullName} {nameof(this.ImportPost)}()");
                return new BadRequestObjectResult(new { Error = "Error writing transactions to the data base." });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error {typeof(TransactionsController).FullName} {nameof(this.ImportPost)}()");
                return new BadRequestObjectResult(new { Error = "Internal server error." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery]SearchOptionsModel searchOptions)
        {
            searchOptions.PageNumber = Math.Max(searchOptions.PageNumber, 1);

            const int takeCount = 10;

            var result = await this.transactionRepository.SearchTransaction(
                new TransactionSearchOptions
                {
                    CurrencyCode = searchOptions.Currency,
                    Status = searchOptions.Status,
                    FromDate = searchOptions.FromDate,
                    ToDate = searchOptions.ToDate,
                    Skip = (searchOptions.PageNumber - 1) * takeCount,
                    Take = takeCount,
                });

            return this.View(new SearchViewModel
            {
                SearchOptions = searchOptions,
                Transactions = result.Items.Select(x =>
                    new TransactionModel
                    {
                        Id = x.Id,
                        Payment = $"{x.Amount:0.00} {x.CurrencyCode}",
                        Status = x.Status.ToString().Substring(0, 1),
                    })
                    .ToArray(),
                Pagination = new Pagination((page) => $"filter({page})", result.Count, searchOptions.PageNumber, takeCount),
            });
        }
    }
}
