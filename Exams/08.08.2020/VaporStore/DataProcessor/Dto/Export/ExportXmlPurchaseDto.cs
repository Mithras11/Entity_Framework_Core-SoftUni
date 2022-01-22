using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace VaporStore.DataProcessor.Dto.Export
{
    [XmlType("Purchase")]
    public class ExportXmlPurchaseDto
    {
        //[XmlElement("Card")]
        public string Card { get; set; }

        public string Cvc { get; set; }

        public string Date { get; set; }
                
        public ExportXmlGameDto Game { get; set; }
    }
}
