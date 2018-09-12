using InsuranceQuotes.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceQuotes.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CalcRate(string firstName, string lastName, string email, string dob, string make1, string model1, string carYear1, string coverage1, string make2, string model2, string carYear2, string coverage2, string make3, string model3, string carYear3, string coverage3, int ticketToggle, string ticket1, string ticket2, string ticket3, string ticket4, string ticket5, int duiToggle, string dui)
        {
            //convert all year inputs to ints
            bool Year1Check, Year2Check, Year3Check;
            Year1Check = int.TryParse(carYear1, out int Year1Value);
            Year2Check = int.TryParse(carYear2, out int Year2Value);
            Year3Check = int.TryParse(carYear3, out int Year3Value);

            Year1Value = (Year1Check) ? Year1Value : DateTime.Now.Year;
            Year2Value = (Year2Check) ? Year2Value : DateTime.Now.Year;
            Year3Value = (Year3Check) ? Year3Value : DateTime.Now.Year;

            //convert date of birth data
            bool DobCheck;
            DobCheck = DateTime.TryParse(dob, out DateTime DateOfBirth);
            //var age = DateTime.Now.Year - DateOfBirth.Year;
            var daysalive = DateTime.Now - DateOfBirth;
            int age = daysalive.Days / 365;

            //convert ticket input
            bool[] TicketCheck = new bool[5];
            DateTime[] Ticket = new DateTime[5];
            TicketCheck[0] = DateTime.TryParse(ticket1, out Ticket[0]);
            TicketCheck[1] = DateTime.TryParse(ticket2, out Ticket[1]);
            TicketCheck[2] = DateTime.TryParse(ticket3, out Ticket[2]);
            TicketCheck[3] = DateTime.TryParse(ticket4, out Ticket[3]);
            TicketCheck[4] = DateTime.TryParse(ticket5, out Ticket[4]);

            //convert dui input
            bool DuiCheck;
            DuiCheck = DateTime.TryParse(dui, out DateTime Dui);

            int DuiActive = 0;
            if (duiToggle == 1)
            {
                Dui = (DuiCheck) ? Dui : DateTime.Now;
                DuiActive = (DateTime.Now - Dui).Days < 3650 ? 1 : 0;
            }

            using (InsuranceEntities db = new InsuranceEntities())
            {
                var Users = db.Users;
                var FoundUsers = Users.Where(x => x.Email == email).ToList().Count;

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(dob) || string.IsNullOrEmpty(make1))
                {
                    return View("FormError");
                }
                // checks if email/account is already in system
                else if (FoundUsers > 0)
                {
                    return View("AccountError");
                }
                // check for valid car year inputs
                else if ((Year1Check == true && (Year1Value < 1900 || Year1Value > DateTime.Now.Year + 1)) || (Year2Check == true && (Year2Value < 1900 || Year2Value > DateTime.Now.Year + 1)) || (Year3Check == true && (Year3Value < 1900 || Year3Value > DateTime.Now.Year + 1)))
                {
                    return View("YearError");
                }
                // checks if user entered valid age
                else if (age > 130 || age < 14)
                {
                    return View("AgeError");
                }
                else
                {
                    //create user and add to db
                    var tempUser = new User();
                    tempUser.FirstName = firstName.ToLower();
                    tempUser.LastName = lastName.ToLower();
                    tempUser.Email = email.ToLower();
                    tempUser.DateOfBirth = DateOfBirth;
                    tempUser.DuiActive = DuiActive;
                    if (duiToggle == 1)
                    {
                        tempUser.DuiIssued = (DuiCheck) ? Dui : DateTime.Now;
                    }
                    else tempUser.DuiIssued = null;

                    db.Users.Add(tempUser);
                    db.SaveChanges();

                    //create all tickets and add to db
                    for (int i = 0; i < ticketToggle; i++)
                    {
                        var tempTicket = new Ticket();
                        tempTicket.IssueDate = (TicketCheck[i]) ? Ticket[i] : DateTime.Now;
                        tempTicket.UserId = tempUser.Id;
                        db.Tickets.Add(tempTicket);
                    }
                    db.SaveChanges();

                    //Create quotes and cars and add to db
                    AddQuoteAndCar(tempUser, make1, model1, Year1Value, coverage1);
                    if ((make2 != null) && (make2 != "")) AddQuoteAndCar(tempUser, make2, model2, Year2Value, coverage2);
                    if ((make3 != null) && (make3 != "")) AddQuoteAndCar(tempUser, make3, model3, Year3Value, coverage3);

                    var cars = from c in db.Cars
                               where c.UserId == tempUser.Id
                               select c;

                    decimal totalrate = 0.0m;
                    foreach (var car in cars)
                    {
                        var quote = from q in db.Quotes
                                    where q.Id == car.QuoteId
                                    select q;

                        totalrate += (decimal)(quote.FirstOrDefault().Rate);
                    }

                    //Add total quote column for user and save to db
                    if (cars.Count() == 1) tempUser.CompleteQuote = totalrate;
                    else tempUser.CompleteQuote = totalrate * (decimal)(1.00 - (.05 * (cars.Count() - 1)));
                    db.SaveChanges();

                    //return Content("Test " + age + " " + " " + coverage1 + " " + cars.Count() + " " + totalrate + " " + tempUser.CompleteQuote);

                    //Display Policy Details
                    return RedirectToAction("LookupRate", new
                    {
                        lookupEmail = tempUser.Email
                    });
                }
            }
        }

        [HttpGet]
        public ActionResult LookupRate(string lookupEmail)
        {
            if (string.IsNullOrEmpty(lookupEmail))
            {
                return View("~/Views/Shared/Error.cshtml");
            }
            else
            {
                using (InsuranceEntities db = new InsuranceEntities())
                {
                    var Users = db.Users;
                    var FoundUsers = Users.Where(x => x.Email == lookupEmail).ToList();
                    if (FoundUsers.Count() < 1)
                    {
                        return View("NoAccountError");
                    }
                    else
                    {
                        var userInfo = new LookupViewModel();

                        User user = FoundUsers.FirstOrDefault();

                        var cars = from c in db.Cars
                                   where c.UserId == user.Id
                                   select c;

                        var listCars = new List<CarViewModel>();

                        foreach (Car car in cars)
                        {
                            var tempcar = new CarViewModel();
                            tempcar.Make = car.Make;
                            tempcar.Model = car.Model;
                            tempcar.Year = car.Year;
                            tempcar.Rate = db.Quotes.Where(x => x.Id == car.QuoteId).ToList().FirstOrDefault().Rate;
                            tempcar.IssueDate = db.Quotes.Where(x => x.Id == car.QuoteId).ToList().FirstOrDefault().IssueDate;
                            listCars.Add(tempcar);
                        }

                        //Calculate Points
                        var Tickets = db.Tickets;
                        var userTickets = db.Tickets.Where(x => x.UserId == user.Id).ToList();
                        int points = userTickets.Count(x => (DateTime.Now - x.IssueDate).Days <= 1095);

                        userInfo.FirstName = user.FirstName;
                        userInfo.LastName = user.LastName;
                        userInfo.UserEmail = user.Email;
                        userInfo.NumCars = cars.Count();
                        userInfo.Points = points;
                        userInfo.Dui = (user.DuiActive == 1) ? true : false;
                        userInfo.TotalRate = user.CompleteQuote;
                        userInfo.Cars = listCars;

                        return View(userInfo);
                    }
                }
            }
        }

        private decimal CalcCarQuote(User user, Car car, int points, string coverage)
        {
            double rate = 50.00;
            //calc age
            var daysalive = DateTime.Now - user.DateOfBirth;
            int age = daysalive.Days / 365;
            //tack on for certain age ranges
            if (age < 18) rate += 100.00;
            else if ((age < 25) || (age > 100)) rate += 25.00;
            //tack on for certain car years
            if ((car.Year < 2000) || (car.Year > 2015)) rate += 25.00;
            //tack on for certain models and makes
            if (car.Make == "porsche")
            {
                rate += 25.00;
                if (car.Model == "911 carrera") rate += 25.00;
            }
            //adjust for points on record
            rate += (points * 10.00);
            //adjust if dui on record
            if (user.DuiActive == 1) rate *= 1.25;
            //adjust if full coverage type
            if (coverage == "0") rate *= 1.50;

            return Convert.ToDecimal(rate);
        }

        private void AddQuoteAndCar(User user, string make, string model, int year, string coverage)
        {
            using (InsuranceEntities db = new InsuranceEntities())
            {
                //create car object
                var tempCar = new Car();
                tempCar.Make = make.ToLower();
                tempCar.Model = model.ToLower();
                tempCar.Year = year;

                //Calculate Points
                var Tickets = db.Tickets;
                var userTickets = db.Tickets.Where(x => x.UserId == user.Id).ToList();
                int points = userTickets.Count(x => (DateTime.Now - x.IssueDate).Days <= 1095);

                //find rate on car
                decimal rate = CalcCarQuote(user, tempCar, points, coverage);

                //create quote object and add to db
                var tempQuote = new Quote();
                tempQuote.Rate = rate;
                tempQuote.CoverageType = coverage;
                tempQuote.IssueDate = DateTime.Now;

                db.Quotes.Add(tempQuote);
                db.SaveChanges();

                //add foreign keys to car object and save car to db
                tempCar.UserId = user.Id;
                tempCar.QuoteId = tempQuote.Id;

                db.Cars.Add(tempCar);
                db.SaveChanges();
            }
        }
    }
}