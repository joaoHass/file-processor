using ImageProcessor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageProcessor.Data.Types;

// For now there will be no UserId property, as I haven't
// set the auth stuff
public class ProcessedFile : IEntity
{
    public int Id { get; private set; }
    public required string OriginalFileName { get; set;}
    public required string FilePath { get; set; }
    public required FileStatus StatusId { get; set; }
    public DateTimeOffset CreatedAt { get; private init; } = DateTimeOffset.UtcNow;
}