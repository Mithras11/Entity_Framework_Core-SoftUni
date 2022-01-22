namespace Cinema.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Cinema.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var movies = context.Movies
                .Where(m => m.Rating >= rating &&
                        m.Projections.Any(p => p.Tickets.Count > 0))
                .OrderByDescending(m => m.Rating)
                .ThenByDescending(m => m.Projections
                                    .SelectMany(p => p.Tickets
                                    .Select(t => t.Price))
                                    .Sum())
                //.Take(10)
                .ToList()
                .Select(m => new ExportMovieDto()
                {
                    MovieName = m.Title,
                    Rating = m.Rating.ToString("f2"),
                    TotalIncomes = m.Projections
                                    .SelectMany(p => p.Tickets
                                    .Select(t => t.Price))
                                    .Sum()
                                    .ToString("f2"),

                    Customers = m.Projections
                                    .SelectMany(p => p.Tickets
                                    .Select(t => new ExportCustomerDto()
                                    {
                                        FirstName = t.Customer.FirstName,
                                        LastName = t.Customer.LastName,
                                        Balance = t.Customer.Balance.ToString("f2")

                                    }).ToList())
                    .OrderByDescending(c => c.Balance)
                    .ThenBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                    .ToList()
                })
                .Take(10)
                .ToList();





            var result = JsonConvert.SerializeObject(movies, Formatting.Indented);

            return result;
        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            var customers = context.Customers
                .Where(c => c.Age >= age)
                .OrderByDescending(c => c.Tickets.Sum(t => t.Price))
                .ToList()
                .Select(c => new ExportXmlCustomerDto()
                {
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    SpentMoney = c.Tickets.Sum(t => t.Price).ToString("f2"),
                    SpentTime = TimeSpan.FromMilliseconds(c.Tickets
                        .Select(t => t.Projection.Movie.Duration.TotalMilliseconds)
                        .Sum())
                        .ToString(@"hh\:mm\:ss")
                })
                .Take(10)
                .ToList();



            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<ExportXmlCustomerDto>), new XmlRootAttribute("Customers"));
            XmlSerializer.Serialize(new StringWriter(sb), customers, namespaces);

            return sb.ToString().Trim();
        }
    }
}