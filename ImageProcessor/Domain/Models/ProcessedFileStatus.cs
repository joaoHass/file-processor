namespace ImageProcessor.Domain.Models;

public enum ProcessedFileStatus
{
    Success,
    Failed,
    FailedUnsupportedFormat,
    FailedUnknownFormat,
}