using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.Dto.Import
{
    [XmlType("partId")]
    public class PartCarDto
    {
        [XmlAttribute("id")]
        public int PartId { get; set; }
    }
}
