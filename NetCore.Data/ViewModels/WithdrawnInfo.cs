using System.ComponentModel.DataAnnotations;

namespace NetCore.Data.ViewModels
{
    public class WithdrawnInfo
    {
        /// <summary>
        /// 사용자 아이디
        /// </summary>
        public string? UserId { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "비밀번호를 입력하세요.")]
        [MinLength(6, ErrorMessage = "비밀번호는 6자 이상 입력하세요.")]
        [Display(Name = "비밀번호")]
        public string? Password { get; set; }
    }
}
