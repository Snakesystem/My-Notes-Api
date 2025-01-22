using Dapper;
using My_Notes_Api.Context;

namespace My_Notes_Api.Services
{
    public interface IAuthService
    {
        public Task<ActionResult> Login(LoginRequest request);
        public Task<ActionResult> Register(RegisterRequest request);

    }

    public class AuthService: IAuthService
    {
        private readonly DapperContext _context;
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;

        public AuthService(DapperContext content, ILoggerManager logger, IConfiguration configuration) 
        {
            _configuration = configuration;
            _logger = logger;
            _context = content;
        }

        public async Task<ActionResult> Login(LoginRequest request)
        {
            ActionResult result = new ActionResult();

            return result;
        }

        public async Task<ActionResult> Register(RegisterRequest request)
        {
            string PROC_NAME = "Register";
            ActionResult result = new ActionResult();

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    connection.Open();

                    var p = new DynamicParameters();
                    p.Add("@Email", request.Email);

                    var existUser = await connection.QueryAsync("SELECT * FROM [dbo].[User] WHERE [Email] = @Email", p);
                    if(existUser.Any())
                    {
                        result.message = "User already exist";
                    } else
                    {
                        using(var trans = connection.BeginTransaction())
                        {
                            Crypto.EncryptText enc = new Crypto.EncryptText();
                            request.Password = enc.EncryptHash(request.Password);

                            p.Add("@UserName", request.UserName);
                            p.Add("@Email", request.Email);
                            p.Add("@Password", request.Password);
                            p.Add("@RegistrationDate", request.RegistrationDate);
                            p.Add("@AppComputerName", request.CompName);
                            p.Add("@AppIPAddress", request.IPAddress);

                            await connection.ExecuteAsync("INSERT INTO [User] (UserID, UserName, Email, Password, RegistrationDate, AppComputerName, AppIPAddress) VALUES ('SX', @UserName, @Email, @Password, @RegistrationDate, @AppComputerName, AppIPAddress)", p, trans);

                            trans.Commit(); 
                            result.result = true;
                            result.message = "Resgister succesfully";
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                result.message = $"{PROC_NAME} : {ex.Message}";
                _logger.LogInfo($"{PROC_NAME} === {ex.Message}");
            }

            return result;
        }
    }
}

