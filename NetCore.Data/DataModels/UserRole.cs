using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCore.Data.DataModels
{
    public class UserRole
    {
        [Key, StringLength(50), Column(TypeName = "varchar(50)")]
        public string RoleId { get; set; } = string.Empty;

        [Required, StringLength(100), Column(TypeName = "nvarchar(100)")]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        public byte RolePriority { get; set; }

        [Required]
        public DateTime ModifiedUtcDate { get; set; }

        [ForeignKey("RoleId")]
        public virtual ICollection<UserRolesByUser>? UserRolesByUsers { get; set; }

    }
}
