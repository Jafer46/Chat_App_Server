using ChatAppServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppServer.Interfaces
{
    public interface IChatAuth
    {
        Task<dynamic?> SignIn(string email, string password);
        Task<dynamic?> SignUp(string userName, string email, string password);
    }
}