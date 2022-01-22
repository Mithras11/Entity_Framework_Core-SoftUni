using AutoMapper;
using AutoMapper.QueryableExtensions;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        //Directories
        private const string DatasetsPath = "../../../Datasets";
        private const string DatasetsResultsPath = "../../../Datasets/Results";

        public static void Main(string[] args)
        {
            var db = new ProductShopContext();
            //ResetDatabase(db);
            InitializeMapper();

            //1. Import Suppliers
            var usersXml = File.ReadAllText($"{DatasetsPath}/users.xml");
            Console.WriteLine(ImportUsers(db, usersXml));

            //2. Import Products
            var productsXml = File.ReadAllText($"{DatasetsPath}/products.xml");
            Console.WriteLine(ImportProducts(db, productsXml));

            //3. Import Categories
            var categoriesXml = File.ReadAllText($"{DatasetsPath}/categories.xml");
            Console.WriteLine(ImportCategories(db, categoriesXml));

            //4. Import CategoriesProducts
            var categoriesProductsXml = File.ReadAllText($"{DatasetsPath}/categories-products.xml");
            Console.WriteLine(ImportCategoryProducts(db, categoriesProductsXml));

            //5. Export Products in range
            var productsInRange = GetProductsInRange(db);
            File.WriteAllText($"{DatasetsResultsPath}/products-in-range.xml", productsInRange);

            //6. Sold Products
            var soldProducts = GetSoldProducts(db);
            File.WriteAllText($"{DatasetsResultsPath}/users-sold-products.xml", soldProducts);

            //7. Categories by products count
            var categoriesByCount = GetCategoriesByProductsCount(db);
            File.WriteAllText($"{DatasetsResultsPath}/categories-by-products.xml", categoriesByCount);

            //8. Users and Products
            var usersWithProducts = GetUsersWithProducts(db);
            File.WriteAllText($"{DatasetsResultsPath}/users-and-products.xml", usersWithProducts);

        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = new UserRootDTO()
            {
                Count = context.Users.Count(u => u.FirstName != null),

                Users = context.Users
                    .Where(u => u.ProductsSold.Count > 0)
                    .OrderByDescending(u => u.ProductsSold.Count)
                    .Select(u => new UserExportDTO
                    {
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Age = u.Age,
                        SoldProducts = new ProductSoldRootDTO
                        {
                            Count = u.ProductsSold.Count(p => p.Buyer != null),
                            Products = u.ProductsSold.Where(p => p.Buyer != null)
                                .Select(p => new ProductSoldDTO
                                {
                                    Name = p.Name,
                                    Price = p.Price
                                })
                                .OrderByDescending(p => p.Price)
                                .ToList()
                        }
                    })
                    .Take(10)
                    .ToList()
            };
  

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(UserRootDTO), new XmlRootAttribute("Users"));
            XmlSerializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().Trim();

        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(c => new CategoryProductDto
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(x => x.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(x => x.Product.Price)
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToList();

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<CategoryProductDto>), new XmlRootAttribute("Categories"));
            XmlSerializer.Serialize(new StringWriter(sb), categories, namespaces);

            return sb.ToString().Trim();

        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                       .Where(u => u.ProductsSold.Count > 0)
                       .Select(u => new ExportUserDto
                       {
                           FirstName = u.FirstName,
                           LastName = u.LastName,
                           SoldProducts = u.ProductsSold
                                .Select(p => new ProductSoldDTO
                                {
                                    Name = p.Name,
                                    Price = p.Price,
                                }).ToList()
                       })
                        //.ProjectTo<ExportUserDto>()
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .Take(5)
                        .ToList();



            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<ExportUserDto>), new XmlRootAttribute("Users"));
            XmlSerializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().Trim();

        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                            .Where(p => p.Price >= 500 && p.Price <= 1000)
                            .OrderBy(p => p.Price)
                            //.Select(p => new ProductInRangeDto
                            //{
                            //    Name = p.Name,
                            //    Price = p.Price,
                            //    Buyer = p.Buyer.FirstName + " " + p.Buyer.LastName
                            //})
                            .ProjectTo<ProductInRangeDto>()
                            .Take(10)
                            .ToList();


            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<ProductInRangeDto>), new XmlRootAttribute("Products"));
            XmlSerializer.Serialize(new StringWriter(sb), products, namespaces);

            return sb.ToString().Trim();

        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportCategoryProductDto[]), new XmlRootAttribute("CategoryProducts"));

            var categoriesProductsDtos = (ImportCategoryProductDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            var categoriesProducts = new List<CategoryProduct>();

            foreach (var item in categoriesProductsDtos)
            {
                if (context.Categories.Any(c => c.Id == item.CategoryId) &&
                    context.Products.Any(p => p.Id == item.ProductId))
                {

                    var categoryProduct = new CategoryProduct()
                    {
                        CategoryId = item.CategoryId,
                        ProductId = item.ProductId
                    };

                    categoriesProducts.Add(categoryProduct);
                }
            }

            context.CategoryProducts.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Count}";

        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportCategoryDto[]), new XmlRootAttribute("Categories"));

            var categoriesDtos = (ImportCategoryDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            //var categories = Mapper.Map<Category[]>(categoriesDtos.Where(c => c.Name != null)).ToList();

            var categories = new List<Category>();

            foreach (var item in categoriesDtos.Where(c => c.Name != null))
            {
                var category = new Category()
                {
                    Name = item.Name

                };

                categories.Add(category);
            }

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count}";


        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportProductDto[]), new XmlRootAttribute("Products"));

            var productsDtos = (ImportProductDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            //var products = Mapper.Map<Product[]>(productsDtos);

            var products = new List<Product>();

            foreach (var item in productsDtos)
            {
                var product = new Product()
                {
                    Name = item.Name,
                    Price = item.Price,
                    SellerId = item.SellerId,
                    BuyerId = item.BuyerId
                };

                products.Add(product);
            }

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count}";
        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportUserDto[]), new XmlRootAttribute("Users"));

            var usersDtos = (ImportUserDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            var users = Mapper.Map<User[]>(usersDtos);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }



        //Configurations

        private static void ResetDatabase(ProductShopContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private static void InitializeMapper()
        {
            Mapper.Initialize(config =>
            {
                config.AddProfile<ProductShopProfile>();
            });
        }




    }
}