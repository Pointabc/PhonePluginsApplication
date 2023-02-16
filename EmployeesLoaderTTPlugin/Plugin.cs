using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Xml.Linq;

namespace EmployeesLoaderTTPlugin
{
    [Author(Name = "Sergey Volkov")]
    public class Plugin : IPluggable
    {
        public class SearchResult
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string phone { get; set; }
        }
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Loading employees from .\\users.json");

            var employeesList = args.Cast<EmployeesDTO>().ToList();
            if (!File.Exists("users.json"))
            {
                logger.Info("File users.json is not exist.");
                return employeesList.Cast<DataTransferObject>();
            }

            // Load employees from json file
            string employeesSearchText = File.ReadAllText("users.json");
            JObject employeesSearch = JObject.Parse(employeesSearchText);

            // get JSON result objects into a list
            IList<JToken> results = employeesSearch["users"].Children().ToList();

            // serialize JSON results into .NET objects
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                SearchResult searchResult = result.ToObject<SearchResult>();
                
                EmployeesDTO employeeDTO = new EmployeesDTO();
                employeeDTO.Name = searchResult.firstName + " " + searchResult.lastName;
                employeeDTO.AddPhone(searchResult.phone);
                employeeDTO.ToJson();
                employeesList.Add(employeeDTO);
            }
            return employeesList.Cast<DataTransferObject>();
        }
    }
}
