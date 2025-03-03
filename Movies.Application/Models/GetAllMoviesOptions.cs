namespace Movies.Application.Models
{
    public class GetAllMoviesOptions
    {
        public int? Year { get; set; }
        public string? Title { get; set; }
        public Guid? UserId { get; set; }
    }
}
