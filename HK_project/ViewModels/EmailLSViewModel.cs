using System.ComponentModel.DataAnnotations;

namespace HK_Project.ViewModels
{
    public class EmailLSViewModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

    }
}
