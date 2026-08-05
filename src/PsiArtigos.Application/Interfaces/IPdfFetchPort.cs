namespace PsiArtigos.Application.Interfaces;

public interface IPdfFetchPort
{
    Task<PdfDocumentContent?> FetchAsync(string pdfUrl, CancellationToken cancellationToken = default);
}

public sealed record PdfDocumentContent(
    byte[] Bytes,
    string ContentType,
    string? FileName);
