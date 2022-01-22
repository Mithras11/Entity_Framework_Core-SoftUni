using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    [XmlRoot("Users")]
    public class UserRootDTO
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlElement("users")]
        public List<UserExportDTO> Users { get; set; }
    }
}
