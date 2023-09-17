namespace HorizontalPrivilegeEscalation.ExampleApi.Persistence;

public class HpeEmail 
{
    public int EmailId { get; set; }
    public int UserId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}