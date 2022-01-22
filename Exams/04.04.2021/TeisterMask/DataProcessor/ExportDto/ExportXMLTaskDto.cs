﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ExportDto
{
    [XmlType("Task")]
    public class ExportXMLTaskDto
    {
        [XmlElement("Name")]
        public string Name { get; set; }

         [XmlElement("Label")]
        public string Label { get; set; }

    }
}