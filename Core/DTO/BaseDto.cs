
namespace Core.DTO;

public class BaseDetailsDto { public long Id { get; set; } }
public class BaseCreateDto;
public class BaseUpdateDto { public long Id { get; set; } }
public class BaseListDto {
    public long Id { get; set; } 

    public override string? ToString()
    {
        var name = GetType().GetProperties().FirstOrDefault(x => x.Name == "Name");
        if(name == null) return base.ToString();
        return name.GetValue(this)?.ToString();
    }
}


public class BaseLiteDto {
    public long Id { get; set; }
    public override string? ToString()
    {
        var name = GetType().GetProperties().FirstOrDefault(x => x.Name == "Name");
        if(name == null) return base.ToString();
        return name.GetValue(this)?.ToString();
    }
}