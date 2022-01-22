using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MusicHub.DataProcessor.ImportDtos
{
    public class ImportoProducerDto
    {
        [Required]
        [MinLength(3), MaxLength(30)]
        public string Name { get; set; }

       
        [RegularExpression(@"^([A-Z][a-z]+)\s([A-Z][a-z]+)$")]
        public string Pseudonym { get; set; }

        
        [RegularExpression(@"^\+(([0-9]{3}\s){3})([0-9]{3})$")]
        public string PhoneNumber { get; set; }

        public virtual List<ImportAlbumDto> Albums { get; set; }
    }
}
