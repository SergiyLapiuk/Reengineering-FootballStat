using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FootBallStat
{
    public class CurrentDate1Attribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime dateTime = Convert.ToDateTime(value);

            DateTime dateNow = DateTime.UtcNow;

            var diff = dateNow.Year - dateTime.Year;

            return Math.Abs(diff) <= 4;
        }
    }
    public partial class Match
    {
        public Match()
        {
            PlayersInMatches = new HashSet<PlayersInMatch>();
        }
        public int Id { get; set; }
        [Display(Name = "Команда-господар")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public int Team1Id { get; set; }
        [Display(Name = "Команда-гість")]
        public int Team2Id { get; set; }
        [Display(Name = "Чемпіонат")]
        public int ChampionshipId { get; set; }
        [Display(Name = "Дата та час")]
        [CurrentDate1(ErrorMessage = "Матч, що проводився більш ніж 4 роки тому, додати не можна, і також матч не можна запланувати більш ніж на 4 роки в перед.")]
        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public DateTime Date { get; set; }

        [Display(Name = "Чемпіонат")]
        public virtual Championship Championship { get; set; } = null!;
        [Display(Name = "Команда-господар")]
        public virtual Team Team1 { get; set; } = null!;
        [Display(Name = "Команда-гість")]
        public virtual Team Team2 { get; set; } = null!;
        public virtual ICollection<PlayersInMatch> PlayersInMatches { get; set; }
    }
}
