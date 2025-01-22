namespace My_Notes_Api
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ActionResult
    {
        public bool result { get; set; }
        public string message { get; set; }

        public ActionResult()
        {
            result = false;
            message = string.Empty;
        }
    }

    public class RegisterRequest
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string IPAddress { get; set; }
        public string CompName { get; set; }
    }
}
