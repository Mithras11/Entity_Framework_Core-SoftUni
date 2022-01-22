using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarDealer.Data;
using CarDealer.Dto;
using CarDealer.Dto.Export;
using CarDealer.Dto.Import;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        private const string DATASETS_PATH = "../../../Datasets";
        private const string DATASETS_RESULTS_PATH = "../../../Datasets/Results";
        public static void Main(string[] args)
        {
            var db = new CarDealerContext();
            ResetDatabase(db);
            InitializeMapper();

            //1. Import suppliers
            var xmlSuppliers = File.ReadAllText($"{DATASETS_PATH}/suppliers.xml");
            Console.WriteLine(ImportSuppliers(db, xmlSuppliers));

            //2. Import parts
            var xmlParts = File.ReadAllText($"{DATASETS_PATH}/parts.xml");
            Console.WriteLine(ImportParts(db, xmlParts));

            //3. Import cars
            var xmlCars = File.ReadAllText($"{DATASETS_PATH}/cars.xml");
            Console.WriteLine(ImportCars(db, xmlCars));

            //4.Import customers
            var xmlCustomers = File.ReadAllText($"{DATASETS_PATH}/customers.xml");
            Console.WriteLine(ImportCustomers(db, xmlCustomers));

            //5.Import sales
            var xmlSales = File.ReadAllText($"{DATASETS_PATH}/sales.xml");
            Console.WriteLine(ImportSales(db, xmlSales));

            //6. Cars with distance
            var carsWithDistance = GetCarsWithDistance(db);
            File.WriteAllText($"{DATASETS_RESULTS_PATH}/cars.xml", carsWithDistance);

            //7.Cars form make BMW
            var BMWcars = GetCarsFromMakeBmw(db);
            File.WriteAllText($"{DATASETS_RESULTS_PATH}/bmw-cars.xml", BMWcars);

            //8.Get local suppliers
            var localSuppliers = GetLocalSuppliers(db);
            File.WriteAllText($"{DATASETS_RESULTS_PATH}/local-suppliers.xml", localSuppliers);

            //9. Get cars with list of parts
            var carsWithParts = GetCarsWithTheirListOfParts(db);
            File.WriteAllText($"{DATASETS_RESULTS_PATH}/cars-and-parts.xml", carsWithParts);


            //10. Total sales
            var customersBySales = GetTotalSalesByCustomer(db);
            File.WriteAllText($"{DATASETS_RESULTS_PATH}/cusotmers-total-sales.xml", customersBySales);

            //11. Sales with discount
            var salesWithDiscount = GetSalesWithAppliedDiscount(db);
            File.WriteAllText($"{DATASETS_RESULTS_PATH}/sales-discounts.xml", salesWithDiscount);

        }

        //Export methods

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            
            var sales = context
               .Sales
               .Select(x => new SaleDto()
               {
                   Car = new CarSaleDto()
                   {
                       Make = x.Car.Make,
                       Model = x.Car.Model,
                       TravelledDistance = x.Car.TravelledDistance
                   },
                   CustomerName = x.Customer.Name,
                   Discount = x.Discount,
                   Price = x.Car.PartCars.Sum(c => c.Part.Price),
                   PriceWithDiscount = x.Car.PartCars.Sum(c => c.Part.Price) - x.Car.PartCars.Sum(c => c.Part.Price) * x.Discount / 100
               })
               .ToList();



            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<SaleDto>), new XmlRootAttribute("sales"));
            XmlSerializer.Serialize(new StringWriter(sb), sales, namespaces);

            return sb.ToString().Trim();

        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
               .Where(c => c.Sales.Count > 0)
               .Select(s => new CustomerBySalesDto()
               {
                   FullName = s.Name,
                   BoughtCars = s.Sales.Count,
                   SpentMoney = s.Sales
                               .SelectMany(s => s.Car.PartCars.Select(pc => pc.Part.Price)).Sum()
               })
                .OrderByDescending(c => c.SpentMoney)
                .ToList();

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<CustomerBySalesDto>), new XmlRootAttribute("customers"));
            XmlSerializer.Serialize(new StringWriter(sb), customers, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var carsWithParts = context.Cars
               .OrderByDescending(c => c.TravelledDistance)
               .ThenBy(c => c.Model)
               .Take(5)
               .Select(c => new CarWIthPartsDto
               {
                   Make = c.Make,
                   Model = c.Model,
                   TravelledDistance = c.TravelledDistance,

                   Parts = c.PartCars.Select(pc => new PartDto
                   {
                       Name = pc.Part.Name,
                       Price = pc.Part.Price
                   })
                   .OrderByDescending(p => p.Price)
                   .ToList()

               })
               .ToList();


            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<CarWIthPartsDto>), new XmlRootAttribute("cars"));
            XmlSerializer.Serialize(new StringWriter(sb), carsWithParts, namespaces);

            return sb.ToString().Trim();

        }
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
               .Where(s => s.IsImporter == false)
               .Select(s => new SupplierDto
               {
                   Id = s.Id,
                   Name = s.Name,
                   PartsCount = s.Parts.Count
               })
               .ToList();

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<SupplierDto>), new XmlRootAttribute("suppliers"));
            XmlSerializer.Serialize(new StringWriter(sb), suppliers, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var bmwCars = context.Cars
               .Where(c => c.Make == "BMW")
               .OrderBy(c => c.Model)
               .ThenByDescending(c => c.TravelledDistance)
               .Select(c => new CarBMWDto
               {
                   Id = c.Id,
                   Model = c.Model,
                   TravelledDistance = c.TravelledDistance
               })
               .ToList();

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<CarBMWDto>), new XmlRootAttribute("cars"));
            XmlSerializer.Serialize(new StringWriter(sb), bmwCars, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                            .Where(c => c.TravelledDistance > 2000000)
                            .OrderBy(c => c.Make)
                            .ThenBy(c => c.Model)
                            .Take(10)
                            .Select(c => new CarWithDistanceDto
                            {
                                Make = c.Make,
                                Model = c.Model,
                                TravelledDistance = c.TravelledDistance
                            })
                            .ToList();


            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<CarWithDistanceDto>), new XmlRootAttribute("cars"));
            XmlSerializer.Serialize(new StringWriter(sb), cars, namespaces);

            return sb.ToString().Trim();

        }

        //Import methods

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportSaleDto[]), new XmlRootAttribute("Sales"));

            var salesDtos = (ImportSaleDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            var sales = Mapper.Map<Sale[]>(salesDtos.Where(s => context.Cars.Any(c => c.Id == s.CarId)).ToArray());

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Length}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {

            var XmlSerializer = new XmlSerializer(typeof(ImportCustomerDto[]), new XmlRootAttribute("Customers"));

            var customersDtos = (ImportCustomerDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            var customers = Mapper.Map<Customer[]>(customersDtos);

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportCarDto[]), new XmlRootAttribute("Cars"));

            var carsDtos = (ImportCarDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            var cars = new List<Car>();
            var partCars = new List<PartCar>();

            foreach (var carDto in carsDtos)
            {
                var car = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance,
                };

                var partIds = carDto.Parts
                    .Where(pc => context.Parts.Any(p => p.Id == pc.PartId))
                    .Select(p => p.PartId)
                    .Distinct();

                foreach (var partId in partIds)
                {
                    var partCar = new PartCar()
                    {
                        PartId = partId,
                        Car = car
                    };

                    partCars.Add(partCar);
                }

                cars.Add(car);
            }

            context.PartCars.AddRange(partCars);
            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportPartDto[]), new XmlRootAttribute("Parts"));

            var partsDtos = (ImportPartDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            var parts = Mapper.Map<Part[]>(partsDtos
                        .Where(p => context.Suppliers.Any(s => s.Id == p.SupplierId)));

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Length}";
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var XmlSerializer = new XmlSerializer(typeof(ImportSupplierDto[]), new XmlRootAttribute("Suppliers"));

            var suppliersDtos = (ImportSupplierDto[])XmlSerializer.Deserialize(new StringReader(inputXml));

            var suppliers = Mapper.Map<Supplier[]>(suppliersDtos);

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}";
        }


        //General settings

        private static void ResetDatabase(CarDealerContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        private static void InitializeMapper()
        {
            Mapper.Initialize(cfg => { cfg.AddProfile<CarDealerProfile>(); });
        }
    }
}