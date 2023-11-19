using System.ComponentModel.DataAnnotations;

namespace NetCore.Data.ViewModels
{
    public class ChangeInfo
    {
        [Required(ErrorMessage = "사용자 이름을 입력하세요.")]
        [Display(Name = "사용자 이름")]
        public required string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "사용자 이메일을 입력하세요.")]
        [Display(Name = "사용자 이메일")]
        public required string UserEmail { get; set; }

        /// <summary>
        /// true : 전부 똑같을 때, false : 하나라도 다를 때
        /// </summary>
        /// <param name="other">비교할 다른 클래스</param>
        /// <returns></returns>
        public bool Equals(UserInfo other)
        {
            if (!string.Equals(UserName, other.UserName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals(UserEmail, other.UserEmail, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
