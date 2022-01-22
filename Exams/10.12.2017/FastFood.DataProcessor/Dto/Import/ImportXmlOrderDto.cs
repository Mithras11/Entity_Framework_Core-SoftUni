using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace FastFood.DataProcessor.Dto.Import
{
    [XmlType("Order")]
    public class ImportXmlOrderDto
    {
        [Required]
        public string Customer { get; set; }

        [Required]
        [MinLength(3), MaxLength(30)]
        public string Employee { get; set; }

        [Required]
        public string DateTime { get; set; }

        [Required]
        public string Type { get; set; }

        [XmlArray]
        public List<ImportXmlItemDto> Items { get; set; }
    }
}