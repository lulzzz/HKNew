using System.ComponentModel.DataAnnotations;

namespace HK_Project.ViewModels
{
    public class UserLoginSinginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
