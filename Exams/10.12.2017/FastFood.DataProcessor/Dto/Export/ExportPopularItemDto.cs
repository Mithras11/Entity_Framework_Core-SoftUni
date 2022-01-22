using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FastFood.DataProcessor.Dto.Export
{
    [XmlType("MostPopularItem")]
    public class ExportPopularItemDto
    {
        public string Name { get; set; }

        public decimal TotalMade { get; set; }
        public int TimesSold { get; set; }
    }
}
