using System.ComponentModel.DataAnnotations;

namespace FootBallStat.ViewModel
{

    public class RegisterViewModel
    {
        [Required]
        [Display(Name ="Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        
        [Display(Name ="Рік народження")]
        [Range(1920, 2017,
        ErrorMessage = "{0} повинен від {1} до {2}.")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public int Year { get; set; }

        [Required]
        [Display(Name ="Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage ="Паролі не співпадають")]
        [Display(Name ="Підтвердження паролю")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
    }
}
