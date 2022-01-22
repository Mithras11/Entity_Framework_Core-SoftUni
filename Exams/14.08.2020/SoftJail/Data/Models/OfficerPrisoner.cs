using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.Data.Models
{
    public class OfficerPrisoner
    {
        [Required]
        public int PrisonerId { get; set; }

        [Required]
        public virtual Prisoner Prisoner { get; set; }

        //[Required]
        public int? OfficerId { get; set; } //why do you insist on making it Required when it shouldn't be?!

        [Required]
        public virtual Officer Officer { get; set; }

    }
}
