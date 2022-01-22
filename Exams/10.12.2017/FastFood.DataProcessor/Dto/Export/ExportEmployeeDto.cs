using System;
using System.Collections.Generic;
using System.Text;

namespace FastFood.DataProcessor.Dto.Export
{
    public class ExportEmployeeDto
    {
        public string Name { get; set; }

        public List<ExportOrderDto> Orders { get; set; }

        public decimal TotalMade { get; set; }
    }
}
