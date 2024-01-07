using ChatAppServer.Models;

namespace ChatAppServer.Interfaces
{
    public interface IChatGroup
    {
        Task<Group?> CreateGroup(string groupName);
        Task<bool> DeleteGroup(string groupId);
        Task<bool> AddGroupUser(string channelId, string userId);
        Task<bool> LeaveGroup(string groupId);
        Task<bool> GroupContainsUser(string groupId, string userId);
        Task<List<User>?> GetGroupUsers(string groupId);
        Task<List<Group>?> GetUsergroups();
        Task<bool> IsGroupAdmin(string channelId, string userId);
    }
}