using ImageProcessor.Domain.Models;

namespace ImageProcessor.Data.Types;

public class ProcessedFileStatus
{
    public required FileStatus Id { get; set; }
    public required string Name { get; set; }
}
