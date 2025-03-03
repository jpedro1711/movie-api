using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

        public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var result = await connection.ExecuteAsync(
                    new CommandDefinition(
                        @"INSERT INTO movies (id, slug, title, yearofrelease) 
                          VALUES (@Id, @Slug, @Title, @YearOfRelease)",
                        movie, transaction
                    )
                );

                if (result > 0)
                {
                    foreach (var genre in movie.Genres)
                    {
                        await connection.ExecuteAsync(
                            new CommandDefinition(
                                @"INSERT INTO genres (movieId, name) 
                                  VALUES (@MovieId, @Name)",
                                new { MovieId = movie.Id, Name = genre },
                                transaction
                            )
                        );
                    }
                }

                transaction.Commit();
                return result > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var result = await connection.ExecuteAsync(
                "DELETE FROM movies WHERE id = @Id",
                new { Id = id }
            );

            return result > 0;
        }

        public async Task<bool> ExistsById(Guid id, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM movies WHERE id = @Id",
                new { Id = id }
            );

            return exists;
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var movies = await connection.QueryAsync<Movie>(
                "SELECT id, slug, title, yearofrelease FROM movies"
            );

            return movies;
        }

        public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(
                "SELECT id, slug, title, yearofrelease FROM movies WHERE id = @Id",
                new { Id = id }
            );

            return movie;
        }

        public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(
                "SELECT id, slug, title, yearofrelease FROM movies WHERE slug = @Slug",
                new { Slug = slug }
            );

            return movie;
        }

        public async Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var result = await connection.ExecuteAsync(
                    new CommandDefinition(
                        @"UPDATE movies 
                          SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease 
                          WHERE id = @Id",
                        movie, transaction
                    )
                );

                await connection.ExecuteAsync(
                    "DELETE FROM genres WHERE movieId = @MovieId",
                    new { MovieId = movie.Id },
                    transaction
                );

                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            @"INSERT INTO genres (movieId, name) 
                              VALUES (@MovieId, @Name)",
                            new { MovieId = movie.Id, Name = genre },
                            transaction
                        )
                    );
                }

                transaction.Commit();
                return movie;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
