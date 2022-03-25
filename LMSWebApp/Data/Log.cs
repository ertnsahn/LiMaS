using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSWebApp.Data
{
    [Table(name: "Log")]
    public class Log
    {
        [Key]
        public long Id { get; set; }
        public string ErrorDetailsMessage { get; set; }
    }
}
