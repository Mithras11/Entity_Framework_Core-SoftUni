namespace TeisterMask.DataProcessor
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
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var XmlSerializer = new XmlSerializer(typeof(List<ImportProjectDto>), new XmlRootAttribute("Projects"));

            using (var reader = new StringReader(xmlString))
            {

                var projectsDtos = (List<ImportProjectDto>)XmlSerializer.Deserialize(reader);

                var projects = new List<Project>();
                var sb = new StringBuilder();

                foreach (var projDto in projectsDtos)
                {
                    //validate project

                    if (!IsValid(projDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //-------------------

                    DateTime openDate;
                    bool isValidOpenDate = DateTime.TryParseExact(projDto.OpenDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out openDate);

                    if (!isValidOpenDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //-------------------

                    DateTime? dueDate;

                    if (!String.IsNullOrEmpty(projDto.DueDate))
                    {
                        DateTime tempDueDate;

                        bool isValidDueDate = DateTime.TryParseExact(projDto.DueDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDueDate);

                        if (!isValidDueDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        dueDate = tempDueDate; //!!!!

                    }
                    else
                    {
                        dueDate = null;
                    }

                    //----------

                    var project = new Project()
                    {
                        Name = projDto.Name,
                        OpenDate = openDate,
                        DueDate = dueDate,
                    };

                    //validate tasks

                    foreach (var taskDto in projDto.Tasks)
                    {
                        if (!IsValid(taskDto))
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        //------
                        DateTime taskOpenDate;
                        bool isValidTaskOpenDate = DateTime.TryParseExact(taskDto.OpenDate, "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out taskOpenDate);

                        if (!isValidOpenDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (taskOpenDate < project.OpenDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        //---------------------

                        DateTime taskDueDate;

                        bool isValidTaskDueDate = DateTime.TryParseExact(taskDto.DueDate, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out taskDueDate);

                        if (!isValidTaskDueDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (taskDueDate > project.DueDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        //add task to project

                        var task = new Task()
                        {
                            Name = taskDto.Name,
                            OpenDate = taskOpenDate,
                            DueDate = taskDueDate,
                            ExecutionType = (ExecutionType)taskDto.ExecutionType,
                            LabelType = (LabelType)taskDto.LabelType
                        };

                        project.Tasks.Add(task);
                    }

                    projects.Add(project);
                    sb.AppendLine(String.Format(SuccessfullyImportedProject, project.Name, project.Tasks.Count));

                }


                context.Projects.AddRange(projects);
                context.SaveChanges();

                return sb.ToString().Trim();
            }
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var employeesDtos = JsonConvert.DeserializeObject<List<ImportEmployeeDto>>(jsonString);
            var taskIds = context.Tasks.Select(t => t.Id).ToList();

            var employees = new List<Employee>();
            var sb = new StringBuilder();

            foreach (var empDto in employeesDtos)
            {
                //validate employee

                if (!IsValid(empDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //----
                if (!IsUsernameValid(empDto.Username))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //create Employee

                var employee = new Employee()
                {
                    Username = empDto.Username,
                    Email = empDto.Email,
                    Phone = empDto.Phone
                };

                //validate tasks

                foreach (var taskId in empDto.Tasks.Distinct())
                {                    

                    if (!taskIds.Contains(taskId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var employeeTask = new EmployeeTask()
                    {
                        Employee = employee,
                        TaskId = taskId
                    };

                    employee.EmployeesTasks.Add(employeeTask);

                }

                employees.Add(employee);
                sb.AppendLine(String.Format(SuccessfullyImportedEmployee, employee.Username, employee.EmployeesTasks.Count));

            }

            context.Employees.AddRange(employees);
            context.SaveChanges();

            return sb.ToString().Trim();

        }

        private static bool IsUsernameValid(string name)
        {
            foreach (var ch in name.ToCharArray())
            {
                if (!char.IsLetterOrDigit(ch))
                {
                    return false;
                }

            }

            return true;
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}