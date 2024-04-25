﻿using MovieAPIDemo.Entities;

namespace MovieAPIDemo.Models
{
    public class CreateMovieViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        //List of Actors
        public List<int> Actors { get; set; }
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImage { get; set; }
    }
}
