namespace ImageProcessor.Domain.Models;

public enum FileStatus
{
    Processing,
    Success,
    Failed,
    FailedUnsupportedFormat,
    FailedUnknownFormat,
}