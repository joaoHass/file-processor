namespace ImageProcessor.Domain.Models;

public enum FileStatus
{
    Success,
    Failed,
    FailedUnsupportedFormat,
    FailedUnknownFormat,
}