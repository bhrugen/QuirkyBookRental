using QuirkyBookRental.Models;
using QuirkyBookRental.Utility;
using QuirkyBookRental.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuirkyBookRental.Controllers
{
    public class BookRentController : Controller
    {
        private ApplicationDbContext db;

        public BookRentController()
        {
            db = ApplicationDbContext.Create();
        }

        //Get Method
        public ActionResult Create(string title = null, string ISBN = null)
        {
            if(title!=null && ISBN!=null)
            {
                BookRentalViewModel model = new BookRentalViewModel
                {
                    Title = title,
                    ISBN = ISBN
                };
                return View(model);
            }
            return View(new BookRentalViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookRentalViewModel bookRent)
        {
            if(ModelState.IsValid)
            {
                var email = bookRent.Email;

                var userDetails = from u in db.Users
                                  where u.Email.Equals(email)
                                  select new { u.Id, u.FirstName, u.LastName, u.BirthDate };

                var ISBN = bookRent.ISBN;

                Book bookSelected = db.Books.Where(b => b.ISBN == ISBN).FirstOrDefault();

                var rentalDuration = bookRent.RentalDuration;

                var chargeRate = from u in db.Users
                                 join m in db.MembershipTypes
                                 on u.MembershipTypeId equals m.Id
                                 where u.Email.Equals(email)
                                 select new { m.ChargeRateOneMonth, m.ChargeRateSixMonth };

                var oneMonthRental = Convert.ToDouble(bookSelected.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateOneMonth) / 100;
                var sixMonthRental = Convert.ToDouble(bookSelected.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateSixMonth) / 100;

                double rentalPr = 0;

                if(bookRent.RentalDuration==SD.SixMonthCount)
                {
                    rentalPr = sixMonthRental;
                }
                else
                {
                    rentalPr = oneMonthRental;
                }

                BookRentalViewModel model = new BookRentalViewModel
                {
                    BookId = bookSelected.Id,
                    RentalPrice = rentalPr,
                    Price = bookSelected.Price,
                    Pages = bookSelected.Pages,
                    FirstName = userDetails.ToList()[0].FirstName,
                    LastName = userDetails.ToList()[0].LastName,
                    BirthDate = userDetails.ToList()[0].BirthDate,
                    ScheduledEndDate = bookRent.ScheduledEndDate,
                    Author = bookSelected.Author,
                    Avaibility = bookSelected.Avaibility,
                    DateAdded = bookSelected.DateAdded,
                    Description = bookSelected.Description,
                    Email = email,
                    GenreId = bookRent.GenreId,
                    Genre = db.Genres.Where(g => g.Id.Equals(bookSelected.GenreId)).First(),
                    ISBN = bookSelected.ISBN,
                    ImageUrl = bookSelected.ImageUrl,
                    ProductDimensions = bookSelected.ProductDimensions,
                    PublicationDate = bookSelected.PublicationDate,
                    Publisher = bookSelected.Publisher,
                    RentalDuration = bookRent.RentalDuration,
                    Status = BookRent.StatusEnum.Requested.ToString(),
                    Title = bookSelected.Title,
                    UserId = userDetails.ToList()[0].Id,
                    RentalPriceOneMonth = oneMonthRental,
                    RentalPriceSixMonth = sixMonthRental
                };

                BookRent modelToAddToDb = new BookRent
                {
                    BookId = bookSelected.Id,
                    RentalPrice = rentalPr,
                    ScheduledEndDate = bookRent.ScheduledEndDate,
                    RentalDuration = bookRent.RentalDuration,
                    Status = BookRent.StatusEnum.Approved,
                    UserId = userDetails.ToList()[0].Id
                };

                bookSelected.Avaibility -= 1;
                db.BookRental.Add(modelToAddToDb);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: BookRent
        public ActionResult Index()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
            }
        }
    }
}