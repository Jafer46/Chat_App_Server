using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;

namespace ChatAppServer.Models
{
    public class GroupUser
    {
        [Key]
        public ObjectId Id { get; set; }
        public ObjectId GroupId { get; set; }
        public ObjectId UserId { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}