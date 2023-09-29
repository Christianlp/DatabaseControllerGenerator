using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Database_Controller_Generator.Models;
using System.Text;

namespace Database_Controller_Generator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoadDatabaseTablestoBrowser(Models.DatabaseRequest dbRequest) 
        {
            DatabaseController db = new DatabaseController(dbRequest);
            ControllerCreator creator = new ControllerCreator();
            DatabaseObjects DBObjects = new DatabaseObjects();
            ResultObjects Results = new ResultObjects();
            DBObjects.Database = dbRequest.Database;
            DBObjects.Hostname = dbRequest.Hostname;
            DBObjects.Schemas = db.GetDatabaseSchemas();
            foreach (DatabaseSchema schema in DBObjects.Schemas)
            {
                schema.Tables = db.GetDatabaseTables(schema.SchemaID);
                foreach (DatabaseTable table in schema.Tables)
                {
                    table.Columns = db.GetDatabaseColumns(table.TableID);
                }
            }
            Results.DatabaseObjects = DBObjects;
            string fileContents = creator.CreateDatabaseController(DBObjects);
            Results.FileContents = fileContents;
            return View("Results", Results);
        }

        [HttpPost]
        public IActionResult LoadDatabaseTablestoFile(Models.DatabaseRequest dbRequest) {
            DatabaseController db = new DatabaseController(dbRequest);
            ControllerCreator creator = new ControllerCreator();
            DatabaseObjects DBObjects = new DatabaseObjects();
            DBObjects.Database = dbRequest.Database;
            DBObjects.Hostname = dbRequest.Hostname;
            DBObjects.Schemas = db.GetDatabaseSchemas();
            foreach (DatabaseSchema schema in DBObjects.Schemas)
            {
                schema.Tables = db.GetDatabaseTables(schema.SchemaID);
                foreach (DatabaseTable table in schema.Tables)
                {
                    table.Columns = db.GetDatabaseColumns(table.TableID);
                }
            }
            string fileContents = creator.CreateDatabaseController(DBObjects);
            return File(Encoding.UTF8.GetBytes(fileContents), "text/plain", "DatabaseController.cs");
        } 

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
