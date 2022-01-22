namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners
                .Where(p => ids.Contains(p.Id))
                .ToList()
                .Select(p => new ExportPrisonerDto
                {
                    Id = p.Id,
                    Name = p.FullName,
                    CellNumber = p.Cell.CellNumber,

                    Officers = p.PrisonerOfficers.Select(po => new ExportOfficerDto()
                    {
                        OfficerName = po.Officer.FullName,
                        Department = po.Officer.Department.Name

                    })
                    .OrderBy(o => o.OfficerName)
                    .ToList(),

                    TotalOfficerSalary = p.PrisonerOfficers.Sum(po => po.Officer.Salary)
                })
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToList();



            var result = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return result;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var prisoners = context.Prisoners
                .Where(p => prisonersNames.Contains(p.FullName))
                .ToList()
                .Select(p => new ExportXmlPrisoner()
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),

                    EncryptedMessages = p.Mails
                        .Select(m => new ExportMessagesDto()
                        {
                            Description = new string(m.Description.ToCharArray().Reverse().ToArray())

                        }).ToList()

                }).OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToList();



            var xml = XmlConverter.Serialize(prisoners, "Prisoners");

            return xml;


            //var sb = new StringBuilder();

            //var namespaces = new XmlSerializerNamespaces();
            //namespaces.Add(string.Empty, string.Empty);


            //var XmlSerializer = new XmlSerializer(typeof(List<ExportXmlPrisoner>), new XmlRootAttribute("Prisoners"));
            //XmlSerializer.Serialize(new StringWriter(sb), prisoners, namespaces);

            //return sb.ToString().Trim();
        }
    }
}