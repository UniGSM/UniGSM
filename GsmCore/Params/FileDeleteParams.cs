namespace GsmCore.Params;

public class FileDeleteParams
{
    public string Root { get; set; } = null!;
    public IEnumerable<string> Files { get; set; } = new List<string>();
}