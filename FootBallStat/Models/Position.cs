using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallStat
{
    public partial class Position
    {
        public Position()
        {
            Players = new HashSet<Player>();
        }

        public int Id { get; set; }
        [Display(Name = "Назва позиції")]
        public string Name { get; set; } = null!;

        public virtual ICollection<Player> Players { get; set; }
    }
}
