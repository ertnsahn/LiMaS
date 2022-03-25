using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSWebApp.Data
{

    [Table(name: "CheckoutHistory")]
    public class BookCheckoutHistory
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(250)]
        public string PersonName { get; set; }

        [MaxLength(20)]
        public string MobileNumber { get; set; }

        [MaxLength(20)]
        public string NationalID { get; set; }

        public DateTime? CheckoutDate { get; set; }

        public DateTime? ReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        public int BookId { get; set; }

        public string Currency { get; set; }

        public int? Penality { get; set; }
    }
}
