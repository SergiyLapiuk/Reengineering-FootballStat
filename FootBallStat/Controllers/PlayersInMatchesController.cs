using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallStat;

namespace FootBallStat.Controllers
{
    public class PlayersInMatchesController : Controller
    {
        private readonly DBFootballStatContext _context;

        public PlayersInMatchesController(DBFootballStatContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id, int? team1Id, int? team2Id, int? championshipId, DateTime? date)
        {
            if (id == null)
                return RedirectToAction("Index", "Matches");

            SetMatchMetadataToViewBag(id.Value, team1Id, team2Id, championshipId, date);

            var players = await _context.PlayersInMatches
                .Where(b => b.MatchId == id)
                .Include(b => b.Match)
                .ThenInclude(m => m.Team1)
                .Include(b => b.Match)
                .ThenInclude(m => m.Team2)
                .Include(b => b.Player)
                .ThenInclude(p => p.Team)
                .ToListAsync();

            return View(players);
        }

        private void SetMatchMetadataToViewBag(int matchId, int? team1Id, int? team2Id, int? championshipId, DateTime? date)
        {
            ViewBag.MatchId = matchId;
            ViewBag.MatchTeam1Id = team1Id;
            ViewBag.MatchTeam2Id = team2Id;
            ViewBag.MatchChampionshipId = championshipId;
            ViewBag.MatchDate = date;
        }

        // GET: PlayersInMatches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playersInMatch = await _context.PlayersInMatches
                .Include(p => p.Match)
                .Include(p => p.Player)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (playersInMatch == null)
            {
                return NotFound();
            }

            return View(playersInMatch);
        }

        // GET: PlayersInMatches/Create
        public IActionResult Create(int matchId)
        {
            var match = _context.Matches
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .FirstOrDefault(m => m.Id == matchId);

            if (match == null) return NotFound();

            ViewBag.MatchId = match.Id;
            ViewBag.MatchTeam1Id = match.Team1Id;
            ViewBag.MatchTeam2Id = match.Team2Id;
            ViewBag.MatchChampionshipId = match.ChampionshipId;
            ViewBag.MatchDate = match.Date;

            ViewData["PlayerId"] = new SelectList(
                _context.Players
                    .Where(p => p.TeamId == match.Team1Id || p.TeamId == match.Team2Id),
                "Id",
                "Name"
            );

            return View();
        }


        private async Task<Match?> GetMatchAsync(int matchId)
        {
            return await _context.Matches.FirstOrDefaultAsync(c => c.Id == matchId);
        }

        private SelectList GetPlayersSelectList(int team1Id, int team2Id)
        {
            return new SelectList(
                _context.Players.Where(p => p.TeamId == team1Id || p.TeamId == team2Id),
                "Id", "Name"
            );
        }

        private IActionResult RedirectToMatchView(Match match)
        {
            return RedirectToAction("Index", "PlayersInMatches", new
            {
                id = match.Id,
                team1Id = match.Team1Id,
                team2Id = match.Team2Id,
                championsipId = match.ChampionshipId,
                date = match.Date
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int matchId, [Bind("Id,PlayerId,MatchId,PlayerGoals")] PlayersInMatch playersInMatch)
        {
            var match = await GetMatchAsync(matchId);
            if (match == null) return NotFound();

            playersInMatch.MatchId = matchId;

            if (!IsUnique(playersInMatch.MatchId, playersInMatch.PlayerId))
            {
                ViewData["ErrorMessage"] = "Цей гравець вже бере участь в цьому матчі!";
                ViewData["PlayerId"] = GetPlayersSelectList(match.Team1Id, match.Team2Id);
                ViewBag.MatchId = matchId;
                return View(playersInMatch);
            }

            if (!ModelState.IsValid)
            {
                ViewData["PlayerId"] = GetPlayersSelectList(match.Team1Id, match.Team2Id);
                ViewBag.MatchId = matchId;
                return View(playersInMatch);
            }

            _context.Add(playersInMatch);
            await _context.SaveChangesAsync();

            return RedirectToMatchView(match);
        }


        bool IsUnique(int matchId, int playerId)
        {
            var q = (from playerInMatch in _context.PlayersInMatches
                     where playerInMatch.MatchId == matchId && playerInMatch.PlayerId == playerId
                     select playerInMatch).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        // GET: PlayersInMatches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            
            var playersInMatch = await _context.PlayersInMatches.Include(x => x.Player).FirstOrDefaultAsync(m => m.Id == id); 
            if (playersInMatch == null)
            {
                return NotFound();
            }
            ViewData["MatchId"] = new SelectList(_context.Matches, "Id", "Id", playersInMatch.MatchId);
            int team1Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team1Id;
            int team2Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team2Id;
            ViewData["PlayerId"] = new SelectList(_context.Players.Where(c => c.TeamId == team1Id || c.TeamId == team2Id), "Id", "Name");
            return View(playersInMatch);
        }

        // POST: PlayersInMatches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PlayerId,MatchId,PlayerGoals")] PlayersInMatch updatedData)
        {
            var existing = await _context.PlayersInMatches
                .Include(p => p.Match)
                .Include(p => p.Player)
                .ThenInclude(pl => pl.Team)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existing == null || id != updatedData.Id)
                return NotFound();

            bool isSamePlayer = existing.PlayerId == updatedData.PlayerId;

            existing.PlayerId = updatedData.PlayerId;
            existing.PlayerGoals = updatedData.PlayerGoals;

            if (IsUnique(existing.MatchId, existing.PlayerId) || isSamePlayer)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(existing);
                        await _context.SaveChangesAsync();
                        return RedirectToMatchView(existing.Match);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PlayersInMatchExists(existing.Id))
                            return NotFound();
                        throw;
                    }
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Цей гравець вже бере участь в цьому матчі!";
            }

            var match = await GetMatchAsync(existing.MatchId);
            if (match == null) return NotFound();

            ViewData["PlayerId"] = GetPlayersSelectList(match.Team1Id, match.Team2Id);
            ViewData["MatchId"] = new SelectList(_context.Matches, "Id", "Id", match.Id);

            return View(existing);
        }


        // GET: PlayersInMatches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playersInMatch = await _context.PlayersInMatches
                .Include(p => p.Match)
                .Include(p => p.Player)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (playersInMatch == null)
            {
                return NotFound();
            }

            return View(playersInMatch);
        }

        // POST: PlayersInMatches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playersInMatch = await _context.PlayersInMatches.FindAsync(id);
            _context.PlayersInMatches.Remove(playersInMatch);
            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Index", "PlayersInMatches", new
            {
                id = playersInMatch.MatchId,
                team1Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team1Id,
                team2Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team2Id,
                championsipId = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().ChampionshipId,
                date = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Date
            });
        }

        private bool PlayersInMatchExists(int id)
        {
            return _context.PlayersInMatches.Any(e => e.Id == id);
        }
    }
}
