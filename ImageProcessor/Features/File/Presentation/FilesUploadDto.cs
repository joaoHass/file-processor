using ImageProcessor.Features.File.Domain.Enums;

namespace ImageProcessor.Features.File.Presentation;

public record FilesUploadDto(
    IFormFile[]? Files,
    FileType TargetFileType,
    bool Compress = false,
    bool Resize = false
);
