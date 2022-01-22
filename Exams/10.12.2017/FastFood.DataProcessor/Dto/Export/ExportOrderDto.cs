using System;
using System.Collections.Generic;
using System.Text;

namespace FastFood.DataProcessor.Dto.Export
{
    public class ExportOrderDto
    {
        public string Customer { get; set; }

        public List<ExportItemDto> Items { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
