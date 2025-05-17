using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallStat;

namespace FootBallStat.Controllers
{
    public class MatchesController : Controller
    {
        private readonly DBFootballStatContext _context;

        public MatchesController(DBFootballStatContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var matches = _context.Matches.Include(m => m.Championship).Include(m => m.Team1).Include(m => m.Team2);
            return View(await matches.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.Championship)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null) return NotFound();

            return RedirectToAction("Index", "PlayersInMatches", new
            {
                id = match.Id,
                team1Id = match.Team1Id,
                team2Id = match.Team2Id,
                championshipId = match.ChampionshipId,
                date = match.Date
            });
        }

        public IActionResult Create()
        {
            PopulateMatchDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Team1Id,Team2Id,ChampionshipId,Date")] Match match)
        {
            if (match.Team1Id == match.Team2Id)
            {
                ViewData["ErrorMessage"] = "Команда не може грати сама проти себе!";
            }
            else if (!IsUnique(match.Team1Id, match.Team2Id, match.Date))
            {
                ViewData["ErrorMessage"] = "Одна з команд вже проводить матч в цей день!";
            }
            else if (ModelState.IsValid)
            {
                _context.Add(match);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateMatchDropdowns(match);
            return View(match);
        }

        private bool IsUnique(int team1Id, int team2Id, DateTime date)
        {
            return !_context.Matches.Any(match =>
                (match.Team1Id == team1Id || match.Team2Id == team2Id ||
                 match.Team1Id == team2Id || match.Team2Id == team1Id) &&
                match.Date.Date == date.Date);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.Championship)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .FirstOrDefaultAsync(m => m.Id == id);

            return match == null ? NotFound() : View(match);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Team1Id,Team2Id,ChampionshipId,Date")] Match match)
        {
            if (id != match.Id) return NotFound();

            var existingMatch = await _context.Matches.FindAsync(id);
            if (existingMatch == null) return NotFound();

            bool sameDay = existingMatch.Date.Date == match.Date.Date;

            existingMatch.Date = match.Date;

            if (!IsUnique(match.Team1Id, match.Team2Id, match.Date) && !sameDay)
            {
                ViewData["ErrorMessage"] = "Одна з команд вже проводить матч в цей день!";
                return View(existingMatch);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(existingMatch);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatchExists(existingMatch.Id)) return NotFound();
                    throw;
                }
            }

            return View(existingMatch);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.Championship)
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .FirstOrDefaultAsync(m => m.Id == id);

            return match == null ? NotFound() : View(match);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            var playersInMatches = _context.PlayersInMatches.Where(p => p.MatchId == id);
            _context.PlayersInMatches.RemoveRange(playersInMatches);

            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool MatchExists(int id) => _context.Matches.Any(e => e.Id == id);

        private void PopulateMatchDropdowns(Match? match = null)
        {
            ViewData["Team1Id"] = new SelectList(_context.Teams, "Id", "Name", match?.Team1Id);
            ViewData["Team2Id"] = new SelectList(_context.Teams, "Id", "Name", match?.Team2Id);
            ViewData["ChampionshipId"] = new SelectList(_context.Championships, "Id", "Name", match?.ChampionshipId);
        }
    }
}
