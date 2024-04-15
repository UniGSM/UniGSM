using Microsoft.EntityFrameworkCore;

namespace GsmCore.Model;

[PrimaryKey(nameof(Id))]
public abstract class BaseModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}