using Microsoft.EntityFrameworkCore;

namespace GsmCore.Models;

[PrimaryKey(nameof(Key))]
public class Setting
{
    public string Key { get; set; }
    public string Value { get; set; }
}