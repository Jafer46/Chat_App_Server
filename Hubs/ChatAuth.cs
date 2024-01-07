using ChatAppServer.Authentication;
using ChatAppServer.Helpers;
using ChatAppServer.Interfaces;
using ChatAppServer.Models;

namespace ChatAppServer.Hubs
{
    public partial class ChatHub : IChatAuth
    {

        public async Task<dynamic?> SignUp(string username, string email, string password)
        {
            if (!PatternMatchHelper.IsValidUsername(username)
                || !PatternMatchHelper.IsValidPassword(password))
            {
                System.Console.WriteLine("username orpassword invalid");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                if (PatternMatchHelper.IsValidEmail(email))
                {
                    email = email.ToLower();
                }
                else
                {
                    email = string.Empty;
                }
            }

            if (await _userServices.ReadFirst(x => x.Username == username) != null)
            {
                System.Console.WriteLine("user doesnt exist");
                return null;
            }

            string encryptedPassword = CryptographyHelper.SecurePassword(password);

            User user = new()
            {
                Username = username,
                Email = email,
                Password = encryptedPassword,
                ConnectionId = Context.ConnectionId,
                DateCreated = DateTime.Now,
                IsOnline = true
            };

            if (_config == null)
            {
                System.Console.WriteLine("configuration not found");
                return null;
            }

            var generatedToken = await TokenGenerator.GenerateJwtToken(user, _config.GetSection("Secrets")["Jwt"]!);
            user.Token = generatedToken.Access_Token;


            if (await _userServices.Create(user))
            {
                var result = new
                {
                    user.Id,
                    user.Token,
                };
                System.Console.WriteLine(result.Token);
                return result;
            }
            System.Console.WriteLine("something went wrong");
            return null;
        }
        public async Task<dynamic?> SignIn(string email, string password)
        {
            if (PatternMatchHelper.IsValidEmail(email))
            {
                User? user = await _userServices.ReadFirst(x => x.Email == email);

                if (user == null)
                {
                    return null;
                }
                if (string.IsNullOrWhiteSpace(user.Password))
                {
                    return null;
                }
                if (!CryptographyHelper.ComparePassword(password, user.Password))
                {
                    return null;
                }


                User registeredUser = user;

                registeredUser.ConnectionId = Context.ConnectionId;
                registeredUser.IsOnline = true;

                var generatedToken = await TokenGenerator.GenerateJwtToken(registeredUser, _config.GetSection("Secrets")["Jwt"]!);
                registeredUser.Token = generatedToken.Access_Token;
                if (await _userServices.Update(registeredUser))
                {
                    System.Console.WriteLine(registeredUser.Token);
                    return new
                    {
                        registeredUser.Id,
                        registeredUser.Token
                    };
                }
                return null;
            }
            return null;
        }
    }
}