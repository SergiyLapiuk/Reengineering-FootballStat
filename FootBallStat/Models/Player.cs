using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FootBallStat
{
    public class CurrentDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime dateTime = Convert.ToDateTime(value);
            DateTime dateNow = DateTime.UtcNow; 
            var diff = dateNow.Year - dateTime.Year;
            return 17 <= diff && diff <= 45;
        }
    }

    public partial class Player
    {
        public Player()
        {
            PlayersInMatches = new HashSet<PlayersInMatch>();
        }

        public int Id { get; set; }
        [Display(Name = "Команда")]
        public int TeamId { get; set; }
        [Display(Name = "Позиція на полі")]
        public int PositionId { get; set; }
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        [Display(Name = "Дата народження")]
        [CurrentDate(ErrorMessage = "Вік футболіста повинен бути від 17 до 45 років.")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime DateOfBirth { get; set; }
        [Display(Name = "Ім'я")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        [MinLength(3, ErrorMessage = "Занадто мало символів!")]
        [MaxLength(20)]
        public string Name { get; set; } = null!;
        [Display(Name = "Ігровий номер")]
        [Range(1, 99,
        ErrorMessage = "{0} повинен від {1} до {2}.")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public int Number { get; set; }

        [Display(Name = "Позиція на полі")]
        public virtual Position Position { get; set; } = null!;
        [Display(Name = "Команда")]
        public virtual Team Team { get; set; } = null!;
        public virtual ICollection<PlayersInMatch> PlayersInMatches { get; set; }
    }
}
