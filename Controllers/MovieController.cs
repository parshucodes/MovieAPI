using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Data;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;
using System.Net.Http.Headers;

namespace MovieAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;
        public MovieController(MovieDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movieCount = _context.Movie.Count();
                var movieList = _mapper.Map<List<MovieListViewModel>>(_context.Movie.Include(x => x.Actors).Skip(pageIndex * pageSize).Take(pageSize).ToList());
                response.Status = true;
                response.Message = "Success";
                response.Data = new { movieList, movieCount };

                return Ok(response);
            }
            catch (Exception ex)
            {

                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetMovieById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {

                var movie = _context.Movie.Include(x => x.Actors).Where(x => x.Id == id).FirstOrDefault();
                if (movie == null)
                {
                    response.Status = false;
                    response.Message = "Record doesn't exist";
                    return BadRequest(response);
                }
                var movieData = _mapper.Map<MovieDetailsViewModel>(movie);
                response.Status = true;
                response.Message = "Success";
                response.Data = movieData;

                return Ok(response);
            }
            catch (Exception ex)
            {

                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }
        [HttpPost]
        public IActionResult Post(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();
                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned";
                        return BadRequest(response);
                    }

                    var postedModel = _mapper.Map<Movie>(model);
                    postedModel.Actors = actors;
                    _context.Movie.Add(postedModel);
                    _context.SaveChanges();

                    var responData = _mapper.Map<MovieDetailsViewModel>(postedModel);
                    response.Status = true;
                    response.Message = "Success";
                    response.Data = responData;


                    return Ok(response);

                }
                else
                {
                    response.Status = false;
                    response.Message = "validation failed";
                    response.Data = ModelState;
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {

                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpPut]
        public IActionResult Put(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Id <= 0)
                    {
                        response.Status = false;
                        response.Message = "Invalid movie record";
                        return BadRequest(response);
                    }
                    var actors = _context.Person.Where(x => model.Actors.Contains(x.Id)).ToList();
                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned";
                        return BadRequest(response);
                    }
                    var movieDetails = _context.Movie.Include(x => x.Actors).Where(x => x.Id == model.Id).FirstOrDefault();
                    if (movieDetails == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned";
                        return BadRequest(response);
                    }
                    movieDetails.CoverImage = model.CoverImage;
                    movieDetails.Description = model.Description;
                    movieDetails.Title = model.Title;
                    movieDetails.Language = model.Language;
                    movieDetails.ReleaseDate = model.ReleaseDate;
                    //find removed actors
                    var removedActors = movieDetails.Actors.Where(x => !model.Actors.Contains(x.Id)).ToList();
                    foreach (var actor in removedActors)
                    {
                        movieDetails.Actors.Remove(actor);
                    }
                    var addedActor = actors.Except(movieDetails.Actors).ToList();
                    foreach (var actor in addedActor)
                    {
                        movieDetails.Actors.Add(actor);
                    }
                    _context.SaveChanges();

                    var responData = new MovieDetailsViewModel
                    {
                        Id = movieDetails.Id,
                        Title = movieDetails.Title,
                        Actors = movieDetails.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth
                        }).ToList(),
                        CoverImage = movieDetails.CoverImage,
                        Language = movieDetails.Language,
                        ReleaseDate = movieDetails.ReleaseDate,
                        Description = movieDetails.Description,
                    };

                    response.Status = true;
                    response.Message = "Success";
                    response.Data = responData;


                    return Ok(response);

                }
                else
                {
                    response.Status = false;
                    response.Message = "validation failed";
                    response.Data = ModelState;
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {

                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _context.Movie.Where(x=>x.Id == id).FirstOrDefault();
                if (movie == null)
                {
                    response.Status = false;
                    response.Message = "Movie Does Not Exist";
                    return BadRequest(response);
                }
                _context.Movie.Remove(movie);
                _context.SaveChanges();
                response.Status = true;
                response.Message = "Deleted Successfully";
                return Ok(response);
            }
            catch (Exception ex)
            {

                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("upload-movie-poster")]
        public async Task<IActionResult> uploadmovieposter(IFormFile imageFile)
        {
            try
            {
                var filename = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"');
                string newPath = @"C:\Users\parsh\Documents\ImagesforMovie";

                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
                string[] allowedImageExtensions = new string[] { ".jpeg", ".jpg", ".png" };
                if (!allowedImageExtensions.Contains(Path.GetExtension(filename)))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Status = false,
                        Message = "only .jpeg,.jpg or .png are allowed",

                    });
                }
                string newFileName = Guid.NewGuid() + Path.GetExtension(filename);
                string fullFilePath = Path.Combine(newPath, newFileName);
                using(var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                return Ok(new { ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles{newFileName}" } );
            }
            catch (Exception ex)
            {

                return BadRequest(new BaseResponseModel
                {
                    Status = false,
                    Message = "Error Occured",

                });
            }
        }
    }
}
