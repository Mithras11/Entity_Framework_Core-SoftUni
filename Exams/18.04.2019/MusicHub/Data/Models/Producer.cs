using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MusicHub.Data.Models
{
    public class Producer
    {
        public Producer()
        {
            this.Albums = new HashSet<Album>();
        }

       [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(3), MaxLength(30)]
        public string Name { get; set; }


        [RegularExpression(@"^([A-Z][a-z]+)\s([A-Z][a-z]+)$")]
        public string Pseudonym { get; set; }


        [RegularExpression(@"^\+(([0-9]{3}\s){3})([0-9]{3})$")]
        public string PhoneNumber { get; set; }

        public virtual ICollection<Album> Albums { get; set; }
    }
}
