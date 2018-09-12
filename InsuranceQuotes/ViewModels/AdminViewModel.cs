using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceQuotes.ViewModels
{
    public class AdminViewModel
    {
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DisplayName("Email")]
        public string UserEmail { get; set; }
        [DisplayName("# of Cars on Policy")]
        public int NumCars { get; set; }
        [DisplayName("Rate"), DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? TotalRate { get; set; }

    }
}