using Microsoft.AspNet.Identity;
using QuirkyBookRental.Models;
using QuirkyBookRental.Utility;
using QuirkyBookRental.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Net;

namespace QuirkyBookRental.Controllers
{
    [Authorize]
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
                var userInDb = db.Users.SingleOrDefault(u => u.Email == email);
                if (userInDb.RentalCount == 10)
                {
                    userInDb.RentalCount++;
                    rentalPr = rentalPr - (rentalPr * 20 / 100);
                }
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
        public ActionResult Index(int? pageNumber, string option=null,string search=null)
        {
            string userid = User.Identity.GetUserId();

            var model = from br in db.BookRental
                        join b in db.Books on br.BookId equals b.Id
                        join u in db.Users on br.UserId equals u.Id
                        select new BookRentalViewModel
                        {
                            Id=br.Id,
                            BookId = b.Id,
                            RentalPrice = br.RentalPrice,
                            Price = b.Price,
                            Pages = b.Pages,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            BirthDate = u.BirthDate,
                            ScheduledEndDate = br.ScheduledEndDate,
                            Author = b.Author,
                            Avaibility = b.Avaibility,
                            DateAdded = b.DateAdded,
                            Description = b.Description,
                            Email = u.Email,
                            StartDate = br.StartDate,
                            GenreId = b.GenreId,
                            Genre = db.Genres.Where(g => g.Id.Equals(b.GenreId)).FirstOrDefault(),
                            ISBN = b.ISBN,
                            ImageUrl = b.ImageUrl,
                            ProductDimensions = b.ProductDimensions,
                            PublicationDate = b.PublicationDate,
                            Publisher = b.Publisher,
                            RentalDuration = br.RentalDuration,
                            Status = br.Status.ToString(),
                            Title = b.Title,
                            UserId = u.Id
                            
                        };

            if(option=="email" && search.Length > 0)
            {
                model = model.Where(u => u.Email.Contains(search));
            }
            if (option == "name" && search.Length > 0)
            {
                model = model.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search));
            }
            if (option == "status" && search.Length > 0)
            {
                model = model.Where(u => u.Status.Contains(search));
            }

            if (!User.IsInRole(SD.AdminUserRole))
            {
                model = model.Where(u => u.UserId.Equals(userid));
            }

            return View(model.ToList().ToPagedList(pageNumber??1,5));
        }







        [HttpPost]
        public ActionResult Reserve(BookRentalViewModel book)
        {
            var userid = User.Identity.GetUserId();
            Book bookToRent = db.Books.Find(book.BookId);
            double rentalPr = 0;

            if (userid != null)
            {
                var chargeRate = from u in db.Users
                                 join m in db.MembershipTypes
                                 on u.MembershipTypeId equals m.Id
                                 where u.Id.Equals(userid)
                                 select new { m.ChargeRateOneMonth, m.ChargeRateSixMonth };
                if (book.RentalDuration == SD.SixMonthCount)
                {
                    rentalPr = Convert.ToDouble(bookToRent.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateSixMonth) / 100;
                }
                else
                {
                    rentalPr = Convert.ToDouble(bookToRent.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateOneMonth) / 100;
                }
                var userInDb = db.Users.SingleOrDefault(c => c.Id == userid);
                if (userInDb.RentalCount == 10)
                {
                    userInDb.RentalCount++;
                    rentalPr = rentalPr - (rentalPr * 20 / 100);
                }
                BookRent bookRent = new BookRent
                {
                    BookId = bookToRent.Id,
                    UserId = userid,
                    RentalDuration = book.RentalDuration,
                    RentalPrice = rentalPr,
                    Status = BookRent.StatusEnum.Requested,

                };

                db.BookRental.Add(bookRent);
                var bookInDb = db.Books.SingleOrDefault(c => c.Id == book.BookId);
                
                bookInDb.Avaibility -= 1;

                db.SaveChanges();
                return RedirectToAction("Index", "BookRent");
            }
            return View();
        }


        public ActionResult Details(int? id)
        {
            if(id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);
            if(model==null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        //Decline GET Method
        public ActionResult Decline(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Decline(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (ModelState.IsValid)
            {
                BookRent bookRent = db.BookRental.Find(model.Id);
                bookRent.Status = BookRent.StatusEnum.Rejected;
                var userInDb = db.Users.SingleOrDefault(c => c.Id == bookRent.UserId);
                if (userInDb.RentalCount == 11)
                {
                    userInDb.RentalCount--;
                }
                Book bookInDb = db.Books.Find(bookRent.BookId);
                bookInDb.Avaibility += 1;
                db.SaveChanges();
            }
                return RedirectToAction("Index");
        }

        //Approve GET Method
        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Approve",model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (ModelState.IsValid)
            {
                BookRent bookRent = db.BookRental.Find(model.Id);
                bookRent.Status = BookRent.StatusEnum.Approved;

                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //PickUp Get Method
        public ActionResult PickUp(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Approve", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PickUp(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (ModelState.IsValid)
            {
                BookRent bookRent = db.BookRental.Find(model.Id);
                bookRent.Status = BookRent.StatusEnum.Rented;
                bookRent.StartDate = DateTime.Now;
                if(bookRent.RentalDuration==SD.SixMonthCount)
                {
                    bookRent.ScheduledEndDate = DateTime.Now.AddMonths(Convert.ToInt32(SD.SixMonthCount));
                }
                else
                {
                    bookRent.ScheduledEndDate = DateTime.Now.AddMonths(Convert.ToInt32(SD.OneMonthCount));
                }

                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //Return Get Method
        public ActionResult Return(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Approve", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Return(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (ModelState.IsValid)
            {
                BookRent bookRent = db.BookRental.Find(model.Id);
                bookRent.Status = BookRent.StatusEnum.Closed;

                bookRent.AdditionalCharge = model.AdditionalCharge;
                Book bookInDb = db.Books.Find(bookRent.BookId);

                var userInDb = db.Users.Single(u => u.Id == bookRent.UserId);
                if (userInDb.RentalCount == 11)
                {
                    userInDb.RentalCount = 0;
                }
                else
                {
                    userInDb.RentalCount++;
                }

                bookInDb.Avaibility += 1;
                
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //Delete GET Method
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int Id)
        {
            if (Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (ModelState.IsValid)
            {
                BookRent bookRent = db.BookRental.Find(Id);
                db.BookRental.Remove(bookRent);

                var bookInDb = db.Books.Where(b => b.Id.Equals(bookRent.BookId)).FirstOrDefault();
                var userInDb = db.Users.SingleOrDefault(c => c.Id == bookRent.UserId);

                if (bookRent.Status.ToString().ToLower().Equals("rented"))
                {
                    bookInDb.Avaibility += 1;
                }
                else
                {
                    //This else would be called in all scenarios except when user has a book with them and never returns
                    if (userInDb.RentalCount == 11)
                    {
                        userInDb.RentalCount--;
                    }
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        private BookRentalViewModel getVMFromBookRent(BookRent bookRent)
        {
            Book bookSelected = db.Books.Where(b => b.Id == bookRent.BookId).FirstOrDefault();
            var userDetails = from u in db.Users
                              where u.Id.Equals(bookRent.UserId)
                              select new { u.Id, u.FirstName, u.LastName, u.BirthDate, u.Email };

            BookRentalViewModel model = new BookRentalViewModel
            {
                Id = bookRent.Id,
                BookId = bookSelected.Id,
                RentalPrice = bookRent.RentalPrice,
                Price = bookSelected.Price,
                Pages = bookSelected.Pages,
                FirstName = userDetails.ToList()[0].FirstName,
                LastName = userDetails.ToList()[0].LastName,
                BirthDate = userDetails.ToList()[0].BirthDate,
                ScheduledEndDate = bookRent.ScheduledEndDate,
                Author = bookSelected.Author,
                AdditionalCharge = bookRent.AdditionalCharge,
                StartDate = bookRent.StartDate,
                Avaibility = bookSelected.Avaibility,
                DateAdded = bookSelected.DateAdded,
                Description = bookSelected.Description,
                Email = userDetails.ToList()[0].Email,
                GenreId = bookSelected.GenreId,
                Genre = db.Genres.FirstOrDefault(g => g.Id.Equals(bookSelected.GenreId)),
                ISBN = bookSelected.ISBN,
                ImageUrl = bookSelected.ImageUrl,
                ProductDimensions = bookSelected.ProductDimensions,
                PublicationDate = bookSelected.PublicationDate,
                Publisher = bookSelected.Publisher,
                RentalDuration = bookRent.RentalDuration,
                Status = bookRent.Status.ToString(),
                Title=bookSelected.Title,
                UserId = userDetails.ToList()[0].Id
            };

            return model;
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