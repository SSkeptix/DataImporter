using DataImporter.Entities;
using DataImporter.FileHandler;
using DataImporter.Services.Impl.Parsers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using Xunit;
using XmlTransaction = DataImporter.Services.Impl.Parsers.XmlTransactionParser.XmlTransaction;
using XmlTransactions = DataImporter.Services.Impl.Parsers.XmlTransactionParser.XmlTransactions;

namespace DataImporter.Tests.Services.Impl.Parsers
{
    public class XmlTransactionParserTests
    {
        private readonly Mock<IXmlFileReader> xmlFileReader;

        private readonly XmlTransactionParser subject;

        public XmlTransactionParserTests()
        {
            var logger = new Mock<ILogger<XmlTransactionParser>>();
            this.xmlFileReader = new Mock<IXmlFileReader>();

            this.subject = new XmlTransactionParser(logger.Object, this.xmlFileReader.Object);
        }

        private XmlTransaction ValidTransaction => new XmlTransaction
        {
            Id = "Invoice0000001",
            PaymentDetails = new XmlTransaction.PaymentDetailsModel
            {
                Amount = "1,000.00",
                CurrencyCode = "USD",
            },
            TransactionDate = "2019-02-20T13:33:16",
            Status = "Done",
        };

        [Fact]
        public void FileExtension_ReturnsXml()
        {
            // Assert
            Assert.Equal(FileExtension.Xml, this.subject.FileExtension);
        }

        [Fact]
        public void Parse_InputDataAreValid_ReturnsMappedData()
        {
            // Arrange
            using (var sourceStream = new MemoryStream())
            {
                var xmlTransactions = new XmlTransactions
                {
                    Transactions = new XmlTransaction[]
                    {
                        new XmlTransaction
                        {
                            Id = "Invoice0000001",
                            PaymentDetails = new XmlTransaction.PaymentDetailsModel
                            {
                                Amount = "1,000.00",
                                CurrencyCode = "USD",
                            },
                            TransactionDate = "2019-02-20T13:33:16",
                            Status = "Done",
                        },
                        new XmlTransaction
                        {
                            Id = "Invoice0000002",
                            PaymentDetails = new XmlTransaction.PaymentDetailsModel
                            {
                                Amount = "300.00",
                                CurrencyCode = "EUR",
                            },
                            TransactionDate = "2019-02-21T02:04:59",
                            Status = "Rejected",
                        },
                    },
                };

                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(xmlTransactions);

                // Act
                var transactions = this.subject.Parse(sourceStream).Result;

                // Assert
                Assert.Equal(xmlTransactions.Transactions[0].Id, transactions[0].Id);
                Assert.Equal(1000, transactions[0].Amount);
                Assert.Equal(CurrencyCode.USD, transactions[0].CurrencyCode);
                Assert.Equal(new DateTime(2019, 2, 20, 13, 33, 16), transactions[0].TransactionDate);
                Assert.Equal(TransactionStatus.Done, transactions[0].Status);

                Assert.Equal(xmlTransactions.Transactions[1].Id, transactions[1].Id);
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
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.Id = id;

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Fact]
        public void Parse_TransactionIdHasMoreThan50Characters_ThrowsArgumentException()
        {
            // Arrange
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.Id = "1234567890 1234567890 1234567890 1234567890 1234567890";

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

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
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.PaymentDetails.Amount = amount;

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

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
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.PaymentDetails.CurrencyCode = currencyCode;

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

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
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.PaymentDetails.CurrencyCode = currencyCode;

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

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
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.TransactionDate = date;

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Fact]
        public void Parse_TransactionDateIsValid_ReturnsMappedData()
        {
            // Arrange
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.TransactionDate = "2019-02-21T22:04:59";

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

                // Act
                var transactions = this.subject.Parse(sourceStream).Result;

                // Assert
                Assert.Equal(new DateTime(2019, 2, 21, 22, 04, 59), transactions[0].TransactionDate);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("gjdfghfdg")]
        public void Parse_TransactionStatusIsNotValid_ThrowsArgumentException(string status)
        {
            // Arrange
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.Status = status;

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.Parse(sourceStream));
            }
        }

        [Theory]
        [InlineData(TransactionStatus.Approved, "Approved")]
        [InlineData(TransactionStatus.Rejected, "Rejected")]
        [InlineData(TransactionStatus.Done, "Done")]
        public void Parse_TransactionStatusIsValid_ReturnsMappedData(TransactionStatus expectedStatus, string status)
        {
            // Arrange
            var xmlTransaction = this.ValidTransaction;
            xmlTransaction.Status = status;

            using (var sourceStream = new MemoryStream())
            {
                this.xmlFileReader.Setup(x => x.ReadFile<XmlTransactions>(sourceStream))
                    .Returns(new XmlTransactions { Transactions = new XmlTransaction[] { xmlTransaction } });

                // Act
                var transactions = this.subject.Parse(sourceStream).Result;

                // Assert
                Assert.Equal(expectedStatus, transactions[0].Status);
            }
        }
    }
}
