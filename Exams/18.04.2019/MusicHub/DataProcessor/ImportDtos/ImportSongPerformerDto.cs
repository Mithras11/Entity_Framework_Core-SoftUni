using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace MusicHub.DataProcessor.ImportDtos
{
    [XmlType("Song")]
    public class ImportSongPerformerDto
    {
        [Required]
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}
