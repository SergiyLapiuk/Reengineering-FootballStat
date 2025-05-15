using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace FootBallStat
{
    public partial class PlayersInMatch
    {
        public int Id { get; set; }
        [Display(Name = "Гравець")]
        public int PlayerId { get; set; }
        [Display(Name = "Матч")]
        public int MatchId { get; set; }
        [Display(Name = "Голи гравця в матчі")]
        [Range(0, 10,
        ErrorMessage = "{0} повинні бути від {1} до {2}.")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public int PlayerGoals { get; set; }

        [Display(Name = "Матч")]
        public virtual Match Match { get; set; } = null!;
        [Display(Name = "Гравець")]
        public virtual Player Player { get; set; } = null!;
    }
}
