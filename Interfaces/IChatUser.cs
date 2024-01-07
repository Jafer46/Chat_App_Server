using ChatAppServer.Models;

namespace ChatAppServer.Interfaces
{
    public interface IChatUser
    {
        Task<List<User>?> GetFriends(string userId);
        Task<User?> GetUserById(string userId);
        Task<bool> IsUserOnline(string userId);
        Task<bool> AddFriend(string toBeFriendId);
        Task<bool> RemoveFriend(string freindId);
        Task<List<User>?> SearchUser(string query, int maxResult = 20);
    }
}