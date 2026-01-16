namespace SPBackend.Requests.Queries.GetAllPlugs;

public class GetAllPlugsResponse
{
    public List<PlugDto> Plugs { get; set; }
}

public class PlugDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Room {get; set;}
}

