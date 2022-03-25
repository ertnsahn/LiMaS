using LMSWebApp.Data;
using LMSWebApp.Enum;
using LMSWebApp.Helpers;
using LMSWebApp.Models;
using LMSWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LMSWebApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly GenericRepository<Book> bookRepository;
        private readonly GenericRepository<BookCheckoutHistory> checkOutRepository;

        private readonly LogHelper logHelper;

        private readonly ApplicationDbContext applicationDbContext;
        public BooksController(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
            bookRepository = new GenericRepository<Book>(applicationDbContext);
            checkOutRepository = new GenericRepository<BookCheckoutHistory>(applicationDbContext);
            this.logHelper = new LogHelper(applicationDbContext);
        }

        public IActionResult Index()
        {
            return View();
        }

        private readonly Dictionary<int?, Expression<Func<Book, object>>> orderByEnum =
           new Dictionary<int?, Expression<Func<Book, object>>> {
              {1, s=> s.BookTitle},
              {2, s=> s.ISBN},
              {3, s=> s.PublishYear},
              {4, s=> s.CoverPrice},
              {5, s=> s.Status}
           };
        private readonly Dictionary<int?, Expression<Func<BookCheckoutHistory, object>>> orderByEnumHistory =
          new Dictionary<int?, Expression<Func<BookCheckoutHistory, object>>> {
              {1, s=> s.MobileNumber},
              {2, s=> s.NationalID},
              {3, s=> s.Penality},
              {4, s=> s.PersonName},
              {5, s=> s.ReturnDate},
              {6, s=> s.ActualReturnDate},
              {7, s=> s.ReturnDate},
              {8, s=> s.CheckoutDate},
              {9, s=> s.Currency},
          };

        public ActionResult History(int id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        public JsonResult GetHistory([FromBody] HistoryDatatableSearchModel searchModel)
       {
            try
            {
                Expression<Func<BookCheckoutHistory, bool>> filter = b => b.BookId == searchModel.BookId;
                var orderBy = orderByEnumHistory[searchModel.SortByColumn];
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    filter = b => b.BookId == searchModel.BookId && 
                    (b.PersonName.ToLower().Contains(searchModel.SearchTerm.ToLower()) || 
                    b.NationalID.ToLower().Contains(searchModel.SearchTerm.ToLower()) ||
                    b.MobileNumber.ToLower().Contains(searchModel.SearchTerm.ToLower())
                    );
                }
                var tableResponse = searchModel.SortDir == SortDir.DESC ?
                          checkOutRepository.GetPaginationData(searchModel, filter, c => c.OrderByDescending(orderBy)) :
                          checkOutRepository.GetPaginationData(searchModel, filter, c => c.OrderBy(orderBy));
                return Json(tableResponse);
            }
            catch (Exception ex)
            {
                logHelper.LogError(ex, MethodBase.GetCurrentMethod());
                return Json(new DataTableResponse<Book>());
            }
        }

        [HttpPost]
        public JsonResult Index([FromBody] BaseSearchModel searchModel)
        {
            try
            {
                Expression<Func<Book, bool>> filter = null;
                var orderBy = orderByEnum[searchModel.SortByColumn];
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    filter = b => b.BookTitle.Contains(searchModel.SearchTerm) || b.ISBN.Contains(searchModel.SearchTerm) ||
                            b.PublishYear.ToString().Contains(searchModel.SearchTerm) || b.CoverPrice.ToString().Contains(searchModel.SearchTerm) ||
                            b.Status.Contains(searchModel.SearchTerm);
                }
                var tableResponse = searchModel.SortDir == SortDir.DESC ?
                          bookRepository.GetPaginationData(searchModel, filter, c => c.OrderByDescending(orderBy)) :
                          bookRepository.GetPaginationData(searchModel, filter, c => c.OrderBy(orderBy));
                return Json(tableResponse);
            }
            catch (Exception ex)
            {
                logHelper.LogError(ex, MethodBase.GetCurrentMethod());
                return Json(new DataTableResponse<Book>());
            }
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Book book)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(book);
                }
                else
                {
                    applicationDbContext.Books.Add(book);
                    applicationDbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (System.Exception ex)
            {
                logHelper.LogError(ex, MethodBase.GetCurrentMethod());
                return View();
            }
        }


        public IActionResult Edit(int id = 0)
        {
            if (id > 0)
            {
                var book = applicationDbContext.Books.FirstOrDefault(i => i.Id == id);
                return View(book);
            }
            return View();
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var book = bookRepository.GetById(id);
                if(book.Status == BookStatusEnum.CheckOut.ToString())
                {
                    return Json(false);
                }
                bookRepository.Delete(id);
                bookRepository.SaveChanges();
                return Json(true);
            }
            catch (System.Exception ex)
            {
                logHelper.LogError(ex, MethodBase.GetCurrentMethod());
                return Json(false);
            }
        }

        [HttpPost]
        public IActionResult Edit(Book book)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(book);
                }
                else
                {
                    applicationDbContext.Books.Update(book);
                    applicationDbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (System.Exception ex)
            {
                logHelper.LogError(ex, MethodBase.GetCurrentMethod());
                return View();
            }
        }

        public IActionResult Checkin(int id)
        {
            var checkout = checkOutRepository.Get(i => i.BookId == id && i.ActualReturnDate == null).OrderByDescending(i=>i.CheckoutDate.Value).First();
            var model = new CheckInModel
            {
                Id = checkout.Id,
                BookId = checkout.BookId,
                NationalID = checkout.NationalID,
                CheckoutDate = checkout.CheckoutDate,
                MobileNumber = checkout.MobileNumber,
                PersonName = checkout.PersonName,
                Penality = checkout.Penality,
                ActualReturnDate = checkout.ActualReturnDate,
                Currency = checkout.Currency,
                ReturnDate = checkout.ReturnDate
            };
            return View(model);
        }


        [HttpPost]
        public IActionResult Checkin(CheckInModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                else
                {
                    var checkoutDb = checkOutRepository.GetFirstOrDefault(i => i.Id == model.Id);

                    var checkout = new BookCheckoutHistory
                    {
                        BookId = checkoutDb.BookId,
                        NationalID = checkoutDb.NationalID,
                        CheckoutDate = checkoutDb.CheckoutDate + DateTime.Now.TimeOfDay,
                        MobileNumber = checkoutDb.MobileNumber,
                        PersonName = checkoutDb.PersonName,
                        ReturnDate = checkoutDb.ReturnDate + DateTime.Now.TimeOfDay,
                        ActualReturnDate = model.ActualReturnDate + DateTime.Now.TimeOfDay,
                        Penality = model.Penality,
                        Currency = model.Currency
                    };


                    checkOutRepository.Insert(checkout);
                    checkOutRepository.SaveChanges();

                    var book = bookRepository.GetByIdAsNoTracking(i => i.Id == checkout.BookId);
                    book.Status = null;
                    bookRepository.Update(book);
                    bookRepository.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                logHelper.LogError(ex, MethodBase.GetCurrentMethod());
                return View();
            }
        }

        public IActionResult Checkout(int id)
        {
            var model = new CheckOutModel();
            model.BookId = id;
            return View(model);
        }

        [HttpPost]
        public IActionResult Checkout(CheckOutModel model)
        {
            try
            {
                ViewBag.Books = bookRepository.Get();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                else
                {
                    var checkout = new BookCheckoutHistory
                    {
                        BookId = model.BookId,
                        NationalID = model.NationalID,
                        CheckoutDate = model.CheckoutDate + DateTime.Now.TimeOfDay,
                        MobileNumber = model.MobileNumber,
                        PersonName = model.PersonName,
                        ReturnDate = model.ReturnDate + DateTime.Now.TimeOfDay
                    };

                    checkOutRepository.Insert(checkout);
                    checkOutRepository.SaveChanges();

                    var book = bookRepository.GetByIdAsNoTracking(i => i.Id == checkout.BookId);
                    book.Status = BookStatusEnum.CheckOut.ToString();
                    bookRepository.Update(book);
                    bookRepository.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                logHelper.LogError(ex, MethodBase.GetCurrentMethod());
                return View();
            }
        }

        public IActionResult Details(int? id)
        {
            var checkout = checkOutRepository.GetFirstOrDefault(i => i.BookId == id);
            var model = new CheckInModel
            {
                Id = checkout.Id,
                BookId = checkout.BookId,
                NationalID = checkout.NationalID,
                CheckoutDate = checkout.CheckoutDate,
                MobileNumber = checkout.MobileNumber,
                PersonName = checkout.PersonName,
                Penality = checkout.Penality,
                ActualReturnDate = checkout.ActualReturnDate,
                Currency = checkout.Currency,
                ReturnDate = checkout.ReturnDate
            };
            return View(model);
        }
    }
}
