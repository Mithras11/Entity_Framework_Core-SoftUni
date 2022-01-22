namespace TeisterMask.DataProcessor
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
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var projects = context.Projects
                .Where(p => p.Tasks.Count > 0)
                .ToList()
                .Select(p => new ExportXmlProjectDTo()
                {
                    TasksCount = p.Tasks.Count,
                    ProjectName = p.Name,
                    HasEndDate = p.DueDate != null ? "Yes" : "No",

                    Tasks = p.Tasks.Select(t => new ExportXMLTaskDto()
                    {
                        Name = t.Name,
                        Label = t.LabelType.ToString()
                    })
                    .OrderBy(t => t.Name)
                    .ToList()
                })
                .OrderByDescending(p => p.TasksCount)
                .ThenBy(p => p.ProjectName)
                .ToList(); //??


            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(); //!!!!
            namespaces.Add(string.Empty, string.Empty);


            var XmlSerializer = new XmlSerializer(typeof(List<ExportXmlProjectDTo>), new XmlRootAttribute("Projects"));
            XmlSerializer.Serialize(new StringWriter(sb), projects, namespaces);

            return sb.ToString().Trim();
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
                .Where(e => e.EmployeesTasks.Any(t => t.Task.OpenDate >= date))
                .ToList()
                .Select(e => new ExportEmployeeDto
                {
                    Username = e.Username,
                    Tasks = e.EmployeesTasks
                            .Where(t => t.Task.OpenDate >= date)
                            .OrderByDescending(t => t.Task.DueDate)
                            .ThenBy(t => t.Task.Name)
                            .Select(t => new ExportTaskDto
                            {
                                Name = t.Task.Name,
                                OpenDate = t.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                                DueDate = t.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                                LabelType = t.Task.LabelType.ToString(),
                                ExecutionType = t.Task.ExecutionType.ToString()

                            }).ToList()
                })
                .OrderByDescending(e => e.Tasks.Count)
                .ThenBy(e => e.Username)
                .Take(10)  //!!!
                .ToList();

            var result = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return result;
        }
    }
}