namespace Vargshala.Contracts.StudyMaterials;

public class StudyMaterialDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BatchName { get; set; } = string.Empty;
    public string FileType { get; set; } = "PDF"; // PDF, Image, Document
    public string FileSize { get; set; } = "2.4 MB";
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; } = DateTime.Today;
    public int DownloadCount { get; set; } = 0;
    public string Status { get; set; } = "Published"; // Published, Draft, Archived
    public string TagsDisplay => Tags.Count > 0 ? string.Join(", ", Tags) : "—";
}
