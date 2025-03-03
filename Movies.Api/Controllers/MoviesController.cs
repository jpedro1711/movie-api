using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers
{
    [ApiController]
    public class MoviesController(IMovieService movieService) : ControllerBase
    {
        private readonly IMovieService _movieService = movieService;

        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
        {
            var movie = request.MapToMovie();
            await _movieService.CreateAsync(movie);
            return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id },movie);
        }

        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<IActionResult> Get([FromRoute] string idOrSlug, [FromQuery] GetAllMoviesRequest request, CancellationToken cancellationToken)
        {
            var movie = Guid.TryParse(idOrSlug, out var id)
                ? await _movieService.GetByIdAsync(id)
                : await _movieService.GetBySlugAsync(idOrSlug);

            var options = request.MapToOptions();

            if (movie is null)
                return NotFound();

            return Ok(movie.MapToResponse());
        }

        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var movies = await _movieService.GetAllAsync();

            var moviesResponse = movies.MapToResponse();

            return Ok(moviesResponse);
        }

        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request, CancellationToken cancellationToken)
        {
            var movie = request.MapToMovie(id);

            var updated = await _movieService.UpdateAsync(movie);

            if (updated is null)
                return NotFound();

            var response = movie.MapToResponse();

            return Ok(response);
        }

        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _movieService.DeleteByIdAsync(id);

            if (!deleted)
                return NotFound();

            return Ok();
        }
    }
}
