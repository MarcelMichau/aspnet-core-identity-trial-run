using System.ComponentModel.DataAnnotations;

namespace CoreIdentityTrialRun.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
