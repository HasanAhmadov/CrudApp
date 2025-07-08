using System.ComponentModel.DataAnnotations;

namespace CrudApp.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberLogin {  get; set; }
        public string ReturnUrl { get; set; }
    }
}
