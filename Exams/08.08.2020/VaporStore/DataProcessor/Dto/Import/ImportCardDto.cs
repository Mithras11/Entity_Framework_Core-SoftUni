using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class ImportCardDto
    {
        [Required]
        [RegularExpression(@"^([0-9]{4}\s){3}([0-9]{4})$")]
        public string Number { get; set; }

        [Required]
        [JsonProperty("CVC")]
        [MinLength(3), MaxLength(3)]
        [RegularExpression("^[0-9]{3}$")]
        public string Cvc { get; set; }

        [Required]
        public string Type { get; set; }

    }
}
