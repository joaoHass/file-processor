namespace ImageProcessor.Presentation.Models;

public record FilesUploadDto(IFormFile[] Files, string TargetFileType, bool ShouldCompress = false, bool ShouldResize = false);