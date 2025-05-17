using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootBallStat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly DBFootballStatContext _context;
        public ChartController(DBFootballStatContext context)
        {
            _context = context;
        }
        [HttpGet("JsonDataMatches")]
        public JsonResult JsonDataMatches()
        {
            var matches = _context.Matches.Include(m => m.PlayersInMatches).Include(m => m.Team1).Include(m => m.Team2).ToList();
            List<object> matPlayerInMatch = new List<object>();
            matPlayerInMatch.Add(new[] { "Матч", "Кількість гравців" });
            foreach (var m in matches)
            {
                string match_name = m.Team1.Name + " | " + m.Team2.Name;
                matPlayerInMatch.Add(new object[] { match_name, m.PlayersInMatches.Count() });
            }

            return new JsonResult(matPlayerInMatch);
        }

        [HttpGet("JsonDataTeams")]
        public JsonResult JsonDataTeams()
        {
            var teams = _context.Teams.Include(t => t.Players).ToList();
            List<object> teamPlayer = new List<object>();
            teamPlayer.Add(new[] { "Команда", "Кількість гравців в команді" });
            foreach (var t in teams)
            {
                teamPlayer.Add(new object[] { t.Name, t.Players.Count() });
            }

            return new JsonResult(teamPlayer);
        }

        [HttpGet("JsonDataCountries")]
        public JsonResult JsonDataCountries()
        {
            var championships = _context.Countries.Include(t => t.Championships).ToList();
            List<object> countryChampionship = new List<object>();
            countryChampionship.Add(new[] { "Країна", "Кількість чемпіонатів в країні" });
            foreach (var  ch in championships)
            {
                countryChampionship.Add(new object[] { ch.Name, ch.Championships.Count() });
            }

            return new JsonResult(countryChampionship);
        }
    }
}
