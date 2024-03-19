using System;
namespace Google.Calendar.Models
{
	public class User
	{
        public string Name { get; set; }
        public string Email { get; set; }
        public string Surname { get; set; }
        public string EmailVerificationCode { get; set; }
        public int Status { get; set; }
    }
}

