using ChatAppServer.Models;

namespace ChatAppServer.Interfaces
{
    public interface IChatMessage
    {
        Task<bool> SendMessage(Message message);
        Task<bool> SetMessageAsSeen(string messageid);
        Task<bool> UpdateMessage(Message message);
        Task<List<Message>?> GetFriendMessageHistry(string friendId);
        Task<List<Message>?> GetGroupMessageHistry(string groupId);
    }
}