using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Test.Api.Data;

public sealed class TodoEntityConfiguration : IEntityTypeConfiguration<TodoEntity>
{
    public void Configure(EntityTypeBuilder<TodoEntity> builder)
    {
        builder.ToTable("todos", "todos");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .IsRequired(true);

        builder.Property(e => e.Title)
            .HasMaxLength(1000)
            .IsRequired(true);

        builder.Property(e => e.DueBy)
            .HasColumnType("date");

        builder.Property(e => e.IsComplete)
            .HasColumnType("boolean")
            .IsRequired(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(true);
    }
}