using Movies.Application.Models;
using System.Threading;

namespace Movies.Application.Repositories
{
    public interface IMovieRepository
    {
        Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
        Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default);
        Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsById(Guid id, CancellationToken cancellationToken = default);
    }
}
