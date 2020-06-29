using DataImporter.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataImporter.SqlStorage.Configurations
{
    internal class TransactionConfguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions")
                .HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(10,4)");
        }
    }
}
