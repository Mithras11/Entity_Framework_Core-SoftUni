namespace MusicHub.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public class SongPerformer
    {
        [ForeignKey(nameof(Song))]
        [Required]
        public int SongId { get; set; }

        public virtual Song Song { get; set; }

        [ForeignKey(nameof(PerformerId))]
        [Required]
        public int PerformerId { get; set; }

        public virtual Performer Performer { get; set; }
    }
}
