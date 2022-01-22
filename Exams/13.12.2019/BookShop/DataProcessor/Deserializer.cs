namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var XmlSerializer = new XmlSerializer(typeof(List<ImportBookDto>), new XmlRootAttribute("Books"));

            using (var reader = new StringReader(xmlString))
            {
                var booksDtos = (List<ImportBookDto>)XmlSerializer.Deserialize(reader);

                var books = new List<Book>();
                var sb = new StringBuilder();

                foreach (var bookDto in booksDtos)
                {
                    if (!IsValid(bookDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //check date


                    DateTime publishedOnDate;

                    bool isValidPubDate =  DateTime.TryParseExact(bookDto.PublishedOn, "MM/dd/yyyy",
                                 CultureInfo.InvariantCulture, DateTimeStyles.None, out publishedOnDate);

                    if (!isValidPubDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }


                    //create book

                    var currBook = new Book()
                    {
                        Name = bookDto.Name,
                        Genre = (Genre)bookDto.Genre,
                        Price = bookDto.Price,
                        Pages = bookDto.Pages,
                        PublishedOn = publishedOnDate
                    };

                    books.Add(currBook);

                    sb.AppendLine(String.Format(SuccessfullyImportedBook,
                      currBook.Name, currBook.Price));

                }

                context.Books.AddRange(books);
                context.SaveChanges();

                return sb.ToString().TrimEnd();
            }
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var authorsDtos = JsonConvert.DeserializeObject<List<ImportAuthorDto>>(jsonString);

            var authors = new List<Author>();

            var sb = new StringBuilder();

            foreach (var authDto in authorsDtos)
            {
                if (!IsValid(authDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //check if email already exists

                bool doesMailExist = authors
                    .FirstOrDefault(a => a.Email == authDto.Email) != null; //hmmmmmm ?!

                if (doesMailExist)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;

                }


                //create author

                var newAuthor = new Author()
                {
                    FirstName = authDto.FirstName,
                    LastName = authDto.LastName,
                    Email = authDto.Email,
                    Phone = authDto.Phone,
                };

                // check all books

                foreach (var book in authDto.Books)
                {
                    var bookToImport = context.Books.Find(book.Id);

                    if (bookToImport == null)
                    {
                        continue;
                    }

                    newAuthor.AuthorsBooks.Add(new AuthorBook()
                    {
                        Book = bookToImport,
                        Author = newAuthor
                    });
                }


                if (newAuthor.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;

                }



                authors.Add(newAuthor);
                sb.AppendLine(String.Format(SuccessfullyImportedAuthor,
                    (newAuthor.FirstName + " " + newAuthor.LastName), newAuthor.AuthorsBooks.Count));

            }

            context.Authors.AddRange(authors);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}