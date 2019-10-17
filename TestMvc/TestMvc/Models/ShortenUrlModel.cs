using System;

namespace TestMvc.Models
{
    public class ShortenUrlModel
    {
        public string Id { get; set; }
        public string ShortenUrl { get; set; }
        public string FullUrl { get; set; }
        public string Custom { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}