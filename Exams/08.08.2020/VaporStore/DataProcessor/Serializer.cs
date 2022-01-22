namespace VaporStore.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
    {
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var genres = context.Genres
               .Where(g => genreNames.Contains(g.Name))
               .ToList()
               .Select(g => new ExportGenreDto()
               {
                   Id = g.Id,
                   Genre = g.Name,

                   Games = g.Games.Where(gm => gm.Purchases.Count > 0)
                   .ToList()
                   .Select(gm => new ExportGameDto()
                   {
                       Id = gm.Id,
                       Title = gm.Name,
                       Developer = gm.Developer.Name,
                       Tags = string.Join(", ", gm.GameTags.Select(gt => gt.Tag.Name)),
                       Players = gm.Purchases.Count
                   })
                   .OrderByDescending(gm => gm.Players)
                   .ThenBy(gm => gm.Id)
                   .ToList(),

                   TotalPlayers = g.Games.Select(gm => gm.Purchases.Count).Sum()
               })
               .OrderByDescending(g => g.TotalPlayers)
               .ThenBy(g => g.Id)
               .ToList();


            var result = JsonConvert.SerializeObject(genres, Formatting.Indented);

            return result;


        }

        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
        {
            var purchaseType = Enum.Parse<PurchaseType>(storeType); //!!!

            var users = context.Users
                .Where(u => u.Cards.Any(c => c.Purchases.Count > 0))
                .ToList()
                .Select(u => new ExportXmlUserDto()
                {
                    Username = u.Username,
                    Purchases = context.Purchases
                            .Where(p => p.Card.User.Username == u.Username &&
                            p.Type == purchaseType)
                            .OrderBy(p => p.Date)
                    .ToList()
                    .Select(p => new ExportXmlPurchaseDto()
                    {
                        Card = p.Card.Number,
                        Cvc = p.Card.Cvc,
                        Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                        Game = new ExportXmlGameDto()
                        {
                            Title = p.Game.Name,
                            Genre = p.Game.Genre.Name.ToString(),
                            Price = p.Game.Price
                        }

                    }).ToList(),

                    TotalSpent = context.Purchases
                                .Where(p => p.Card.User.Username == u.Username &&
                                p.Type == purchaseType)
                    .ToList()   //without that price comes out as 0.00 insetad of 0 !?
                    .Sum(p => p.Game.Price)
                })
                .Where(u => u.Purchases.Count > 0)
                .OrderByDescending(u => u.TotalSpent)
                .ThenBy(u => u.Username)
                .ToList();



            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<ExportXmlUserDto>), new XmlRootAttribute("Users"));
            XmlSerializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().Trim();
        }
    }
}