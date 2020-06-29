﻿using DataImporter.Entities;
using DataImporter.FileHandler;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataImporter.Services.Impl.Parsers
{
    public class CsvTransactionParser : FileParser<Transaction>
    {
        private readonly ILogger logger;
        private readonly ICsvFileReader csvFileReader;

        public CsvTransactionParser(
            ILogger logger,
            ICsvFileReader csvFileReader)
        {
            this.logger = logger;
            this.csvFileReader = csvFileReader;
        }

        public override FileExtension FileExtension => FileExtension.Csv;

        public override async Task<Transaction[]> Parse(Stream stream)
        {
            var csvTransactions = await this.csvFileReader.ReadFile<CsvTransaction>(stream);

            if (this.IsArrayValid(csvTransactions, ValidationRules, out var error))
            {
                var transactions = csvTransactions.Select(this.Map).ToArray();
                this.logger.LogTrace($"{nameof(CsvTransactionParser)} successfully parsed {transactions.Length} transactions");
                return transactions;
            }
            else
            {
                this.logger.LogInformation("{nameof(CsvTransactionParser)} found invalid transactions during parsing. " +
                    $"Details: {Environment.NewLine}{error}");
                throw new ArgumentException(error);
            }
        }

        private const string TransactionDateFormat = "dd/MM/yyyy HH:mm:ss";

        private static readonly ValidationRule<CsvTransaction>[] ValidationRules = new ValidationRule<CsvTransaction>[]
        {
            new ValidationRule<CsvTransaction>
            {
                Rule = x => (x.Id?.Length ?? 0) <= 50,
                ErrorMessage = "Transaction Identificator has more than 50 characters",
            },
            new ValidationRule<CsvTransaction>
            {
                Rule = x => !string.IsNullOrEmpty(x.Id),
                ErrorMessage = "Transaction Identificator is empty",
            },
            new ValidationRule<CsvTransaction>
            {
                Rule = x => decimal.TryParse(x.Amount, out _),
                ErrorMessage = "Amount has invalid value or empty",
            },
            new ValidationRule<CsvTransaction>
            {
                Rule = x => EnumHelper.TryParse<CurrencyCode>(x.CurrencyCode, StringComparison.InvariantCulture, out _),
                ErrorMessage = "Currency Code has invalid value or empty",
            },
            new ValidationRule<CsvTransaction>
            {
                Rule = x => DateTime.TryParseExact(x.TransactionDate, TransactionDateFormat, null, DateTimeStyles.None, out _),
                ErrorMessage = "Transaction Date has invalid value or empty",
            },
            new ValidationRule<CsvTransaction>
            {
                Rule = x => EnumHelper.TryParse<CsvTransactionStatus>(x.Status, StringComparison.InvariantCulture, out _),
                ErrorMessage = "Status has invalid value or empty",
            },
        };

        private Transaction Map(CsvTransaction csvTransaction)
        {
            return new Transaction
            {
                Id = csvTransaction.Id,
                Amount = decimal.Parse(csvTransaction.Amount),
                CurrencyCode = EnumHelper.Parse<CurrencyCode>(csvTransaction.CurrencyCode, StringComparison.InvariantCulture),
                TransactionDate = DateTime.ParseExact(csvTransaction.TransactionDate, TransactionDateFormat, null),
                Status = EnumHelper.Parse<CsvTransactionStatus>(csvTransaction.Status, StringComparison.InvariantCulture) switch
                {
                    CsvTransactionStatus.Approved => TransactionStatus.Approved,
                    CsvTransactionStatus.Failed => TransactionStatus.Rejected,
                    CsvTransactionStatus.Finished => TransactionStatus.Done,
                    _ => throw new ArgumentException("Unknown transaction status"),
                },
            };
        }

        internal class CsvTransaction
        {
            public string Id { get; set; }

            public string Amount { get; set; }

            public string CurrencyCode { get; set; }

            public string TransactionDate { get; set; }

            public string Status { get; set; }
        }

        internal enum CsvTransactionStatus
        {
            Approved,
            Failed,
            Finished,
        }
    }
}
