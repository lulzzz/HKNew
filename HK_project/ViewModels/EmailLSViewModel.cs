﻿using System.ComponentModel.DataAnnotations;

namespace HK_Project.ViewModels
{
    public class EmailLSViewModel
    {
        [Required(ErrorMessage = "必須輸入")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

    }
}
