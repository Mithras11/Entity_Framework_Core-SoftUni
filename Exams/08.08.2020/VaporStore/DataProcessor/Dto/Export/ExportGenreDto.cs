using System;
using System.Collections.Generic;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Export
{
    public class ExportGenreDto
    {
        public int Id { get; set; }
        public string Genre { get; set; }

        public List<ExportGameDto> Games { get; set; }

        public int TotalPlayers { get; set; }
    }
}
