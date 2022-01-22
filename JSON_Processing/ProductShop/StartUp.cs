using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        private const string DatasetPath = "../../../Datasets";
        private const string DatasetResultPath = "../../../Datasets/Results";

        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            //1.Import users
            var usersData = File.ReadAllText($"{DatasetPath}/users.json");
            Console.WriteLine(ImportUsers(context, usersData));

            //2.Import Products
            var productsData = File.ReadAllText($"{DatasetPath}/products.json");
            Console.WriteLine(ImportProducts(context, productsData));

            //3.Import Categories
            var categoriesData = File.ReadAllText($"{DatasetPath}/categories.json");
            Console.WriteLine(ImportCategories(context, categoriesData));

            //4.Import Categories and Products
            var categoriesProductsData = File.ReadAllText($"{DatasetPath}/categories-products.json");
            Console.WriteLine(ImportCategoryProducts(context, categoriesProductsData));

            //5.Get Products in Range
            var products = GetProductsInRange(context);
            Console.WriteLine(products);
            File.WriteAllText($"{DatasetResultPath}/products-in-range.json", products);

            //6.Export Successfully Sold Products
            var users = GetSoldProducts(context);
            Console.WriteLine(users);
            File.WriteAllText($"{DatasetResultPath}/users-with-sold-products.json", users);

            //7.Export Categories by Products Count
            var categories = GetCategoriesByProductsCount(context);
            Console.WriteLine(categories);
            File.WriteAllText($"{DatasetResultPath}/categories-by-products.json", categories);

            //8.Export Users and Products
            var usersWithProducts = GetUsersWithProducts(context);
            Console.WriteLine(usersWithProducts);
            File.WriteAllText($"{DatasetResultPath}/users-with-products", usersWithProducts);
        }


        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<List<User>>(inputJson);

            context.Users.AddRange(users);

            context.SaveChanges();

            return $"Successfully imported {users.Count()}";


        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<List<Product>>(inputJson);

            context.Products.AddRange(products);

            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {

            var categories = JsonConvert.DeserializeObject<List<Category>>(inputJson)
                .Where(c => c.Name != null)
                .ToList();

            context.Categories.AddRange(categories);

            context.SaveChanges();

            return $"Successfully imported {categories.Count()}";


        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categoriesProducts = JsonConvert.DeserializeObject<List<CategoryProduct>>(inputJson);

            context.CategoryProducts.AddRange(categoriesProducts);

            context.SaveChanges();

            return $"Successfully imported {context.CategoryProducts.Count()}";


        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                            .Where(p => p.Price >= 500 && p.Price <= 1000)
                            .Select(p => new
                            {
                                Name = p.Name,
                                Price = p.Price,
                                Seller = p.Seller.FirstName + " " + p.Seller.LastName
                            })
                            .OrderBy(p => p.Price)
                            .ToList();

            var settings = SetJsonConvert();

            var result = JsonConvert.SerializeObject(products, settings);

            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                        .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                        .Select(u => new
                        {
                            u.FirstName,
                            u.LastName,
                            SoldProducts = u.ProductsSold.Select(p => new
                            {
                                p.Name,
                                p.Price,
                                BuyerFirstName = p.Buyer.FirstName,
                                BuyerLastName = p.Buyer.LastName
                            }).ToList()
                        })
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToList();

            var settings = SetJsonConvert();

            var result = JsonConvert.SerializeObject(users, settings);

            return result;
        }



        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(c => new
                {
                    Category = c.Name,
                    ProductsCount = c.CategoryProducts.Count,
                    AveragePrice = $"{c.CategoryProducts.Average(x => x.Product.Price):f2}",
                    TotalRevenue = $"{c.CategoryProducts.Sum(x => x.Product.Price):f2}"
                })
                .OrderByDescending(c => c.ProductsCount)
                .ToList();

            var settings = SetJsonConvert();

            var result = JsonConvert.SerializeObject(categories, settings);

            return result;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Count > 0)
                .Select(u => new
                {
                    //u.FirstName,
                    u.LastName,
                    u.Age,
                    SoldProducts = new
                    {
                        Count = u.ProductsSold.Count(p => p.Buyer != null),
                        Products = u.ProductsSold.Where(p => p.Buyer != null)
                                .Select(p => new
                                {
                                    p.Name,
                                    p.Price
                                })
                                .ToList()
                    }

                })
                .OrderByDescending(u => u.SoldProducts.Count)
                .ToList();

            var dataObj = new
            {
                UsersCount = users.Count,
                Users = users
            };

            var settings = SetJsonConvert();

            var result = JsonConvert.SerializeObject(dataObj, settings);

            return result;
        }



        private static JsonSerializerSettings SetJsonConvert()
        {
            var contractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            };
            return settings;
        }
    }
}