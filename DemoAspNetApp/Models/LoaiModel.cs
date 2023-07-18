using System.ComponentModel.DataAnnotations;

namespace DemoAspNetApp.Models
{
    public class LoaiModel
    {
        [Required]
        [MaxLength(50)]
        public string TenLoai { get; set; }
    }
}
