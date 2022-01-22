using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var db = new SoftUniContext();

            using (db)
            {
                //03. Employees full info
                //Console.WriteLine(GetEmployeesFullInformation(db));

                //04. Employees with Salary Over 50 000
                //Console.WriteLine(GetEmployeesWithSalaryOver50000(db));

                //05. Employees from Research and Development
                //Console.WriteLine(GetEmployeesFromResearchAndDevelopment(db));

                //06. Adding a New Address and Updating Employee
                //Console.WriteLine(AddNewAddressToEmployee(db));

                //7.Employees and Projects
                //Console.WriteLine(GetEmployeesInPeriod(db));

                //8.Addresses by Town
                //Console.WriteLine(GetAddressesByTown(db));

                //9.Employee 147
                //Console.WriteLine(GetEmployee147(db));

                //10.Departments with More Than 5 Employees
                //Console.WriteLine(GetDepartmentsWithMoreThan5Employees(db));

                //11.Find Latest 10 Projects
                // Console.WriteLine(GetLatestProjects(db));

                //12.Increase Salaries
                //Console.WriteLine(IncreaseSalaries(db));

                //13. Find Employees by First Name Starting With Sa
                //Console.WriteLine(GetEmployeesByFirstNameStartingWithSa(db));

                //14.Delete Project by Id
                //Console.WriteLine(DeleteProjectById(db));

                //15. Remove town
                Console.WriteLine(RemoveTown(db));
            }
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FirstName,
                    e.MiddleName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(e => e.EmployeeId)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine(string.Join(' ', emp.FirstName, emp.LastName, emp.MiddleName, emp.JobTitle, $"{emp.Salary:f2}"));
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.Salary
                })
                .Where(e => e.Salary > 50000)
                .OrderBy(e => e.FirstName)
                .ToList();



            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} - {emp.Salary:f2}");
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Department.Name == "Research and Development")
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    DeptName = e.Department.Name,
                    e.Salary
                })
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .ToList();




            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} from {emp.DeptName} - ${emp.Salary:f2}");
            }

            return sb.ToString().Trim();


        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var newAddress = new Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };


            var Nakov = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");
            Nakov.Address = newAddress;

            context.SaveChanges(); //!!!


            var empAddresses = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => new
                {
                    e.Address.AddressText
                })
                .ToList();


            var sb = new StringBuilder();

            foreach (var emp in empAddresses)
            {
                sb.AppendLine(emp.AddressText);
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var data = context.Employees
                .Where(e => e.EmployeesProjects.Any(x => x.Project.StartDate.Year >= 2001 && x.Project.StartDate.Year <= 2003))
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,

                    Projects = e.EmployeesProjects.Select(
                        p => new
                        {
                            Name = p.Project.Name,
                            StartDate = p.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),

                            EndDate = p.Project.EndDate.HasValue ?
                                            p.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture) :
                                            "not finished"
                        })
                })
                .Take(10)
                .ToList();


            var sb = new StringBuilder();

            foreach (var emp in data)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} - Manager: {emp.ManagerFirstName} {emp.ManagerLastName}");

                foreach (var prj in emp.Projects)
                {
                    sb.AppendLine($"--{prj.Name} - {prj.StartDate} - {prj.EndDate}");
                }
            }

            return sb.ToString().Trim();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                    .Select(a => new
                    {
                        a.AddressText,
                        TownName = a.Town.Name,
                        EmpCount = a.Employees.Count
                    })
                    .OrderByDescending(a => a.EmpCount)
                    .ThenBy(a => a.TownName)
                    .ThenBy(a => a.AddressText)
                    .Take(10);



            var sb = new StringBuilder();

            foreach (var addr in addresses)
            {
                sb.AppendLine($"{addr.AddressText}, {addr.TownName} - {addr.EmpCount} employees");
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var data = context.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects
                        .Select(x => x.Project.Name)
                        .OrderBy(p => p)
                        .ToList()
                }).SingleOrDefault(e => e.EmployeeId == 147);


            var sb = new StringBuilder();

            sb.AppendLine($"{data.FirstName} {data.LastName} - {data.JobTitle}");

            foreach (var project in data.Projects)
            {
                sb.AppendLine($"{project}");
            }


            return sb.ToString().Trim();

        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                    .Where(d => d.Employees.Count > 5)
                    .OrderBy(d => d.Employees.Count)
                    .ThenBy(d => d.Name)
                    .Select(d => new
                    {
                        DeptName = d.Name,
                        MngFirstName = d.Manager.FirstName,
                        MngLastName = d.Manager.LastName,
                        Employees = d.Employees
                            .Select(e => new
                            {
                                e.FirstName,
                                e.LastName,
                                e.JobTitle
                            })
                            .OrderBy(e => e.FirstName)
                            .ThenBy(e => e.LastName)
                            .ToList()
                    })
                    .ToList();



            var sb = new StringBuilder();

            foreach (var dept in departments)
            {
                sb.AppendLine($"{dept.DeptName} - {dept.MngFirstName} {dept.MngLastName}");

                foreach (var emp in dept.Employees)
                {
                    sb.AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle}");
                }
            }

            return sb.ToString().Trim();

        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .Select(p => new
                {
                    p.Name,
                    p.Description,
                    p.StartDate
                })
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .OrderBy(p => p.Name)
                .ToList();


            var sb = new StringBuilder();

            foreach (var project in projects)
            {
                sb.AppendLine($"{project.Name}");
                sb.AppendLine($"{project.Description}");
                sb.AppendLine(project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture));
            }

            return sb.ToString().Trim();


        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(p => p.Department.Name == "Engineering" ||
                    p.Department.Name == "Tool Design" ||
                    p.Department.Name == "Marketing" ||
                    p.Department.Name == "Information Services")
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToList();

            foreach (var emp in employees)
            {
                emp.Salary *= 1.12M;
            }

            context.SaveChanges();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} (${emp.Salary:F2})");


            }

            return sb.ToString().Trim();



        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.FirstName.StartsWith("SA") || e.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle} - (${emp.Salary:F2})");
            }

            return sb.ToString().Trim();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var employeesProjects = context.EmployeesProjects
                .Where(ep => ep.ProjectId == 2)
                .ToList();

            foreach (var ep in employeesProjects)
            {
                context.EmployeesProjects.Remove(ep);

            }


            var projects = context.Projects.Find(2);
            context.Projects.Remove(projects);

            context.SaveChanges();

            var projectData = context.Projects
                .Take(10)
                .Select(p => p.Name)
                .ToList();

            var sb = new StringBuilder();

            foreach (var project in projectData)
            {
                sb.AppendLine(project);
            }

            return sb.ToString().Trim();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Address.Town.Name == "Seattle");

            foreach (var emp in employees)
            {
                emp.AddressId = null;
            }

            var addresses = context.Addresses
                .Where(a => a.Town.Name == "Seattle");



            int count = 0;

            foreach (var address in addresses)
            {
                context.Addresses.Remove(address);
                count++;
            }

            var town = context.Towns.FirstOrDefault(t => t.Name == "Seattle");

            context.Towns.Remove(town);

            context.SaveChanges();


            return $"{count} addresses in Seattle were deleted";
        }
    }
}
