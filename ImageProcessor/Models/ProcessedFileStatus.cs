namespace ImageProcessor.Models;

public enum ProcessedFileStatus
{
    Success,
    Processing,
    Failed,
    FailedUnsupportedFormat,
    FailedUnknownFormat,
}