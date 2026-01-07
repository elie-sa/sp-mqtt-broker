namespace SPBackend.Models;

public class PlugPolicy
{
    public long Id { get; set; }
    public bool SetStatus { get; set; }
    public long PlugId { get; set; }
    public Plug Plug { get; set; }
    public long PolicyId { get; set; }
    public Policy Policy { get; set; }
}