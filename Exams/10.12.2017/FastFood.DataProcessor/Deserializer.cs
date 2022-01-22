using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using FastFood.Data;
using FastFood.DataProcessor.Dto.Import;
using FastFood.Models;
using FastFood.Models.Enums;
using Newtonsoft.Json;

namespace FastFood.DataProcessor
{
    public static class Deserializer
    {
        private const string FailureMessage = "Invalid data format.";
        private const string SuccessMessage = "Record {0} successfully imported.";

        public static string ImportEmployees(FastFoodDbContext context, string jsonString)
        {
            var employeesDtos = JsonConvert.DeserializeObject<List<ImportEmployeeDto>>(jsonString);
            var employees = new List<Employee>();

            var sb = new StringBuilder();

            foreach (var empDto in employeesDtos)
            {
                //check if valid via attributes
                if (!IsValid(empDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }


                bool doesPositionExist = context.Positions.Any(p => p.Name == empDto.Position);

                if (!doesPositionExist)
                {
                    var position = new Position
                    {
                        Name = empDto.Position
                    };



                    context.Positions.Add(position);
                    context.SaveChanges();
                }

                var positionToAdd = context.Positions.FirstOrDefault(p => p.Name == empDto.Position);

                var employee = new Employee()
                {
                    Name = empDto.Name,
                    Age = empDto.Age,
                    Position = positionToAdd

                };

                employees.Add(employee);

                sb.AppendLine(String.Format(SuccessMessage, employee.Name));

            }

            context.Employees.AddRange(employees);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

        public static string ImportItems(FastFoodDbContext context, string jsonString)
        {
            var itemsDtos = JsonConvert.DeserializeObject<List<ImportItemDto>>(jsonString);

            var sb = new StringBuilder();

            foreach (var itemDto in itemsDtos)
            {
                //check if valid via attributes
                if (!IsValid(itemDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                bool doesItemExist = context.Items.Any(x => x.Name == itemDto.Name);

                if (doesItemExist)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }


                bool doesCategoryExist = context.Categories.Any(x => x.Name == itemDto.Category);

                if (!doesCategoryExist)
                {
                    var category = new Category
                    {
                        Name = itemDto.Category
                    };


                    context.Categories.Add(category);
                    context.SaveChanges();
                }

                var categoryToAdd = context.Categories.FirstOrDefault(x => x.Name == itemDto.Category);

                var item = new Item()
                {
                    Name = itemDto.Name,
                    Price = itemDto.Price,
                    Category = categoryToAdd
                };

                context.Items.Add(item);
                context.SaveChanges();

                sb.AppendLine(String.Format(SuccessMessage, item.Name));

            }

            return sb.ToString().Trim();
        }

        public static string ImportOrders(FastFoodDbContext context, string xmlString)
        {
            var XmlSerializer = new XmlSerializer(typeof(List<ImportXmlOrderDto>), new XmlRootAttribute("Orders"));

            using (var reader = new StringReader(xmlString))
            {
                var ordersDtos = (List<ImportXmlOrderDto>)XmlSerializer.Deserialize(reader);

                var orders = new List<Order>();
                var sb = new StringBuilder();

                foreach (var orderDto in ordersDtos)
                {
                    if (!IsValid(orderDto))
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }

                    //check if employee exists

                    bool doesEmployeeExist = context.Employees.Any(x => x.Name == orderDto.Employee);

                    if (!doesEmployeeExist)
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }

                    var employeeToAdd = context.Employees.FirstOrDefault(x => x.Name == orderDto.Employee);

                    //check datetime

                    DateTime dateTime;
                    bool isValidDate = DateTime.TryParseExact(orderDto.DateTime, "dd/MM/yyyy HH:mm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

                    if (!isValidDate)
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }

                    //check type

                    object orderType;
                    bool isValidType = Enum.TryParse(typeof(OrderType), orderDto.Type, out orderType);

                    if (!isValidType)
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }


                    //create order

                    var currOrder = new Order()
                    {
                        Customer = orderDto.Customer,
                        DateTime = dateTime,
                        Type = (OrderType)orderType,
                        Employee = employeeToAdd,
                        EmployeeId = employeeToAdd.Id
                    };


                    //check items

                    if (orderDto.Items.Any(x => !IsValid(x)))
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }


                    //check if item exist

                    bool doAllItemsExist = true;

                    foreach (var itemDto in orderDto.Items)
                    {
                        if (context.Items.Any(x => x.Name == itemDto.Name) == false)
                        {
                            doAllItemsExist = false;

                            sb.AppendLine(FailureMessage);//??
                            break;
                        }

                        var itemToAdd = context.Items.FirstOrDefault(x => x.Name == itemDto.Name);


                        var currOrderItem = new OrderItem()
                        {
                            Order = currOrder,
                            Item = itemToAdd,
                            Quantity = itemDto.Quantity
                        };

                        currOrder.OrderItems.Add(currOrderItem);

                    }

                    if (!doAllItemsExist)
                    {
                        sb.AppendLine(FailureMessage);
                        continue;
                    }


                    orders.Add(currOrder);

                    sb.AppendLine($"Order for {currOrder.Customer} on {currOrder.DateTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)} added");

                }

                context.Orders.AddRange(orders);
                context.SaveChanges();

                return sb.ToString().Trim();


            }
        }


        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}