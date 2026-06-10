namespace TimeSight.Models;

public class Workspace
{
    public Guid? Id { get; set; }
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}
