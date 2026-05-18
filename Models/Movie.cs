namespace Proje2.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string? PhotoPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
