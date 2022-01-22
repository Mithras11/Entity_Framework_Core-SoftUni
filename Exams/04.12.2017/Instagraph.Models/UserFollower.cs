using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Instagraph.Models
{
    public class UserFollower
    {
        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        [ForeignKey(nameof(Follower))]
        public int FollowerId { get; set; }

        [Required]
        public User Follower { get; set; }
    }
}