using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Theatre.Data.Models.Enums;

namespace Theatre.Data.Models
{
    public class Play
    {

        public Play()
        {
            this.Tickets = new HashSet<Ticket>();
            this.Casts = new HashSet<Cast>();
        }

        public int Id { get; set; }


        [Required]
        [MinLength(3), MaxLength(50)]
        public string Title { get; set; }

        [Required]
        //min length?
        public TimeSpan Duration { get; set; }


        [Required]
        [Range(0.00, 10.00)]
        public float Rating { get; set; }

        [Required]
        [Range(0, 3)]
        public Genre Genre { get; set; }

        [Required]
        [MaxLength(700)]
        public string Description { get; set; }

        [Required]
        [MinLength(4), MaxLength(30)]
        public string Screenwriter { get; set; }


        public virtual ICollection<Cast> Casts { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }

    }
}
