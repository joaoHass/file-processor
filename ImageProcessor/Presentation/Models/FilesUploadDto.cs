using ImageProcessor.Domain.Models;

namespace ImageProcessor.Presentation.Models;

public record FilesUploadDto(
    IFormFile[]? Files,
    FileType TargetFileType,
    bool Compress = false,
    bool Resize = false
);
