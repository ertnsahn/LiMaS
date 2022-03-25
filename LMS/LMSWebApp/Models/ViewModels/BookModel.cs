using System.ComponentModel.DataAnnotations;

namespace LMSWebApp.Models.ViewModels
{
    public class BookModel
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Book Title")]
        [StringLength(250)]
        public string BookTitle { get; set; }
        [Required]
        [StringLength(250)]
        public string IBAN { get; set; }
        [Required]
        [Display(Name = "Publish Year")]
        public int? PublishYear { get; set; }
        [Required]
        [Display(Name = "Cover Price")]
        public decimal? CoverPrice { get; set; }
        public string Status { get; set; }

    }
}
