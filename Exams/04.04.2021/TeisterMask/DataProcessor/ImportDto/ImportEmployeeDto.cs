using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TeisterMask.Data.Models;

namespace TeisterMask.DataProcessor.ImportDto
{
    public class ImportEmployeeDto
    {
        //[JsonProperty("Username")]
        [Required]
        [StringLength(40, MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-z0-9]{3,}$")]
        public string Username { get; set; }

        //[JsonProperty("Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        //[JsonProperty("Phone")]
        [Required]
        [RegularExpression(@"^[\d]{3}-[\d]{3}-[\d]{4}$")]
        public string Phone { get; set; }

        //[JsonProperty("Tasks")]
        public List<int> Tasks { get; set; }


    }
}
