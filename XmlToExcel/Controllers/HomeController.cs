using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XmlToExcel.Models;

namespace XmlToExcel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        [Obsolete]
        private readonly IHostingEnvironment _hostingEnvironment;

        [Obsolete]
        public HomeController(IHostingEnvironment hostingEnvironment, ILogger<HomeController> logger)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
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

        //[HttpPost("FileUpload")]
        //[Obsolete]
        //public IActionResult Upload(IFormFile file)
        //{
        //    // Extract file name from whatever was posted by browser
        //    var fileName = GetUniqueFileName(System.IO.Path.GetFileName(file.FileName));

        //    var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

        //    // If file with same name exists delete it
        //    //if (System.IO.File.Exists(fileName))
        //    //{
        //    //    System.IO.File.Delete(fileName);
        //    //}           

        //    // Create new local file and copy contents of uploaded file
        //    using (var localFile = System.IO.File.OpenWrite(fileName))
        //    using (var uploadedFile = file.OpenReadStream())
        //    {
        //        uploadedFile.CopyTo(localFile);
        //    }

        //    ViewBag.Message = "File successfully uploaded";
        //    return RedirectToAction("Index");
        //}


        [HttpPost("FileUpload")]
        public IActionResult Upload(IFormFile file)
        {
            DataTable dt = new DataTable();
            if (file != null)
            {
                //Set Key Name
                string filename = GetUniqueFileName(file.FileName);
                //Get url To Save
                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/XML", filename);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                dt =  excelConvert(filename);
            }
            ViewBag.Message = "File successfully uploaded";
            return View(dt);
        }

        public DataTable excelConvert(string filename)
        {
            DataTable dtExcel = new DataTable();
            dtExcel.Columns.Add("DATETYPE");
            dtExcel.Columns.Add("FIELDNAME");
            dtExcel.Columns.Add("NAME");
            dtExcel.Columns.Add("TYPE");
            dtExcel.Columns.Add("TYPE_OF_OPERATION");


            string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/XML", filename);
            //XElement root = XElement.Load(SavePath);
            XDocument xdoc = XDocument.Load(SavePath);
            var folders = xdoc.Descendants("FOLDER");
            foreach (var folder in folders)
            {
                var parents = folder.Elements();
                foreach (var parent in parents)
                {
                    if (parent.Attribute("DATABASETYPE") != null && parent.Attribute("BUSINESSNAME") != null)
                    {
                        var typeOfOperation = parent.Name.LocalName;
                        var dataType = parent.Attribute("DATABASETYPE").Value;
                        var fieldName = parent.Attribute("BUSINESSNAME").Value;
                        var name = parent.Attribute("NAME").Value;
                        var type = parent.Attribute("DESCRIPTION").Value;
                        dtExcel.Rows.Add(dataType, fieldName, name, type, typeOfOperation);
                    }
                    var childrens = parent.Elements();
                    foreach (var children in childrens)
                    {
                        if (children.Attribute("DATATYPE") != null && children.Attribute("BUSINESSNAME") != null)
                        {
                            var dataTypeChild = children.Attribute("DATATYPE").Value;
                            var fieldNameChild = children.Attribute("BUSINESSNAME").Value;
                            var nameChild = children.Attribute("NAME").Value;
                            var typeChild = children.Attribute("DESCRIPTION").Value;
                            var typeOfOperationChild = children.Name.LocalName;
                            dtExcel.Rows.Add(dataTypeChild, fieldNameChild, nameChild, typeChild, typeOfOperationChild);
                        }
                    }
                }
            }
            var cnt = dtExcel.Rows.Count;
            return dtExcel;
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
    }
}
