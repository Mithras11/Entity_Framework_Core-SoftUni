namespace Theatre.DataProcessor
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Theatre.Data;
    using Theatre.Data.Models;
    using Theatre.Data.Models.Enums;
    using Theatre.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportPlay
            = "Successfully imported {0} with genre {1} and a rating of {2}!";

        private const string SuccessfulImportActor
            = "Successfully imported actor {0} as a {1} character!";

        private const string SuccessfulImportTheatre
            = "Successfully imported theatre {0} with #{1} tickets!";

        public static string ImportPlays(TheatreContext context, string xmlString)
        {
            var plays = new List<Play>();
            var sb = new StringBuilder();

            var playsDtos = XmlConverter.Deserializer<ImportPlayDto>(xmlString, "Plays");

            foreach (var playDto in playsDtos)
            {
                bool isValidGenre = Enum.TryParse(typeof(Genre), playDto.Genre, out var genre);

                bool isValidDuration = TimeSpan.TryParseExact(
                                        playDto.Duration, "c", CultureInfo.InvariantCulture, out var duration);

                if (TimeSpan.Parse(playDto.Duration).Hours < 1)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!IsValid(playDto) ||
                    !isValidGenre ||
                    !isValidDuration)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                //create new play

                var play = new Play()
                {
                    Title = playDto.Title,
                    Duration = duration,
                    Rating = playDto.Rating,
                    Genre = (Genre)genre,
                    Description = playDto.Description,
                    Screenwriter = playDto.Screenwriter
                };



                plays.Add(play);

                sb.AppendLine(String.Format(SuccessfulImportPlay,
                    play.Title, play.Genre, play.Rating));

            }

            context.Plays.AddRange(plays);
            context.SaveChanges();
            return sb.ToString().Trim();

        }

        public static string ImportCasts(TheatreContext context, string xmlString)
        {
            var casts = new List<Cast>();
            var sb = new StringBuilder();

            var castsDtos = XmlConverter.Deserializer<ImportCastDto>(xmlString, "Casts");

            foreach (var castDto in castsDtos)
            {

                if (!IsValid(castDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                //create new cast

                var cast = new Cast()
                {
                    FullName = castDto.FullName,
                    IsMainCharacter = castDto.IsMainCharacter,
                    PhoneNumber = castDto.PhoneNumber,
                    PlayId = castDto.PlayId

                };


                casts.Add(cast);

                string role = cast.IsMainCharacter ? "main" : "lesser";

                sb.AppendLine(String.Format(SuccessfulImportActor,
                    cast.FullName, role));

            }

            context.Casts.AddRange(casts);
            context.SaveChanges();
            return sb.ToString().Trim();
        }

        public static string ImportTtheatersTickets(TheatreContext context, string jsonString)
        {
            var theatresDtos = JsonConvert.DeserializeObject<List<ImportTheatreDto>>(jsonString);
            var theatres = new List<Theatre>();
            var sb = new StringBuilder();

            foreach (var theatreDto in theatresDtos)
            {

                if (!IsValid(theatreDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                //create theatre

                var theatre = new Theatre()
                {
                    Name = theatreDto.Name,
                    Director = theatreDto.Director,
                    NumberOfHalls = theatreDto.NumberOfHalls

                };

                //check tickets

                //bool IsIdValid = true;
                foreach (var ticketDto in theatreDto.Tickets)
                {

                    if (!IsValid(ticketDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }


                    //check playId

                    //var playIdToImport = context.Plays.Find(ticketDto.PlayId);

                    //if (playIdToImport == null)
                    //{
                    //    sb.AppendLine(ErrorMessage);
                    //    //IsIdValid = false;//??
                    //    // break;
                    //    continue;
                    //}



                    theatre.Tickets.Add(new Ticket()
                    {
                        Price = ticketDto.Price,
                        RowNumber = ticketDto.RowNumber,
                        PlayId = ticketDto.PlayId,
                        //Play = playIdToImport,
                        TheatreId = theatre.Id,
                        Theatre = theatre

                    });
                }

                //if (IsIdValid)
                //{
                theatres.Add(theatre);
                sb.AppendLine(String.Format(SuccessfulImportTheatre,
                    theatre.Name, theatre.Tickets.Count.ToString()));
                // }


            }

            context.Theatres.AddRange(theatres);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }


        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
