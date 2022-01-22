namespace VaporStore.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
    {
        //Messages
        private const string ErrorMessage = "Invalid Data";

        private const string SuccessfullyImportedGame
            = "Added {0} ({1}) with {2} tags";

        private const string SuccessfullyImportedUser
            = "Imported {0} with {1} cards";

        private const string SuccessfullyImportedPurchase
            = "Imported {0} for {1}";


        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            var gamesDtos = JsonConvert.DeserializeObject<List<ImportGameDto>>(jsonString);

            var games = new List<Game>();
            var developers = new List<Developer>();
            var genres = new List<Genre>();
            var tags = new List<Tag>();


            var sb = new StringBuilder();

            foreach (var gameDto in gamesDtos)
            {
                //validate gameDto

                if (!IsValid(gameDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //validate ReleaseDate

                DateTime releaseDate;

                bool isValidReleaseDate = DateTime.TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);

                if (!isValidReleaseDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //check if tags are missing

                if (gameDto.Tags.Length == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //create new game

                var gameToImport = new Game
                {
                    Name = gameDto.Name,
                    Price = gameDto.Price,
                    ReleaseDate = releaseDate //!!!
                };

                //check if developer exists 

                var currDev = developers.FirstOrDefault(d => d.Name == gameDto.Developer);

                if (currDev == null)
                {
                    currDev = new Developer()
                    {
                        Name = gameDto.Developer
                    };

                    developers.Add(currDev);
                }

                //add to game
                gameToImport.Developer = currDev;

                //check if genre exists

                var currGenre = genres.FirstOrDefault(g => g.Name == gameDto.Genre);

                if (currGenre == null)
                {
                    currGenre = new Genre()
                    {
                        Name = gameDto.Genre
                    };

                    genres.Add(currGenre);
                }

                //add to game
                gameToImport.Genre = currGenre;


                //check tags

                foreach (var tagName in gameDto.Tags)
                {

                    if (string.IsNullOrEmpty(tagName))
                    {
                        continue;
                    }

                    var currTag = tags.FirstOrDefault(t => t.Name == tagName);

                    if (currTag == null)
                    {
                        currTag = new Tag()
                        {
                            Name = tagName,
                        };

                        tags.Add(currTag);
                    }

                    //add to game
                    gameToImport.GameTags.Add(new GameTag()
                    {
                        Tag = currTag,
                        Game = gameToImport
                    });
                }

                if (gameToImport.GameTags.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                games.Add(gameToImport);
                sb.AppendLine(String.Format(SuccessfullyImportedGame, gameToImport.Name, gameToImport.Genre.Name, gameToImport.GameTags.Count));
            }

            context.Games.AddRange(games);
            context.SaveChanges();

            return sb.ToString().Trim();



        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            var usersDtos = JsonConvert.DeserializeObject<List<ImportUserDto>>(jsonString);

            var users = new List<User>();

            var sb = new StringBuilder();

            foreach (var userDto in usersDtos)
            {
                //check userDto
                if (!IsValid(userDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //check cards first!!



                var cardsToImport = new List<Card>();

                var cardsAreValid = true;

                foreach (var cardDto in userDto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
                        cardsAreValid = false;
                        break;
                    }


                    //check card type

                    object cardType;

                    bool isValidCard = Enum.TryParse(typeof(CardType), cardDto.Type, out cardType);

                    if (!isValidCard)
                    {
                        cardsAreValid = false;
                        break;
                    }


                    //create card and add to card list

                    var currCard = new Card()
                    {
                        Number = cardDto.Number,
                        Cvc = cardDto.Cvc,
                        Type = (CardType)cardType //!!!! otherwise it's only an object, you need to unbox it
                    };

                    cardsToImport.Add(currCard);
                }

                if (!cardsAreValid || cardsToImport.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //create User

                var userToimport = new User()
                {
                    FullName = userDto.FullName,
                    Username = userDto.Username,
                    Email = userDto.Email,
                    Cards = cardsToImport,
                };

                users.Add(userToimport);
                sb.AppendLine(String.Format(SuccessfullyImportedUser, userToimport.Username, userToimport.Cards.Count));

            }


            context.Users.AddRange(users);
            context.SaveChanges();

            return sb.ToString().Trim();


        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            var XmlSerializer = new XmlSerializer(typeof(List<ImportPurchaseDto>), new XmlRootAttribute("Purchases"));

            using (var reader = new StringReader(xmlString))
            {
                var purchasesDtos = (List<ImportPurchaseDto>)XmlSerializer.Deserialize(reader);

                var purchases = new List<Purchase>();
                var sb = new StringBuilder();

                foreach (var prcDto in purchasesDtos)
                {
                    //validate project

                    if (!IsValid(prcDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //validate type

                    object prcType;

                    bool isValidPurchase = Enum.TryParse(typeof(PurchaseType), prcDto.Type, out prcType);

                    if (!isValidPurchase)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //validate date

                    DateTime prcDate;
                    bool isValidPrcDate = DateTime.TryParseExact(prcDto.Date, "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out prcDate);

                    if (!isValidPrcDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //find card if exists

                    var card = context.Cards.FirstOrDefault(c => c.Number == prcDto.Card);

                    if (card == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //find game

                    var game = context.Games.FirstOrDefault(g => g.Name == prcDto.Game);

                    if (game == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }



                    //create new purchase

                    var purchaseToImport = new Purchase()
                    {
                        Game = game, //????
                        Type = (PurchaseType)prcType,
                        ProductKey = prcDto.ProductKey,
                        Date = prcDate,
                        Card = card
                    };

                    purchases.Add(purchaseToImport);
                    sb.AppendLine(String.Format(SuccessfullyImportedPurchase,
                        purchaseToImport.Game.Name, purchaseToImport.Card.User.Username));
                }

                context.Purchases.AddRange(purchases);
                context.SaveChanges();

                return sb.ToString().Trim();

            }

        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}