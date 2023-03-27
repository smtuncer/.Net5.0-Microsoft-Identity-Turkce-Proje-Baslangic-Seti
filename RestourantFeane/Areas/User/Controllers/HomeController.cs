using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestourantFeane.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RestourantFeane.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
