using System.ComponentModel.DataAnnotations;

namespace HK_Project.ViewModels
{
    public class PasswordLSViewModel
    {
        [Required(ErrorMessage = "必須輸入")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "必須輸入")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
