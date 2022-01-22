namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        //messages

        private const string ErrorMessage = "Invalid Data";

        private const string SuccessfullyImportedDepartment
            = "Imported {0} with {1} cells";

        private const string SuccessfullyImportedPrisoners
            = "Imported {0} {1} years old";

        private const string SuccessfullyImportedOfficers
           = "Imported {0} ({1} prisoners)";

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var deptsDtos = JsonConvert.DeserializeObject<List<ImportDeptDto>>(jsonString);

            var depts = new List<Department>();

            var sb = new StringBuilder();


            foreach (var deptDto in deptsDtos)
            {
                //check if valid via attributes
                if (!IsValid(deptDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;

                }


                if (deptDto.Cells.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //check cells

                if (deptDto.Cells.Any(c => c.CellNumber < 1 || c.CellNumber > 1000))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                var currDept = new Department()
                {
                    Name = deptDto.Name,
                    Cells = deptDto.Cells.Select(c => new Cell()
                    {
                        CellNumber = c.CellNumber,
                        HasWindow = c.HasWindow,

                    }).ToList()
                };

                depts.Add(currDept);
                sb.AppendLine(String.Format(SuccessfullyImportedDepartment,
                    currDept.Name, currDept.Cells.Count));

            }

            context.Departments.AddRange(depts);
            context.SaveChanges();

            return sb.ToString().Trim();

        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonersDtos = JsonConvert.DeserializeObject<List<ImportPrisonerDto>>(jsonString);

            var prisoners = new List<Prisoner>();

            var sb = new StringBuilder();


            foreach (var prisonerDto in prisonersDtos)
            {
                //check if valid via attributes
                if (!IsValid(prisonerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //check dates

                DateTime incarcerationDate;
                bool isValidIncDate = DateTime.TryParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out incarcerationDate);

                if (!isValidIncDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                //DateTime? releaseDate = null;

                //if (!string.IsNullOrEmpty(prisonerDto.ReleaseDate))
                //{
                //    DateTime relDateValue;
                //    bool isValidRelDate = DateTime.TryParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy",
                //                        CultureInfo.InvariantCulture, DateTimeStyles.None, out relDateValue);

                //    if (!isValidRelDate)
                //    {
                //        sb.AppendLine(ErrorMessage);
                //        continue;
                //    }

                //    releaseDate = relDateValue;
                //}



                DateTime relDate;

                bool isValidRelDate = DateTime.TryParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out  relDate);

                if (!isValidRelDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }



                //check mails

                if (prisonerDto.Mails.Any(m => !IsValid(m)))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                var currPrisoner = new Prisoner()
                {
                    FullName = prisonerDto.FullName,
                    Nickname = prisonerDto.Nickname,
                    Age = prisonerDto.Age,

                    IncarcerationDate = incarcerationDate,
                    ReleaseDate = isValidRelDate ? (DateTime?)relDate : null,
                    Bail = prisonerDto.Bail,
                    CellId = prisonerDto.CellId,
                    Mails = prisonerDto.Mails.Select(m => new Mail()
                    {
                        Description = m.Description,
                        Sender = m.Sender,
                        Address = m.Address
                    }).ToList()
                };



                prisoners.Add(currPrisoner);
                sb.AppendLine(String.Format(SuccessfullyImportedPrisoners,
                    currPrisoner.FullName, currPrisoner.Age));
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {

            //var XmlSerializer = new XmlSerializer(typeof(List<ImportXmlOfficerDto>), new XmlRootAttribute("Officers"));

            //using (var reader = new StringReader(xmlString))
            //{
            //    var officersDtos = (List<ImportXmlOfficerDto>)XmlSerializer.Deserialize(reader);

            //    var officers = new List<Officer>();
            //    var sb = new StringBuilder();

            //    foreach (var officerDto in officersDtos)
            //    {
            //        if (!IsValid(officerDto))
            //        {
            //            sb.AppendLine(ErrorMessage);
            //            continue;
            //        }

            //        //check position

            //        object positionType;

            //        bool isValidPosition = Enum.TryParse(typeof(Position), officerDto.Position, out positionType);

            //        if (!isValidPosition)
            //        {
            //            sb.AppendLine(ErrorMessage);
            //            continue;
            //        }

            //        //check weapon

            //        object weaponType;

            //        bool isValidWeapon = Enum.TryParse(typeof(Weapon), officerDto.Weapon, out weaponType);

            //        if (!isValidWeapon)
            //        {
            //            sb.AppendLine(ErrorMessage);
            //            continue;
            //        }

            //        //create new officer

            //        var currOfficer = new Officer()
            //        {
            //            FullName = officerDto.FullName,
            //            Salary = officerDto.Salary,
            //            Position = (Position)positionType,
            //            Weapon = (Weapon)weaponType,
            //            DepartmentId = officerDto.DepartmentId,
            //        };

            //        //create officersPrisoners

            //        var officersPrisoners = officerDto.Prisoners
            //            .Select(p => new OfficerPrisoner()
            //            {
            //                Officer = currOfficer,
            //                PrisonerId = p.Id
            //            }).ToList();

            //        currOfficer.OfficerPrisoners = officersPrisoners;

            //        officers.Add(currOfficer);

            //        sb.AppendLine(String.Format(SuccessfullyImportedOfficers,
            //            currOfficer.FullName, currOfficer.OfficerPrisoners.Count));

            //    }

            //    context.Officers.AddRange(officers);
            //    context.SaveChanges();

            //    return sb.ToString().Trim();
            //}



            var officers = new List<Officer>();
            var sb = new StringBuilder();

            var officersDtos = XmlConverter.Deserializer<ImportXmlOfficerDto>(xmlString, "Officers");

            foreach (var officerDto in officersDtos)
            {
                bool isValidPosition = Enum.TryParse(typeof(Position), officerDto.Position, out var positionType);

                bool isValidWeapon = Enum.TryParse(typeof(Weapon), officerDto.Weapon, out var weaponType);

                if (!IsValid(officerDto) ||
                    !isValidPosition ||
                    !isValidWeapon)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                //create new officer

                var currOfficer = new Officer()
                {
                    FullName = officerDto.FullName,
                    Salary = officerDto.Salary,
                    Position = (Position)positionType,
                    Weapon = (Weapon)weaponType,
                    DepartmentId = officerDto.DepartmentId,
                };

                //create officersPrisoners

                var officersPrisoners = officerDto.Prisoners
                    .Select(p => new OfficerPrisoner()
                    {
                        Officer = currOfficer,
                        PrisonerId = p.Id
                    }).ToList();

                currOfficer.OfficerPrisoners = officersPrisoners;

                officers.Add(currOfficer);

                sb.AppendLine(String.Format(SuccessfullyImportedOfficers,
                    currOfficer.FullName, currOfficer.OfficerPrisoners.Count));

            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return sb.ToString().Trim();

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