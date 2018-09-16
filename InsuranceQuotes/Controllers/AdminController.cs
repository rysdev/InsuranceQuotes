using InsuranceGenerator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceGenerator.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var UserList = new List<AdminViewModel>();
            using (InsuranceEntities db = new InsuranceEntities())
            {
                var users = db.Users;
                foreach (var user in users)
                {
                    var userInfo = new AdminViewModel();

                    var cars = from c in db.Cars
                               where c.UserId == user.Id
                               select c;

                    userInfo.FirstName = user.FirstName;
                    userInfo.LastName = user.LastName;
                    userInfo.UserEmail = user.Email;
                    userInfo.NumCars = cars.Count();

                    userInfo.TotalRate = user.CompleteQuote;

                    UserList.Add(userInfo);
                }
            }

            return View(UserList);
        }
    }
}