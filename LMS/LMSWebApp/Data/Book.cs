using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSWebApp.Data
{
    [Table(name: "Book")]
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(250)]
        [Required]
        public string BookTitle { get; set; }

        [MaxLength(250)]
        [Required]
        public string ISBN { get; set; }

        [Required]
        public int? PublishYear { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Required]
        public decimal? CoverPrice { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }
}
