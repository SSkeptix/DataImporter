using DataImporter.Entities;
using DataImporter.FileHandler;
using DataImporter.Models;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataImporter.Services.Impl.Parsers
{
    public class XmlTransactionParser : FileParser<Transaction>
    {
        private readonly ILogger<XmlTransactionParser> logger;
        private readonly IXmlFileReader xmlFileReader;

        public XmlTransactionParser(
            ILogger<XmlTransactionParser> logger,
            IXmlFileReader xmlFileReader)
        {
            this.logger = logger;
            this.xmlFileReader = xmlFileReader;
        }

        public override FileExtension FileExtension => FileExtension.Xml;

        public override Task<Transaction[]> Parse(Stream stream)
        {
            var xmlTransactions = this.xmlFileReader.ReadFile<XmlTransactions>(stream).Transactions;

            if (this.IsArrayValid(xmlTransactions, ValidationRules, out var error))
            {
                var transactions = xmlTransactions.Select(this.Map).ToArray();
                this.logger.LogTrace($"{nameof(XmlTransactionParser)} successfully parsed {transactions.Length} transactions");
                return Task.FromResult(transactions);
            }
            else
            {
                this.logger.LogInformation("{nameof(XmlTransactionParser)} found invalid transactions during parsing. " +
                    $"Details: {Environment.NewLine}{error}");
                return Task.FromException<Transaction[]>(new ParsingException(error));
            }
        }

        private const string TransactionDateFormat = "yyyy-MM-ddTHH:mm:ss";

        private static readonly ValidationRule<XmlTransaction>[] ValidationRules = new ValidationRule<XmlTransaction>[]
        {
            new ValidationRule<XmlTransaction>
            {
                Rule = x => (x.Id?.Length ?? 0) <= 50,
                ErrorMessage = "Transaction Id has more than 50 characters",
            },
            new ValidationRule<XmlTransaction>
            {
                Rule = x => !string.IsNullOrEmpty(x.Id),
                ErrorMessage = "Transaction Id is empty",
            },
            new ValidationRule<XmlTransaction>
            {
                Rule = x => decimal.TryParse(x.PaymentDetails.Amount, out _),
                ErrorMessage = "Amount has invalid value or empty",
            },
            new ValidationRule<XmlTransaction>
            {
                Rule = x => EnumHelper.TryParse<CurrencyCode>(x.PaymentDetails.CurrencyCode, StringComparison.InvariantCulture, out _),
                ErrorMessage = "Currency Code has invalid value or empty",
            },
            new ValidationRule<XmlTransaction>
            {
                Rule = x => DateTime.TryParseExact(x.TransactionDate, TransactionDateFormat, null, DateTimeStyles.None, out _),
                ErrorMessage = "Transaction Date has invalid value or empty",
            },
            new ValidationRule<XmlTransaction>
            {
                Rule = x => EnumHelper.TryParse<TransactionStatus>(x.Status, StringComparison.InvariantCulture, out _),
                ErrorMessage = "Status has invalid value or empty",
            },
        };

        private Transaction Map(XmlTransaction xmlTransaction)
        {
            return new Transaction
            {
                Id = xmlTransaction.Id,
                Amount = decimal.Parse(xmlTransaction.PaymentDetails.Amount),
                CurrencyCode = EnumHelper.Parse<CurrencyCode>(xmlTransaction.PaymentDetails.CurrencyCode, StringComparison.InvariantCulture),
                TransactionDate = DateTime.ParseExact(xmlTransaction.TransactionDate, TransactionDateFormat, null),
                Status = EnumHelper.Parse<TransactionStatus>(xmlTransaction.Status, StringComparison.InvariantCulture),
            };
        }

        [XmlRoot(ElementName = "Transactions")]
        internal class XmlTransactions
        {
            [XmlElement("Transaction")]
            public XmlTransaction[] Transactions { get; set; }
        }

        internal class XmlTransaction
        {
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }

            public string TransactionDate { get; set; }

            [XmlElement]
            public PaymentDetailsModel PaymentDetails { get; set; }

            public string Status { get; set; }

            public class PaymentDetailsModel
            {
                public string Amount { get; set; }

                public string CurrencyCode { get; set; }
            }
        }
    }
}
