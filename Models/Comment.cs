namespace Proje2.Models;

public class Comment
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Movie Movie { get; set; } = null!;
}
