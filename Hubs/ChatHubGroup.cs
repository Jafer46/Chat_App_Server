using ChatAppServer.Interfaces;
using ChatAppServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppServer.Hubs
{
    public partial class ChatHub : IChatGroup
    {
        public async Task<bool> AddGroupUser(string groupId, string userId)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null || userId == null)
                {
                    return false;
                }

                string? Token = hc.Request.Query["access_token"];

                User? cuser = await _userServices.ReadFirst(x => x.Token == Token);
                User? user = await _userServices.ReadFirst(x => x.Id.ToString() == userId);

                if (cuser == null || user == null)
                {
                    return false;
                }

                if (await GroupContainsUser(groupId, userId) || !await GroupContainsUser(groupId, cuser.Id.ToString()))
                {
                    return false;
                }

                GroupUser groupUser = new GroupUser()
                {
                    GroupId = MongoDB.Bson.ObjectId.Parse(groupId),
                    UserId = MongoDB.Bson.ObjectId.Parse(userId)
                };
                if (cuser.Id.ToString() == userId)
                {
                    groupUser.IsAdmin = true;
                }
                if (await _groupUserServices.Create(groupUser))
                {
                    Message message = new Message()
                    {
                        SenderId = cuser.Id,
                        GroupId = groupId,
                        Text = $"{cuser.Username} added {user.Username}"
                    };
                    await SendMessage(message);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public async Task<Group?> CreateGroup(string groupName)
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return null;
                }

                string? Token = hc.Request.Query["access_token"];

                User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

                if (cuser == null)
                {
                    return null;
                }

                Group group = new Group()
                {
                    Title = groupName,
                    CreatedAt = DateTime.Now
                };
                if (await _groupServices.Create(group))
                {
                    await AddGroupUser(group.Id.ToString(), cuser.Id.ToString());
                    return group;
                }
            }
            catch { }
            return null;
        }

        public async Task<bool> DeleteGroup(string groupId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if (hc == null)
            {
                return false;
            }

            string? Token = hc.Request.Query["access_token"];

            User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

            if (cuser == null)
            {
                return false;
            }

            if (!await IsGroupAdmin(groupId, cuser.Id.ToString()))
            {
                return false;
            }

            if (!await _groupUserServices.Delete(x => x.GroupId.ToString() == groupId))
            {
                return false;
            }

            if (!await _messageServices.Delete(x => x.GroupId == groupId))
            {
                return false;
            }

            return await _groupServices.Delete(x => x.Id.ToString() == groupId);
        }

        public async Task<List<User>?> GetGroupUsers(string groupId)
        {
            List<GroupUser> groupUsers = await _groupUserServices.GetAll(x => x.GroupId.ToString() == groupId);
            if (groupUsers == null)
            {
                return null;
            }
            List<User> users = new List<User>();
            foreach (var groupUser in groupUsers)
            {
                User? user = await _userServices.ReadFirst(u => u.Id == groupUser.UserId);
                if (user == null) continue;
                users.Add(user);
            }
            return users;
        }

        public async Task<List<Group>?> GetUsergroups()
        {
            try
            {
                HttpContext? hc = Context.GetHttpContext();
                if (hc == null)
                {
                    return null;
                }

                string? Token = hc.Request.Query["access_token"];

                User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

                if (cuser == null)
                {
                    return null;
                }
                List<GroupUser>? groupUsers = await _groupUserServices.GetAll(x => x.UserId == cuser.Id);
                if (groupUsers == null)
                {
                    return null;
                }
                List<Group> groups = new List<Group>();
                foreach (var groupUser in groupUsers)
                {
                    Group? group = await _groupServices.ReadFirst(x => x.Id == groupUser.GroupId);
                    if (group == null)
                    {
                        continue;
                    }
                    groups.Add(group);
                }
                return groups;
            }
            catch { }
            return null;
        }

        public async Task<bool> GroupContainsUser(string groupId, string userId)
        {
            GroupUser? groupUser = await _groupUserServices
                         .ReadFirst(x => x.GroupId.ToString() == groupId && x.UserId.ToString() == userId);
            if (groupUser == null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> IsGroupAdmin(string groupId, string userId)
        {
            GroupUser? groupUser = await _groupUserServices.ReadFirst(x => x.UserId.ToString() == userId && x.GroupId.ToString() == groupId);
            if (groupUser == null)
            {
                return false;
            }
            return groupUser.IsAdmin;
        }

        public async Task<bool> LeaveGroup(string groupId)
        {
            HttpContext? hc = Context.GetHttpContext();
            if (hc == null)
            {
                return false;
            }

            string? Token = hc.Request.Query["access_token"];

            User? cuser = await _userServices.ReadFirst(x => x.Token == Token);

            if (cuser == null)
            {
                return false;
            }

            return await _groupUserServices.Delete(x => x.UserId == cuser.Id && x.GroupId.ToString() == groupId);
        }
    }
}