using ImageProcessor.Features.File.Data;
using ImageProcessor.Features.File.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProcessedFile = ImageProcessor.Features.File.Data.ProcessedFile;

namespace ImageProcessor.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext(options)
{
    public DbSet<ProcessedFile> ProcessedFile { get; set; }
    public DbSet<ProcessedFileStatus> ProcessedFileStatus { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //
        builder.Entity<ProcessedFileStatus>().Property(e => e.Id).HasConversion<int>();

        builder
            .Entity<ProcessedFileStatus>()
            .HasData(
                Enum.GetValues(typeof(FileStatus))
                    .Cast<FileStatus>()
                    .Where(e => e != FileStatus.Processing)
                    .Select(e => new ProcessedFileStatus() { Id = e, Name = e.ToString() })
            );

        //
        builder.Entity<ProcessedFile>().Property(e => e.StatusId).HasConversion<int>();

        builder
            .Entity<ProcessedFile>()
            .HasOne<ProcessedFileStatus>()
            .WithMany()
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        base.OnModelCreating(builder);
    }
}
