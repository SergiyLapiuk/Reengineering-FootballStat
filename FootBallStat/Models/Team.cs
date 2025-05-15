using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallStat
{
    public partial class Team
    {
        public Team()
        {
            MatchTeam1s = new HashSet<Match>();
            MatchTeam2s = new HashSet<Match>();
            Players = new HashSet<Player>();
        }

        public int Id { get; set; }
        [Display (Name = "Команда")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        [MinLength(2, ErrorMessage = "Занадто мало символів!")]
        [MaxLength(20)]
        public string Name { get; set; } = null!;

        public virtual ICollection<Match> MatchTeam1s { get; set; }
        public virtual ICollection<Match> MatchTeam2s { get; set; }
        public virtual ICollection<Player> Players { get; set; }
    }
}
