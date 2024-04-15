using System.ComponentModel.DataAnnotations.Schema;

namespace GsmCore.Model;

public class BackupRepository : BaseModel
{
    public BackupRepositoryType Type { get; set; }
    public string Name { get; set; }

    [Column(TypeName = "TEXT")] public Dictionary<string, object> Data { get; set; }
}