using DataImporter.FileHandler;
using DataImporter.Models;
using DataImporter.Services;
using DataImporter.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Web.Pages.Transactions
{
    public class ImportModel : PageModel
    {
        private readonly ILogger<ImportModel> logger;
        private readonly ITransactionService transactionService;

        public ImportModel(
            ILogger<ImportModel> logger,
            ITransactionService transactionService)
        {
            this.logger = logger;
            this.transactionService = transactionService;
        }

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost()
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
                this.logger.LogWarning(ex, $"Error {typeof(ImportModel).FullName} OnPost()");
                return new BadRequestObjectResult(new { Error = "Unsupported file type." });
            }
            catch (DataException ex)
            {
                this.logger.LogWarning(ex, $"Error {typeof(ImportModel).FullName} OnPost()");
                return new BadRequestObjectResult(new { Error = "Error writing transactions to the data base." });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error {typeof(ImportModel).FullName} OnPost()");
                return new BadRequestObjectResult(new { Error = "Internal server error." });
            }
        }
    }
}