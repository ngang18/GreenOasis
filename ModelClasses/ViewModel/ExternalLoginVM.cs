using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace ModelClasses.ViewModel
{
    public class ExternalLoginVM
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? LoginStatus { get; set; }

        //Third-party login
        public IEnumerable<AuthenticationScheme> Schemes { get; set; }
    }
}
