using System;
using System.Collections.Generic;

namespace TestMvc.Models
{
    public class IndexViewModel
    {
        public ShortenUrlModel shortUrlModel { get; set; }
        public List<ShortenUrlModel> history { get; set; }
        public string alertMessage { get; set; }
        public bool colorAlert { get; set; }
    }
}