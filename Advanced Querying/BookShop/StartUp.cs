namespace BookShop
{
    using System;
    using System.Linq;
    using System.Text;
    using Models.Enums;
    using Data;
    using Initializer;
    using System.Globalization;

    public class StartUp
    {
        public static void Main()
        {
            var db = new BookShopContext();
            //DbInitializer.ResetDatabase(db);

            //var command = Console.ReadLine();
            //Console.WriteLine(GetBooksByAgeRestriction(db, command));

            //Console.WriteLine(GetGoldenBooks(db));

            //Console.WriteLine(GetBooksByPrice(db));

            //var year = int.Parse(Console.ReadLine());
            //Console.WriteLine(GetBooksNotReleasedIn(db, year));

            //var categories = Console.ReadLine();
            //Console.WriteLine(GetBooksByCategory(db, categories));

            //var date = Console.ReadLine();
            //Console.WriteLine(GetBooksReleasedBefore(db, date));

            //var ending = Console.ReadLine();
            //Console.WriteLine(GetAuthorNamesEndingIn(db,ending));    

            //var input = Console.ReadLine();
            //Console.WriteLine(GetBookTitlesContaining(db, input));

            var input = Console.ReadLine();
            Console.WriteLine(GetBooksByAuthor(db, input));

            //var lengthCheck = int.Parse(Console.ReadLine());
            //Console.WriteLine(CountBooks(db, lengthCheck));

            //Console.WriteLine(CountCopiesByAuthor(db));

            //Console.WriteLine(GetTotalProfitByCategory(db));

            //Console.WriteLine(GetMostRecentBooks(db));

            //IncreasePrices(db);

            //Console.WriteLine(RemoveBooks(db));
        }


        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var books = context.Books
                .Where(b => b.AgeRestriction == Enum.Parse<AgeRestriction>(command, true))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }

            return sb.ToString().Trim();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.EditionType == EditionType.Gold &&
                        b.Copies < 5000)
                .Select(b => new { b.BookId, b.Title })
                .OrderBy(b => b.BookId)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().Trim();

        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Price > 40)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .OrderByDescending(b => b.Price)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:F2}");
            }

            return sb.ToString().Trim();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                 .Select(b => new
                 {
                     b.BookId,
                     b.Title
                 })
                .OrderBy(b => b.BookId)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }

            return sb.ToString().Trim();

        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var listOfCategories = input
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.ToLower())
                .ToList();

            var books = context.Books
                .Where(b => b.BookCategories
                            .Any(bc => listOfCategories
                                       .Contains(bc.Category.Name.ToLower())))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }

            return sb.ToString().Trim();

        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var formattedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context.Books
               .Where(b => b.ReleaseDate < formattedDate)
               .Select(b => new
               {
                   b.Title,
                   b.EditionType,
                   b.Price,
                   b.ReleaseDate

               })
               .OrderByDescending(b => b.ReleaseDate)
               .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:F2}");
            }

            return sb.ToString().Trim();

        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
               .Where(a => a.FirstName.EndsWith(input))
               .Select(a => new
               {
                   FullName = a.FirstName + " " + a.LastName
               })
               .OrderBy(a => a.FullName)
               .ToList();

            var sb = new StringBuilder();

            foreach (var author in authors)
            {
                sb.AppendLine(author.FullName);
            }

            return sb.ToString().Trim();

        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
            .Where(b => b.Title.ToLower().Contains(input.ToLower()))
            .Select(b => b.Title)
            .OrderBy(b => b)
            .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }

            return sb.ToString().Trim();

        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
              .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
              .Select(b => new
              {
                  b.BookId,
                  b.Title,
                  AuthorFullName = b.Author.FirstName + " " + b.Author.LastName
              })
              .OrderBy(b => b.BookId)
              .ToList();

            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} ({book.AuthorFullName})");
            }

            return sb.ToString().Trim();

        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var booksCount = context.Books
               .Where(b => b.Title.Length > lengthCheck)
               .Count();

            return booksCount;

        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var books = context.Authors
              .Select(a => new
              {
                  AuthorFullName = a.FirstName + " " + a.LastName,
                  TotalBooksCount = a.Books.Sum(b => b.Copies)
              })
              .OrderByDescending(a => a.TotalBooksCount)
              .ToList();


            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.AuthorFullName} - {book.TotalBooksCount}");
            }

            return sb.ToString().Trim();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var books = context.Categories
             .Select(c => new
             {
                 CategoryName = c.Name,
                 TotalProfit = c.CategoryBooks.Sum(cb => cb.Book.Price * cb.Book.Copies)
             })
             .OrderByDescending(x => x.TotalProfit)
             .ThenBy(x => x.CategoryName)
             .ToList();


            var sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.CategoryName} ${book.TotalProfit:F2}");
            }

            return sb.ToString().Trim();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var list = context.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    Books = c.CategoryBooks
                            .Select(cb => cb.Book)
                            .OrderByDescending(b => b.ReleaseDate)
                            .Take(3)
                            .ToList()
                })
                .OrderBy(x => x.CategoryName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var item in list)
            {
                sb.AppendLine($"--{item.CategoryName}");

                foreach (var book in item.Books)
                {
                    sb.AppendLine($"{book.Title} ({book.ReleaseDate.Value.Year})");
                }
            }

            return sb.ToString().Trim();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Copies < 4200)
                .ToList();

            var count = books.Count();

            var booksCategories = context.BooksCategories
                .Where(bc => bc.Book.Copies < 4200)
                .ToList();

            context.BooksCategories.RemoveRange(booksCategories);
            context.Books.RemoveRange(books);

            context.SaveChanges();

            return count;
        }
    }
}
