using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MusicHub.Data.Models
{
    public class SongPerformer
    {
        [Required]
        [ForeignKey(nameof(Song))]
        public int SongId { get; set; }

        [Required] 
        public virtual Song Song { get; set; }

        [Required]
        [ForeignKey(nameof(Perfomer))]
        public int PerformerId { get; set; }

        [Required]
        public virtual Performer Perfomer { get; set; }
    }
}
