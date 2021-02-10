using CinemaApi.Data;
using CinemaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {

        private CinemaDbContext _dbContext;

        public MoviesController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        // api/movies/AllMovies? sort = desc & pageNumber = 1 & pageSize = 5
        [Authorize]
        [HttpGet("[Action]")]
        public IActionResult AllMovies(string sort, int? pageNumber, int? pageSize)
        {
            var CurrentPageNumber = pageNumber ?? 1;
            var currentPageSize = pageSize ?? 5;
            //var movies = _dbContext.Movies;
            //return Ok(movies);
            var movies = from movie in _dbContext.Movies
                         select new
                         {
                             Id = movie.Id,
                             Name = movie.Name,
                             Duration = movie.Duration,
                             Language = movie.Language,
                             Rating = movie.Rating,
                             Genre = movie.Genre,
                             ImageUrl = movie.ImageUrl

                         };

            switch (sort)
            {
                case "desc":
                    return Ok(movies.Skip((CurrentPageNumber - 1) * currentPageSize).Take(currentPageSize).OrderByDescending(m => m.Rating));
                case "asc":
                    return Ok(movies.Skip((CurrentPageNumber - 1) * currentPageSize).Take(currentPageSize).OrderBy(m => m.Rating));
                default:
                    return Ok(movies.Skip((CurrentPageNumber - 1) * currentPageSize).Take(currentPageSize));
            }

        }

        // api/movies/moviedetail/1
        [Authorize]
        [HttpGet("[Action]/{Id}")]
        public IActionResult MovieDetail(int Id)
        {
            var movie = _dbContext.Movies.Find(Id);
            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);
        }

        // api/movies/FindMovie?movieName=MissionImpossible
        [Authorize]
        [HttpGet("[Action]")]
        public IActionResult FindMovies(string movieName)
        {
            var movies = from movie in _dbContext.Movies
                         where movie.Name.StartsWith(movieName)
                         select new
                         {
                             Id = movie.Id,
                             Name = movie.Name,
                             ImageUrl = movie.ImageUrl
                         };
            return Ok(movies);
        }

        //POST api/movies
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromForm] Movie movie)
        {
            var guid = Guid.NewGuid();
            var filePath = Path.Combine("wwwroot", guid + ".jpg");
            if (movie.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                movie.Image.CopyTo(fileStream);
            }
            movie.ImageUrl = filePath.Remove(0, 7);
            _dbContext.Movies.Add(movie);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        // PUT api/<MoviesController>/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Movie movie)
        {
            var mov = _dbContext.Movies.Find(id);
            if (movie == null)
            {
                return NotFound("No record found against this id");
            }
            else
            {
                var guid = Guid.NewGuid();
                var filePath = Path.Combine("wwwroot", guid + ".jpg");
                if (movie.Image != null)
                {
                    var fileStream = new FileStream(filePath, FileMode.Create);
                    movie.Image.CopyTo(fileStream);
                    mov.ImageUrl = filePath.Remove(0, 7);
                }
                mov.Name = movie.Name;
                mov.Description = movie.Description;
                mov.Language = movie.Language;
                mov.Duration = movie.Duration;
                mov.PlayingDate = movie.PlayingDate;
                mov.PlayingTime = movie.PlayingTime;
                mov.Rating = movie.Rating;
                mov.Genre = movie.Genre;
                mov.TrailorUrl = movie.TrailorUrl;
                mov.TicketPrice = movie.TicketPrice;

                _dbContext.SaveChanges();
                return Ok("Record updated successfully");
            }

        }


        // DELETE api/<MoviesController>/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _dbContext.Movies.Find(id);
            if (movie == null)
            {
                return NotFound("Record not found against this id");
            }
            else
            {
                _dbContext.Movies.Remove(movie);
                _dbContext.SaveChanges();
                return Ok("Record deleted");
            }

        }

    }
}
