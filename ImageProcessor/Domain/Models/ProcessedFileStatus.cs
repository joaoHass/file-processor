namespace ImageProcessor.Domain.Models;

public enum ProcessedFileStatus
{
    Success,
    Processing,
    Failed,
    FailedUnsupportedFormat,
    FailedUnknownFormat,
}