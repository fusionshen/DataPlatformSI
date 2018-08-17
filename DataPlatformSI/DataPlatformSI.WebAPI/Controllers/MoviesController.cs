using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.DataAccess;
using DataPlatformSI.DataAccess.Models;
using DataPlatformSI.DataAccess.Repositories;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.WebAPI.Controllers
{

    [Route("odata/[controller]")]
    [ApiController]
    public class MoviesController : Controller
    {
        private readonly ProductsRepository _db;

        public MoviesController(ProductsRepository repository)
        {
            _db = repository;
        }

        public IActionResult Get()
        {
            return Ok(_db.GetMovies());
        }

        [HttpPost]
        public IActionResult CheckOut(int key)
        {
            var movie = _db.GetMovies().FirstOrDefault(m => m.ID == key);
            if (movie == null)
            {
                return BadRequest(ModelState);
            }

            if (!TryCheckoutMovie(movie))
            {
                return BadRequest("The movie is already checked out.");
            }

            return Ok(movie);
        }

        [HttpPost]
        public IActionResult Return(int key)
        {
            var movie = _db.GetMovies().FirstOrDefault(m => m.ID == key);
            if (movie == null)
            {
                return BadRequest(ModelState);
            }

            movie.DueDate = null;

            return Ok(movie);
        }

        // Check out a list of movies.
        [HttpPost]
        public IActionResult CheckOutMany(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Client passes a list of movie IDs to check out.
            var movieIDs = new HashSet<int>(parameters["MovieIDs"] as IEnumerable<int>);

            // Try to check out each movie in the list.
            var results = new List<Movie>();
            foreach (Movie movie in _db.GetMovies().Where(m => movieIDs.Contains(m.ID)))
            {
                if (TryCheckoutMovie(movie))
                {
                    results.Add(movie);
                }
            }

            // Return a list of the movies that were checked out.
            return Ok(results);
        }

        [HttpPost]
        [ODataRoute("CreateMovie")]
        public IActionResult CreateMovie(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string title = parameters["Title"] as string;

            Movie movie = new Movie()
            {
                Title = title,
                ID = _db.GetMovies().Count() + 1,
            };

            _db.GetMovies().Append(movie);

            return Ok(movie);
        }

        protected Movie GetMovieByKey(int key)
        {
            return _db.GetMovies().FirstOrDefault(m => m.ID == key);
        }

        private bool TryCheckoutMovie(Movie movie)
        {
            if (movie.IsCheckedOut)
            {
                return false;
            }
            else
            {
                // To check out a movie, set the due date.
                movie.DueDate = DateTime.Now.AddDays(7);
                return true;
            }
        }
    }
}
