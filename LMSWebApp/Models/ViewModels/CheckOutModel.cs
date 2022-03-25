using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LMSWebApp.Models.ViewModels
{
    public class CheckOutModel
    {
        public int Id { get; set; }
        [Required]
        public string PersonName { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string NationalID { get; set; }
        [Required]
        public DateTime? CheckoutDate { get; set; }
        [Required]
        public DateTime? ReturnDate { get; set; }
        [Required]
        public int BookId { get; set; }

    }
}
