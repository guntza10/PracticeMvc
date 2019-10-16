using System;

namespace TestMvc.Models
{
    public class ShortenUrlModel
    {
        public string Id { get; set; }
        public string ShortUrl { get; set; }
        public string FullUrl { get; set; }
        public string CustomUrl { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}