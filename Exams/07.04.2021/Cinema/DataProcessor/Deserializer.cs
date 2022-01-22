namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Cinema.Data.Models;
    using Cinema.Data.Models.Enums;
    using Cinema.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie
            = "Successfully imported {0} with genre {1} and rating {2:f2}!";
        private const string SuccessfulImportHallSeat
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var moviesDtos = JsonConvert.DeserializeObject<List<ImportMovieDto>>(jsonString);

            var movies = new List<Movie>();

            var sb = new StringBuilder();

            foreach (var movieDto in moviesDtos)
            {
                if (!IsValid(movieDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                //check genre and cast to enumType


                if (!IsValid(movieDto.Genre))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //check duration and convert

                TimeSpan duration;

                bool isDurationValid = TimeSpan.TryParseExact(
                                      movieDto.Duration, "c", CultureInfo.InvariantCulture, out duration);

                if (!isDurationValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //check if title exists

                if (movies.Any(m => m.Title == movieDto.Title))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //create new movie
                var newMovie = new Movie()
                {
                    Title = movieDto.Title,
                    Genre = movieDto.Genre,
                    Duration = duration,
                    Rating = movieDto.Rating,
                    Director = movieDto.Director
                };


                movies.Add(newMovie);
                sb.AppendLine(String.Format(SuccessfulImportMovie,
                    newMovie.Title, newMovie.Genre, newMovie.Rating));
            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().Trim();


        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var hallsDtos = JsonConvert.DeserializeObject<List<ImportHallDto>>(jsonString);

            var halls = new List<Hall>();

            var sb = new StringBuilder();

            foreach (var hallDto in hallsDtos)
            {
                if (!IsValid(hallDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //create new hall
                var newHall = new Hall()
                {
                    Name = hallDto.Name,
                    Is4Dx = hallDto.Is4Dx,
                    Is3D = hallDto.Is3D,
                };

                //create seats and add to hall
                var seats = new List<Seat>();

                for (int i = 1; i <= hallDto.SeatsCount; i++)
                {
                    seats.Add(new Seat()
                    {
                        // Id = i,
                        Hall = newHall
                    });
                }

                newHall.Seats = seats;

                //check projection type

                string projectionType = "";

                if (hallDto.Is4Dx && hallDto.Is3D)
                {
                    projectionType = "4Dx/3D";
                }
                else if (hallDto.Is4Dx)
                {
                    projectionType = "4Dx";
                }
                else if (hallDto.Is3D)
                {
                    projectionType = "3D";
                }
                else
                {
                    projectionType = "Normal";
                }


                //add to halls
                halls.Add(newHall);
                sb.AppendLine(String.Format(SuccessfulImportHallSeat,
                    newHall.Name, projectionType, newHall.Seats.Count));

            }

            context.Halls.AddRange(halls);
            context.SaveChanges();

            return sb.ToString().Trim();

        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            var XmlSerializer = new XmlSerializer(typeof(List<ImportProjectionDto>), new XmlRootAttribute("Projections"));

            using (var reader = new StringReader(xmlString))
            {
                var projectionsDtos = (List<ImportProjectionDto>)XmlSerializer.Deserialize(reader);

                var projections = new List<Projection>();
                var sb = new StringBuilder();

                foreach (var projDto in projectionsDtos)
                {
                    var movie = context.Movies.FirstOrDefault(m => m.Id == projDto.MovieId);

                    if (movie == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var hall = context.Halls.FirstOrDefault(h => h.Id == projDto.HallId);

                    if (hall == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }


                    var dateTime = DateTime.ParseExact(projDto.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    var newProj = new Projection()
                    {
                        MovieId = projDto.MovieId,
                        Movie = movie,
                        HallId = projDto.HallId,
                        Hall = hall,
                        DateTime = dateTime
                    };

                    projections.Add(newProj);

                    sb.AppendLine(String.Format(SuccessfulImportProjection,
                       newProj.Movie.Title, newProj.DateTime.ToString("MM/dd/yyyy")));

                }

                context.Projections.AddRange(projections);
                context.SaveChanges();

                return sb.ToString().Trim();
            }
        }


        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(List<ImportCustomerDto>), new XmlRootAttribute("Customers"));

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            var reader = new StringReader(xmlString);

            using (reader)
            {
                var customerDtos = (List<ImportCustomerDto>)serializer.Deserialize(reader);

                var customersToAdd = new List<Customer>();

                foreach (var customerDto in customerDtos)
                {
                    if (!IsValid(customerDto))
                    {
                        sb.AppendLine(ErrorMessage);

                        continue;
                    }

                    var customer = new Customer()
                    {
                        FirstName = customerDto.FirstName,
                        LastName = customerDto.LastName,
                        Age = customerDto.Age,
                        Balance = customerDto.Balance,

                    };

                    //you need validate tickets and projections explicitly, otherwise an error is thrown

                    foreach (var ticketDto in customerDto.Tickets)
                    {
                        if (!IsValid(ticketDto))
                        {
                            sb.AppendLine(ErrorMessage);
                            break;
                        }

                        var projection = context.Projections.FirstOrDefault(p => p.Id == ticketDto.ProjectionId);

                        if (projection == null)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        var ticket = new Ticket()
                        {
                            CustomerId = customer.Id,
                            Customer = customer,
                            Price = ticketDto.Price,
                            ProjectionId = ticketDto.ProjectionId,
                            Projection = projection,
                        };

                        customer.Tickets.Add(ticket);

                    }

                    customersToAdd.Add(customer);

                    sb.AppendLine(string.Format(SuccessfulImportCustomerTicket, customer.FirstName,
                        customer.LastName, customer.Tickets.Count));

                }

                context.Customers.AddRange(customersToAdd);

                context.SaveChanges();

                return sb.ToString().Trim();
            }
        }



        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}
