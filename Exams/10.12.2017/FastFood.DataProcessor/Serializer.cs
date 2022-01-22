using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FastFood.Data;
using FastFood.DataProcessor.Dto.Export;
using Newtonsoft.Json;

namespace FastFood.DataProcessor
{
    public class Serializer
    {
        public static string ExportOrdersByEmployee(FastFoodDbContext context, string employeeName, string orderType)
        {
            var emplOrders = context.Employees
                .Where(e => e.Name == employeeName)
                .ToList()
                .Select(e => new
                {
                    Name = e.Name,
                    Orders = e.Orders.Where(o => o.Type.ToString() == orderType)
                            .Select(o => new
                            {
                                Customer = o.Customer,
                                Items = o.OrderItems
                                    .Select(oi => new
                                    {
                                        Name = oi.Item.Name,
                                        Price = decimal.Parse($"{oi.Item.Price:f2}"),
                                        Quantity = oi.Quantity

                                    }).ToList(),

                                TotalPrice = decimal.Parse($"{o.TotalPrice:f2}")

                            })
                            .OrderByDescending(o => o.TotalPrice)
                            .ThenByDescending(o => o.Items.Count)
                            .ToList(),

                    TotalMade = e.Orders.Sum(o => decimal.Parse($"{o.TotalPrice:f2}"))

                }).ToList();

            var result = JsonConvert.SerializeObject(emplOrders, Formatting.Indented);
            return result;
        }

        public static string ExportCategoryStatistics(FastFoodDbContext context, string categoriesString)
        {
            var categoryNames = categoriesString.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            var categories = context.Categories
                .Where(c => categoryNames.Contains(c.Name))
                .ToList()
                .Select(c => new ExportCategoryDto()
                {
                    Name = c.Name,

                    MostPopularItem = c.Items.Select(i => new ExportPopularItemDto()
                    {

                        Name = i.Name,
                        TimesSold = i.OrderItems.Sum(oi => oi.Quantity),
                        TotalMade = i.OrderItems.Sum(oi => oi.Quantity * i.Price)

                    }).OrderByDescending(x => x.TotalMade)
                    .FirstOrDefault()
                })
                .OrderByDescending(c => c.MostPopularItem.TotalMade)
                .ThenByDescending(c => c.MostPopularItem.TimesSold)
                .ToList();

            var sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            var XmlSerializer = new XmlSerializer(typeof(List<ExportCategoryDto>), new XmlRootAttribute("Categories"));
            XmlSerializer.Serialize(new StringWriter(sb), categories, namespaces);

            return sb.ToString().Trim();
        }
    }
}