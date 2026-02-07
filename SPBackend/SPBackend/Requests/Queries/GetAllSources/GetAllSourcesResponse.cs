namespace SPBackend.Requests.Queries.GetAllSources;

public class GetAllSourcesResponse
{
    public List<SourceDto> Sources { get; set; }   
}

public class SourceDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long MaxCapacity { get; set; }
    public long HouseholdId { get; set; }
}