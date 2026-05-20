namespace PhanMemQuanLyCongNo.Domain.Entities;

public class Attachment
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RelatedId { get; set; }

    public string RelatedType { get; set; } = "";

    public string FileName { get; set; } = "";

    public string FilePath { get; set; } = "";

    public string ContentType { get; set; } = "";

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}