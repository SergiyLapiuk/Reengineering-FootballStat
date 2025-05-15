using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallStat
{
    public partial class Championship
    {
        public Championship()
        {
            Matches = new HashSet<Match>();
        }

        public int Id { get; set; }


        [Display(Name = "Країна")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        [Display(Name = "Чемпіонат")]
        [MinLength(3, ErrorMessage = "Занадто мало символів!")]
        [MaxLength(20)]
        public string Name { get; set; } = null!;

        [Display(Name = "Країна")]
        public virtual Country Country { get; set; } = null!;
        public virtual ICollection<Match> Matches { get; set; }
    }
}
