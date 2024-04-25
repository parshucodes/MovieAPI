using AutoMapper;
using MovieAPIDemo.Entities;
using MovieAPIDemo.Models;

namespace MovieAPIDemo
{
    public class MappingProfiles:Profile
    {
        public MappingProfiles()
        {
            CreateMap<Movie, MovieListViewModel>();
            CreateMap<MovieListViewModel, Movie>();
            CreateMap<Movie, MovieDetailsViewModel>();
            CreateMap<CreateMovieViewModel, Movie>().ForMember(x=>x.Actors, y=>y.Ignore());

            CreateMap<Person, ActorViewModel>();
            CreateMap<Person, ActorDetailViewModel>();
            CreateMap<ActorViewModel, Person>();

        }
    }
}
