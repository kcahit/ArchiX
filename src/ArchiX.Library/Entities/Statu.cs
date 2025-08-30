
using ArchiX.Library.Entities;

public class Statu : BaseEntity
{
  
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
