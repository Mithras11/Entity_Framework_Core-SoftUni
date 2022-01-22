namespace Theatre.DataProcessor
{
    using Newtonsoft.Json;
    using System;
    using System.Globalization;
    using System.Linq;
    using Theatre.Data;
    using Theatre.DataProcessor.ExportDto;

    public class Serializer
    {
        public static string ExportTheatres(TheatreContext context, int numbersOfHalls)
        {
            var theatres = context.Theatres
                .Where(t => t.NumberOfHalls >= numbersOfHalls &&
                            t.Tickets.Count >= 20)
                .ToList()
                .Select(t => new ExportTheatreDto
                {
                    Name = t.Name,
                    Halls = t.NumberOfHalls,
                    TotalIncome = t.Tickets
                                .Where(x => x.RowNumber >= 1 && x.RowNumber <= 5)
                                .ToList()
                                .Sum(x => x.Price),

                    Tickets = t.Tickets
                        .Where(x => x.RowNumber >= 1 && x.RowNumber <= 5)
                        .ToList()
                    .Select(x => new ExportTicketDto()
                    {

                        Price = x.Price,
                        RowNumber = x.RowNumber

                    })
                    .OrderByDescending(x => x.Price)
                    .ToList()
                })
                .OrderByDescending(t => t.Halls)
                .ThenBy(t => t.Name)
                .ToList();

            var result = JsonConvert.SerializeObject(theatres, Formatting.Indented);
            return result;


        }

        public static string ExportPlays(TheatreContext context, double rating)
        {
            var plays = context.Plays
                 .Where(x => x.Rating <= rating)
                 .ToList()
                 .Select(x => new ExportPlayDto
                 {
                     Title = x.Title,
                     Duration = x.Duration.ToString("c", CultureInfo.InvariantCulture),
                     Rating = x.Rating == 0 ? "Premier" : x.Rating.ToString(),
                     Genre = x.Genre.ToString(),

                     Actors = x.Casts
                        .Where(y => y.IsMainCharacter)
                         .Select(y => new ExportActorDto()
                         {

                             FullName = y.FullName,
                             MainCharacter = $"Plays main character in '{x.Title}'."
                         })
                         .OrderByDescending(y => y.FullName)
                         .ToList()
                 })
                 .OrderBy(x => x.Title)
                 .ThenByDescending(x => x.Genre)
                 .ToList();


            var xml = XmlConverter.Serialize(plays, "Plays");
            return xml;

        }
    }
}
