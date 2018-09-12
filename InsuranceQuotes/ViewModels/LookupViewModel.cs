using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InsuranceQuotes.ViewModels
{
    public class LookupViewModel
    {
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("Email")]
        public string UserEmail { get; set; }
        [DisplayName("# of Cars on Policy")]
        public int NumCars { get; set; }
        [DisplayName("# of Points on Record (From tickets in the last 3 years)")]
        public int Points { get; set; }
        [DisplayName("DUI On Record? (From the last 10 years)")]
        public bool Dui { get; set; }
        [DisplayName("Monthly Rate for this Policy"), DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? TotalRate { get; set; }
        public List<CarViewModel> Cars { get; set; }
    }
}