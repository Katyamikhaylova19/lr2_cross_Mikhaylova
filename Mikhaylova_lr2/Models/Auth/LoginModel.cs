using System.ComponentModel.DataAnnotations;

namespace Mikhaylova_lr2.Models.Auth
{
    public class LoginModel
    {
        [Required] public string Username { get; set; }
        [Required] public string Password { get; set; }
    }
}
