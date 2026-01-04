namespace Shared.Models;

public class ApiProblemDetails
{
    public int Status { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
}
