using Microsoft.EntityFrameworkCore;

namespace GsmCore.Model;

[PrimaryKey(nameof(Key))]
public class Setting
{
    public string Key { get; set; }
    public string Value { get; set; }
}