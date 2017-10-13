using QuirkyBookRental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuirkyBookRental.Controllers
{
    public class GenreController : Controller
    {
        private ApplicationDbContext db;

        public GenreController()
        {
            db = new ApplicationDbContext();
        }

        
        // GET: Genre
        public ActionResult Index()
        {
            return View(db.Genres.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
        }
    }
}