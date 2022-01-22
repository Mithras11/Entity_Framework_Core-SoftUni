using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {
        //Directories
        private const string DatasetsPath = "../../../Datasets";
        private const string DatasetsResultsPath = "../../../Datasets/Results";


        public static void Main(string[] args)
        {
            var db = new CarDealerContext();
            ResetDatabase(db);
            InitializeMapper();

            //9.Import Suppliers
            var suppliersData = File.ReadAllText($"{DatasetsPath}/suppliers.json");
            Console.WriteLine(ImportSuppliers(db, suppliersData));

            //10.Import Parts
            var partsData = File.ReadAllText($"{DatasetsPath}/parts.json");
            Console.WriteLine(ImportParts(db, partsData));

            //11.Import Cars
            var carsData = File.ReadAllText($"{DatasetsPath}/cars.json");
            Console.WriteLine(ImportCars(db, carsData));

            //12.Import Customers
            var customersData = File.ReadAllText($"{DatasetsPath}/customers.json");
            Console.WriteLine(ImportCustomers(db, customersData));

            //13.Import Sales
            var salesData = File.ReadAllText($"{DatasetsPath}/sales.json");
            Console.WriteLine(ImportSales(db, salesData));

            //14.Export Ordered Customers
            var orderedCustomers = GetOrderedCustomers(db);
            Console.WriteLine(orderedCustomers);
            File.WriteAllText($"{DatasetsResultsPath}/ordered-customers.json", orderedCustomers);

            //15.Export Cars from Toyota
            var toyotaCars = GetCarsFromMakeToyota(db);
            Console.WriteLine(toyotaCars);
            File.WriteAllText($"{DatasetsResultsPath}/toyota-cars.json", toyotaCars);

            //16.Export Local Suppliers
            var localSuppliers = GetLocalSuppliers(db);
            Console.WriteLine(localSuppliers);
            File.WriteAllText($"{DatasetsResultsPath}/local-suppliers.json", localSuppliers);

            //17.Export Cars with Their List of Parts
            var carsWithParts = GetCarsWithTheirListOfParts(db);
            Console.WriteLine(carsWithParts);
            File.WriteAllText($"{DatasetsResultsPath}/cars-with-list-of-parts.json", carsWithParts);

            ////18. Export Total Sales By Customer
            var totalSalesByCustomer = GetTotalSalesByCustomer(db);
            Console.WriteLine(totalSalesByCustomer);
            File.WriteAllText($"{DatasetsResultsPath}/total-sales-by-customer.json", totalSalesByCustomer);

            //19. Export Sales with and without discount
            var saleswithDiscount = GetSalesWithAppliedDiscount(db);
            Console.WriteLine(saleswithDiscount);
            File.WriteAllText($"{DatasetsResultsPath}/sales-with-discount.json", saleswithDiscount);

        }

        //Import Methods

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var suppliers = JsonConvert.DeserializeObject<List<Supplier>>(inputJson);

            context.AddRange(suppliers);

            context.SaveChanges();

            return $"Successfully imported {suppliers.Count()}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {

            var parts = JsonConvert.DeserializeObject<List<Part>>(inputJson)
                        .Where(p => Enumerable.Range(context.Suppliers.Min(s => s.Id),
                                                    context.Suppliers.Max(s => s.Id))
                                    .Contains(p.SupplierId))
                        .ToList();

            context.AddRange(parts);

            context.SaveChanges();

            return $"Successfully imported {parts.Count}.";

        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            //var cars = JsonConvert.DeserializeObject<List<Car>>(inputJson);

            //context.AddRange(cars);

            //context.SaveChanges();

            //return $"Successfully imported {cars.Count}.";


            var jsonCars = JsonConvert.DeserializeObject<List<ImportCarDTO>>(inputJson);

            var cars = new List<Car>();

            foreach (var carDTO in jsonCars)
            {
                var car = new Car()
                {
                    Make = carDTO.Make,
                    Model = carDTO.Model,
                    TravelledDistance = carDTO.TravelledDistance,
                };

                foreach (var partId in carDTO.partsId.Distinct())
                {
                    car.PartCars.Add(new PartCar()
                    {
                        Car = car,
                        PartId = partId
                    });
                }

                cars.Add(car);

            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}.";


        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var customers = JsonConvert.DeserializeObject<List<Customer>>(inputJson);

            context.AddRange(customers);

            context.SaveChanges();

            return $"Successfully imported {customers.Count}.";
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var sales = JsonConvert.DeserializeObject<List<Sale>>(inputJson);

            context.AddRange(sales);

            context.SaveChanges();

            return $"Successfully imported {sales.Count}.";
        }


        //Export methods
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new CustomerDTO
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = c.IsYoungDriver
                })
                .ToList();

            var result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var toyotaCars = context.Cars
                .Where(c => c.Make == "Toyota")
                //.Select(c => new CarDTO
                //{
                //    Id = c.Id,
                //    Make = c.Make,
                //    Model = c.Model,
                //    TravelledDistance = c.TravelledDistance
                //})
                .ProjectTo<CarDTO>()
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .ToList();

            var result = JsonConvert.SerializeObject(toyotaCars, Formatting.Indented);

            return result;

        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                //.Select(s => new SupplierDTO
                //{
                //    Id = s.Id,
                //    Name = s.Name,
                //    PartsCount = s.Parts.Count
                //})
                .ProjectTo<SupplierDTO>()
                .ToList();

            var result = JsonConvert.SerializeObject(suppliers, Formatting.Indented);

            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsWithParts = context.Cars
                .Select(c => new
                {
                    car = new
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TravelledDistance = c.TravelledDistance
                    },

                    parts = c.PartCars.Select(pc => new
                    {
                        Name = pc.Part.Name,
                        Price = pc.Part.Price.ToString("f2")
                    })
                    .ToList()
                })
                .ToList();

            var result = JsonConvert.SerializeObject(carsWithParts, Formatting.Indented);

            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            //var customers = context.Customers
            //    .Where(c => c.Sales.Count > 0)
            //    .Select(s => new
            //    {
            //        fullName = s.Name,
            //        boughtCars = s.Sales.Count,
            //        spentMoney = s.Sales
            //        .SelectMany(s => s.Car.PartCars.Select(pc => pc.Part.Price)).Sum()
            //    })
            //     .OrderByDescending(c => c.boughtCars)
            //     .ThenByDescending(c => c.spentMoney)
            //     .ToList();



            var customers = context
                    .Customers
                    .ProjectTo<CustomerTotalSalesDTO>()
                    .Where(c => c.BoughtCars >= 1)
                    .OrderByDescending(c => c.SpentMoney)
                    .ThenByDescending(c => c.BoughtCars)
                    .ToList();

            string result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;

        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Take(10)
                .Select(s => new
                {
                    car = new
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },

                    customerName = s.Customer.Name,
                    Discount = s.Discount.ToString("f2"),
                    price = (s.Car.PartCars.Sum(pc => pc.Part.Price)).ToString("f2"),
                    priceWithDiscount =
                                    (s.Car.PartCars.Sum(pc => pc.Part.Price) - s.Car.PartCars.Sum(pc => pc.Part.Price) * (s.Discount / 100))
                                    .ToString("f2")

                }).ToList();

            var result = JsonConvert.SerializeObject(sales, Formatting.Indented);
            return result;


        }



        //Configurations

        private static void ResetDatabase(CarDealerContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private static void InitializeMapper()
        {
            Mapper.Initialize(config =>
            {
                config.AddProfile<CarDealerProfile>();
            });
        }
    }
}