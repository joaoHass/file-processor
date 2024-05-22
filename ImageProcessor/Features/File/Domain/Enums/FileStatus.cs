namespace ImageProcessor.Features.File.Domain.Enums;

public enum FileStatus
{
    Processing,
    Success,
    Failed,
    FailedUnsupportedFormat,
    FailedUnknownFormat,
}
