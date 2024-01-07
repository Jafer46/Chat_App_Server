using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace ChatAppServer.Models
{
    public class UserFriend
    {
        [Key]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public ObjectId UserFreindId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}