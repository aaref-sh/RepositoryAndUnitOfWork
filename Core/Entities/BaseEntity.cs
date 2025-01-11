using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

public class BaseEntity : IBaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.UtcNow;
}
