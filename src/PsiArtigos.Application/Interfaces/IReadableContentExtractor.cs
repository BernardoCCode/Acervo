using PsiArtigos.Domain.Enums;

namespace PsiArtigos.Application.Interfaces;

public interface IReadableContentExtractor
{
    Task<ExtractedReadableContent?> FromPdfAsync(
        byte[] pdfBytes,
        CancellationToken cancellationToken = default);

    Task<ExtractedReadableContent?> FromHtmlUrlAsync(
        string pageUrl,
        CancellationToken cancellationToken = default);
}

public sealed record ExtractedReadableContent(
    string Body,
    ReadableContentSource Source,
    int? PageCount);
