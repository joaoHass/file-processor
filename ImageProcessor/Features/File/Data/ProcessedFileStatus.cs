using ImageProcessor.Features.File.Domain.Enums;

namespace ImageProcessor.Features.File.Data;

public class ProcessedFileStatus
{
    public required FileStatus Id { get; set; }
    public required string Name { get; set; }
}
