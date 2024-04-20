namespace GsmCore.DTO;

public class ServerFile
{
    public string Name { get; set; } = null!;
    public long Size { get; set; }
    public bool IsFile { get; set; }
    public bool IsSymlink { get; set; }
    public bool IsEditable { get; set; }
    public string MimeType { get; set; } = null!;
    public DateTime ModifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}