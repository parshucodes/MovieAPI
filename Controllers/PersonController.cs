using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAPIDemo.Data;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;

namespace MovieAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;
        public PersonController(MovieDbContext context, IMapper mapper)
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
                var actorCount = _context.Person.Count();
                var actorList = _mapper.Map<List<ActorViewModel>>(_context.Person.Skip(pageIndex * pageSize).Take(pageSize).ToList());
                response.Status = true;
                response.Message = "Success";
                response.Data = new { actorList, actorCount };

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
        public IActionResult GetPersonById(int id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {

                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();
                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Record doesn't exist";
                    return BadRequest(response);
                }
                var personData = new ActorDetailViewModel
                { 
                    Id = person.Id,
                    Name=person.Name,
                    DateOfBirth = person.DateOfBirth,
                    Movies = _context.Movie.Where(x=>x.Actors.Contains(person)).Select(x=>x.Title).ToArray(),
                };
                response.Status = true;
                response.Message = "Success";
                response.Data = personData;

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
        public IActionResult Post(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    

                    var postedModel = new Person
                    {
                        Name = model.Name,
                        DateOfBirth = model.DateOfBirth,
                    };
                    _context.Person.Add(postedModel);
                    _context.SaveChanges();

                    //once data is posted we'll load id in model and upload it in model to display in viw model
                    model.Id = postedModel.Id;

                    response.Status = true;
                    response.Message = "Success";
                    response.Data = model;


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
        public IActionResult Put(ActorViewModel model)
        {  
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                
                if (ModelState.IsValid)
                {
                    var postedModel = _mapper.Map<Person>(model);
                    if (model.Id<=0)
                    {
                        response.Status = false;
                        response.Message = "record doesn't exist";
                        return BadRequest(response);
                    }
                    var personDetail = _context.Person.Where(x=>x.Id==model.Id).AsNoTracking().FirstOrDefault();
                    if (personDetail==null)
                    {
                        response.Status = false;
                        response.Message = "Invalid";
                        return BadRequest(response);
                    }
                    _context.Person.Update(personDetail);
                    _context.SaveChanges();

                    response.Status = true;
                    response.Message = "Success";
                    response.Data = postedModel;


                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Invalid model";
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
                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();
                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Person Does Not Exist";
                    return BadRequest(response);
                }
                _context.Person.Remove(person);
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
        [HttpGet]
        [Route("Search/searchText")]
        public IActionResult Get(string searchText)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var searchedPerson = _context.Person.Where(x=>x.Name.Contains(searchText)).Select(x=> new
                {
                    x.Id, x.Name,
                }).ToList();
                response.Status = true;
                response.Message = "Success";
                response.Data = searchedPerson;


                return Ok(response);
            }
            catch (Exception ex)
            {

                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }
    }
}
