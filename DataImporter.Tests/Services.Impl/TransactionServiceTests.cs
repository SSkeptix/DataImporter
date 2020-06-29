using AutoFixture;
using DataImporter.Entities;
using DataImporter.Repositories;
using DataImporter.Services;
using DataImporter.Services.Impl;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DataImporter.Tests.Services.Impl
{
    public class TransactionServiceTests
    {
        private readonly Mock<IFileParserFactory> fileParserFactory;
        private readonly Mock<ITransactionRepository> transactionRepository;

        private readonly IFixture fixture;
        private readonly TransactionService subject;

        public TransactionServiceTests()
        {
            this.fixture = new Fixture();

            this.fileParserFactory = new Mock<IFileParserFactory>();
            var logger = new Mock<ILogger<TransactionService>>();
            this.transactionRepository = new Mock<ITransactionRepository>();

            this.subject = new TransactionService(this.fileParserFactory.Object, logger.Object, this.transactionRepository.Object);
        }

        [Fact]
        public void ImportTransaction_TransactionAreValid_PassTransactionsToRepository()
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
                var transactions = this.fixture.CreateMany<Transaction>().ToArray();
                var fileName = this.fixture.Create<string>();

                var parser = new Mock<IFileParser<Transaction>>();
                parser.Setup(x => x.Parse(stream)).ReturnsAsync(transactions);

                this.fileParserFactory.Setup(x => x.GetParser<Transaction>(fileName)).Returns(parser.Object);

                this.transactionRepository.Setup(x => x.InsertTransaction(transactions)).Returns(Task.CompletedTask);

                // Act
                this.subject.ImportTransactionsFromFile(stream, fileName).Wait();

                // Assert
                this.transactionRepository.Verify(x => x.InsertTransaction(transactions));
                this.transactionRepository.VerifyNoOtherCalls();
            }
        }

        [Fact]
        public void ImportTransaction_ParserThownsExceptions_ThownsExceptions()
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
                var exception = new ArgumentException();
                var fileName = this.fixture.Create<string>();

                var parser = new Mock<IFileParser<Transaction>>();
                parser.Setup(x => x.Parse(stream)).ThrowsAsync(exception);

                this.fileParserFactory.Setup(x => x.GetParser<Transaction>(fileName)).Returns(parser.Object);

                // Act & Assert
                Assert.ThrowsAsync<ArgumentException>(() => this.subject.ImportTransactionsFromFile(stream, fileName));
                this.transactionRepository.VerifyNoOtherCalls();
            }
        }

        [Fact]
        public void ImportTransaction_RepositoryThownsExceptions_ThownsExceptions()
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
                var exception = new Exception();
                var transactions = this.fixture.CreateMany<Transaction>().ToArray();
                var fileName = this.fixture.Create<string>();

                var parser = new Mock<IFileParser<Transaction>>();
                parser.Setup(x => x.Parse(stream)).ReturnsAsync(transactions);

                this.fileParserFactory.Setup(x => x.GetParser<Transaction>(fileName)).Returns(parser.Object);

                this.transactionRepository.Setup(x => x.InsertTransaction(transactions)).ThrowsAsync(exception);

                // Act & Assert
                Assert.ThrowsAsync<DataException>(() => this.subject.ImportTransactionsFromFile(stream, fileName));
            }
        }
    }
}
