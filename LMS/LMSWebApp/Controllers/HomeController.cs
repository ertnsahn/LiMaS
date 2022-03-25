using LMSWebApp.Data;
using LMSWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace LMSWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GenericRepository<BookCheckoutHistory> bookHistoryRepository;
        private readonly ApplicationDbContext applicationDbContext;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDbContext)
        {
            _logger = logger;
            this.applicationDbContext = applicationDbContext;
            bookHistoryRepository = new GenericRepository<BookCheckoutHistory>(applicationDbContext);
        }

        public IActionResult Index()
        {
            var totalBooks = applicationDbContext.Books.Count();
            ViewBag.TotalBooks = totalBooks;

            var totalCheckoutBooks = applicationDbContext.Books.Count(b => b.Status == "Checkout");
            ViewBag.TotalCheckoutBooks = totalCheckoutBooks;

            var totalLateBooks = applicationDbContext.CheckoutHistories.Count(b => b.ReturnDate < DateTime.Now);
            ViewBag.TotalLateBooks = totalLateBooks;

            var totalPenality = applicationDbContext.CheckoutHistories.Sum(b => b.Penality);
            ViewBag.TotalPenality = totalPenality;
            return View();
        }

        private string GetMonthName(int month)
        {
            try
            {
                string[] monthNames = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
                return monthNames[month];
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [HttpGet]
        public IActionResult GetChartData(string start, string end)
        {
            List<string> monthNames;

            DateTime previousDate = DateTime.Parse(start);
            DateTime endDate = DateTime.Parse(end);

            GetMonthsBetweenDates(previousDate, endDate, out monthNames, "MMMM");

            var checkOutBooks = bookHistoryRepository.Get(b => b.CheckoutDate >= previousDate && b.CheckoutDate <= endDate);
            var checkInBooks = bookHistoryRepository.Get(b => b.ReturnDate >= previousDate && b.ReturnDate <= endDate);

            var checkOutBooksData = checkOutBooks.GroupBy(x => x.CheckoutDate.Value.Month).Select(month => new ChartModel
            {
                MonthName = GetMonthName(month.Key - 1),
                DataCount = month.Count()
            }).ToList();

            var checkInBooksData = checkInBooks.GroupBy(x => x.ReturnDate.Value.Month).Select(month => new ChartModel
            {
                MonthName = GetMonthName(month.Key - 1),
                DataCount = month.Count()
            }).ToList();

            var listCheckOutData = new List<ChartModel>();
            var listCheckinData = new List<ChartModel>();
            foreach (var monthName in monthNames)
            {
                var checkOutData = checkOutBooksData.FirstOrDefault(x => x.MonthName == monthName);
                if (checkOutData == null)
                {
                    listCheckOutData.Add(new ChartModel
                    {
                        MonthName = monthName,
                        DataCount = 0
                    });
                }
                else
                {
                    listCheckOutData.Add(checkOutData);
                }


                var checkinData = checkInBooksData.FirstOrDefault(x => x.MonthName == monthName);
                if (checkinData == null)
                {
                    listCheckinData.Add(new ChartModel
                    {
                        MonthName = monthName,
                        DataCount = 0
                    });
                }
                else
                {
                    listCheckinData.Add(checkinData);
                }
            }

            var statsDates = $"Statistics:{previousDate:D} - {endDate:D}";

            return Json(new
            {
                listCheckOutData,
                listCheckinData,
                monthNames,
                statsDates
            });
        }



        public static List<int> GetMonthsBetweenDates(DateTime start, DateTime end, out List<string> monthNames, string format = "")
        {
            monthNames = new List<string>();
            end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));

            var diff = Enumerable.Range(0, int.MaxValue)
                .Select(start.AddMonths)
                .TakeWhile(e => e <= end)
                .Select(e => e.Month).ToList();

            format = string.IsNullOrEmpty(format) ? "MMM" : format;
            monthNames = Enumerable.Range(0, int.MaxValue)
                .Select(start.AddMonths)
                .TakeWhile(e => e <= end)
                .Select(e => e.ToString(format)).ToList();

            return diff;
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

    public class ChartModel
    {
        public string MonthName { get; set; }
        public int DataCount { get; set; }
    }
}
