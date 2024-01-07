using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppServer.Hubs
{
    public partial class ChatHub : IChatMessage
    {
        public async Task<List<Message>?> GetFriendMessageHistry(string friendId)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return null;
                }
                var token = hc.Request.Query["access_token"];
                User? cuser = await _userServices.ReadFirst(u => u.Token == token);
                if (cuser == null)
                {
                    return null;
                }

                IEnumerable<Message>? messages =
                            await _messageServices.GetAll(m =>
                            (m.SenderId == cuser.Id && m.RecieverId.ToString() == friendId) ||
                            (m.SenderId.ToString() == friendId && m.RecieverId == cuser.Id.ToString()));
                List<Message> messageList = messages.Take(20).ToList();
                return messageList;
            }
            catch { }
            return null;

        }

        public async Task<List<Message>?> GetGroupMessageHistry(string groupId)
        {
            try
            {
                IEnumerable<Message>? messages = await _messageServices.GetAll(m => m.GroupId.ToString() == groupId);
                List<Message>? messageList = messages.Take(20).ToList();
                return messageList;
            }
            catch { }
            return null;
        }

        public async Task<bool> SendMessage(Message message)
        {
            if (message == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(message.Text) || string.IsNullOrWhiteSpace(message.Text))
            {
                return false;
            }
            message.DateSent = DateTime.UtcNow;
            message.Sent = true;
            if (await _messageServices.Create(message))
            {
                if (message.GroupId == null && message.RecieverId != null)
                {
                    try
                    {
                        List<User> users = new List<User>();
                        users.Add(await _userServices.ReadFirst(u => u.Id == message.SenderId));
                        users.Add(await _userServices.ReadFirst(u => u.Id.ToString() == message.RecieverId));
                        if (users == null)
                        {
                            return false;
                        }
                        foreach (var user in users)
                        {
                            await Clients.Client(user.ConnectionId).SendAsync("RecieveMessage", message);
                        }
                    }
                    catch { }
                    return true;
                }
                else if (message.GroupId != null && message.RecieverId == null)
                {
                    List<User> users = await GetGroupUsers(message.GroupId);
                    if (users is null)
                    {
                        return false;
                    }
                    foreach (User user in users)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(user.ConnectionId))
                            {
                                continue;
                            }

                            await Clients.Client(user.ConnectionId).SendAsync("ReceiveMessage", message);
                        }
                        catch { }
                    }

                    return true;
                }
            }
            return false;
        }

        public async Task<bool> SetMessageAsSeen(string messageid)
        {
            Message? message = await _messageServices.ReadFirst(m => m.Id.ToString() == messageid);
            if (message == null)
            {
                return false;
            }
            message.Seen = true;
            if (!await UpdateMessage(message))
            {
                return false;
            }
            return true;

        }

        public async Task<bool> UpdateMessage(Message message)
        {
            if (!await _messageServices.Update(message))
            {
                return false;
            }
            try
            {
                List<User>? users = new List<User>();
                if (message.RecieverId != null && message.GroupId == null)
                {
                    users.Add(await _userServices.ReadFirst(u => u.Id == message.SenderId));
                    users.Add(await _userServices.ReadFirst(u => u.Id.ToString() == message.RecieverId));
                    if (users == null)
                    {
                        return false;
                    }
                    foreach (var user in users)
                    {
                        await Clients.Client(user.ConnectionId!).SendAsync("UpdateMessage", message);
                    }
                    return true;
                }
                else if (message.RecieverId == null && message.GroupId != null)
                {
                    users = await GetGroupUsers(message.GroupId);
                    if (users == null)
                    {
                        return false;
                    }
                    foreach (var user in users)
                    {
                        await Clients.Client(user.ConnectionId!).SendAsync("UpdateMessage", message);
                    }
                    return true;
                }

            }
            catch { }

            return false;
        }
    }

}