using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InsuranceQuotes.ViewModels
{
    public class CarViewModel
    {
        [DisplayName("Make")]
        public string Make { get; set; }
        [DisplayName("Model")]
        public string Model { get; set; }
        [DisplayName("Year")]
        public int Year { get; set; }
        [DisplayName("Rate per Car"), DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal? Rate { get; set; }
        [DisplayName("Policy Date")]
        public DateTime? IssueDate { get; set; }
    }
}