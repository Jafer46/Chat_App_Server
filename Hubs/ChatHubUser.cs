using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppServer.Hubs
{
    public partial class ChatHub : IChatUser
    {
        public async Task<bool> AddFriend(string toBeFreindId)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return false;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(x => x.Token == token);
                if (cuser == null)
                {
                    return false;
                }
                UserFriend userFriend = new UserFriend();
                userFriend.CreatedAt = DateTime.Now;
                userFriend.UserId = cuser.Id;
                userFriend.UserFreindId = MongoDB.Bson.ObjectId.Parse(toBeFreindId);
                if (!await _userFreindServices.Create(userFriend))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<List<User>?> GetFriends(string userId)
        {
            List<UserFriend> userFriends = await _userFreindServices
                             .GetAll(x => x.UserId.ToString() == userId || x.UserFreindId.ToString() == userId);
            if (userFriends == null)
            {
                return null;
            }
            List<User> friends = new List<User>();
            foreach (var userFriend in userFriends)
            {
                if (userFriend.UserId.ToString() == userId)
                {
                    friends.Add(await GetUserById(userFriend.UserFreindId.ToString()));
                }
                else
                {
                    friends.Add(await GetUserById(userFriend.UserId.ToString()));
                }
            }
            return friends;
        }
        public async Task<User?> GetUserById(string userId)
        {
            User? user = await _userServices.ReadFirst(u => u.Id.ToString() == userId);
            return user;
        }
        public async Task<bool> IsUserOnline(string userId)
        {
            User? user = await _userServices.ReadFirst(x => x.Id.ToString() == userId);
            if (user == null)
            {
                return false;
            }
            return user.IsOnline;
        }

        public async Task<bool> RemoveFriend(string freindId)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return false;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(u => u.Token == token);
                if (cuser == null)
                {
                    return false;
                }
                if (!await _userFreindServices
                   .Delete(x => (x.UserId == cuser.Id && x.UserFreindId.ToString() == freindId) ||
                   (x.UserId.ToString() == freindId && x.UserFreindId == cuser.Id)))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<User>?> SearchUser(string query, int maxResult = 20)
        {
            IEnumerable<User>? users = (await _userServices.GetAll(x =>
            x.Username.Contains(query, StringComparison.InvariantCultureIgnoreCase)
            || x.Email.Contains(query, StringComparison.InvariantCultureIgnoreCase)))
            .OrderBy(x => x.Username).Take(maxResult);

            if (users == null)
            {
                return null;
            }
            return users.ToList();
        }
    }
}