using DataImporter.Entities;
using DataImporter.FileHandler;
using DataImporter.Services.Impl.Parsers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using Xunit;
using CsvTransaction = DataImporter.Services.Impl.Parsers.CsvTransactionParser.CsvTransaction;

namespace DataImporter.Tests.Services.Impl.Parsers
{
    public class CsvTransactionParserTests
    {
        private readonly Mock<ICsvFileReader> csvFileReader;

        private readonly CsvTransactionParser subject;

        public CsvTransactionParserTests()
        {
            this.csvFileReader = new Mock<ICsvFileReader>();
            var logger = new Mock<ILogger>();

            this.subject = new CsvTransactionParser(logger.Object, this.csvFileReader.Object);
        }

        private CsvTransaction ValidTransaction => new CsvTransaction
        {
            Id = "Invoice0000001",
            Amount = "1,000.00",
            CurrencyCode = "USD",
            TransactionDate = "20/02/2019 13:33:16",
            Status = "Approved",
        };

        [Fact]
        public void FileExtension_ReturnsCsv()
        {
            // Assert
            Assert.Equal(FileExtension.Csv, this.subject.FileExtension);
        }

        [Fact]
        public void Parse_InputDataAreValid_ReturnsMappedData()
        {
            // Arrange
            using (var sourceStream = new MemoryStream())
            {
                var csvTransactions = new CsvTransaction[]
                {
                    new CsvTransaction
                    {
                        Id = "Invoice0000001",
                        Amount = "1,000.00",
                        CurrencyCode = "USD",
                        TransactionDate = "20/02/2019 13:33:16",
                        Status = "Approved",
                    },
                    new CsvTransaction
                    {
                        Id = "Invoice0000002",
                        Amount = "300.00",
                        CurrencyCode = "EUR",
                        TransactionDate = "21/02/2019 02:04:59",
                        Status = "Failed",
                    },
                };

                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(csvTransactions);

                // Act
                var transactions = this.subject.Parse(sourceStream).Result;

                // Assert
                Assert.Equal(csvTransactions[0].Id, transactions[0].Id);
                Assert.Equal(1000, transactions[0].Amount);
                Assert.Equal(CurrencyCode.USD, transactions[0].CurrencyCode);
                Assert.Equal(new DateTime(2019, 2, 20, 13, 33, 16), transactions[0].TransactionDate);
                Assert.Equal(TransactionStatus.Approved, transactions[0].Status);

                Assert.Equal(csvTransactions[1].Id, transactions[1].Id);
                Assert.Equal(300, transactions[1].Amount);
                Assert.Equal(CurrencyCode.EUR, transactions[1].CurrencyCode);
                Assert.Equal(new DateTime(2019, 2, 21, 2, 4, 59), transactions[1].TransactionDate);
                Assert.Equal(TransactionStatus.Rejected, transactions[1].Status);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Parse_TransactionIdIsNull_ThrowsArgumentException(string id)
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.Id = id;

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Fact]
        public void Parse_TransactionIdHasMoreThan50Characters_ThrowsArgumentException()
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.Id = "1234567890 1234567890 1234567890 1234567890 1234567890";

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("xsdgvfdsfgvd")]
        public void Parse_TransactionAmountIsNotValid_ThrowsArgumentException(string amount)
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.Amount = amount;

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Usd")]
        [InlineData("usd")]
        [InlineData("currency")]
        public void Parse_TransactionCurrencyCodeIsNotValid_ThrowsArgumentException(string currencyCode)
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.CurrencyCode = currencyCode;

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Theory]
        [InlineData(CurrencyCode.USD, "USD")]
        [InlineData(CurrencyCode.EUR, "EUR")]
        public void Parse_TransactionCurrencyCodeIsValid_ReturnsMappedData(CurrencyCode expectedCurrencyCode, string currencyCode)
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.CurrencyCode = currencyCode;

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act
                var transactions = this.subject.Parse(sourceStream).Result;

                // Assert
                Assert.Equal(expectedCurrencyCode, transactions[0].CurrencyCode);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("gjdfghfdg")]
        [InlineData("2019-01-23T13:45:10")]
        public void Parse_TransactionDateIsNotValid_ThrowsArgumentException(string date)
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.TransactionDate = date;

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Fact]
        public void Parse_TransactionDateIsValid_ReturnsMappedData()
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.TransactionDate = "20/02/2019 13:33:16";

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act
                var transactions = this.subject.Parse(sourceStream).Result;

                // Assert
                Assert.Equal(new DateTime(2019, 2, 20, 13, 33, 16), transactions[0].TransactionDate);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("gjdfghfdg")]
        public void Parse_TransactionStatusIsNotValid_ThrowsArgumentException(string status)
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.Status = status;

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Theory]
        [InlineData(TransactionStatus.Approved, "Approved")]
        [InlineData(TransactionStatus.Rejected, "Failed")]
        [InlineData(TransactionStatus.Done, "Finished")]
        public void Parse_TransactionStatusIsValid_ReturnsMappedData(TransactionStatus expectedStatus, string status)
        {
            // Arrange
            var csvTransaction = ValidTransaction;
            csvTransaction.Status = status;

            using (var sourceStream = new MemoryStream())
            {
                this.csvFileReader.Setup(x => x.ReadFile<CsvTransaction>(sourceStream))
                    .ReturnsAsync(new CsvTransaction[] { csvTransaction });

                // Act
                var transactions = this.subject.Parse(sourceStream).Result;

                // Assert
                Assert.Equal(expectedStatus, transactions[0].Status);
            }
        }
    }
}
