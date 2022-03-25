using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LMSWebApp.Models.ViewModels
{
    public class CheckInModel
    {
        public int Id { get; set; }
        public string PersonName { get; set; }
        public string MobileNumber { get; set; }
        public string NationalID { get; set; }
        public DateTime? CheckoutDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        [Required]
        public DateTime? ActualReturnDate { get; set; }

        public int BookId { get; set; }
        [Required]

        public string Currency { get; set; }

        public int? Penality { get; set; }
    }
}
