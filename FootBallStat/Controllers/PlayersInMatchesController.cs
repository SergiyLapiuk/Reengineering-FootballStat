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

        // GET: PlayersInMatches
        public async Task<IActionResult> Index(int? id, int? team1Id, int? team2Id, int? championshipId, DateTime? date)
        {
            if (id == null) return RedirectToAction("Matches", "Index");
            ViewBag.MatchId = id;
            ViewBag.MatchTeam1Id = team1Id;
            ViewBag.MatchTeam2Id = team2Id;
            ViewBag.MatchChampionshipId = championshipId;
            ViewBag.MatchDate = date;
            var playersInMatchesByMatch = _context.PlayersInMatches.Where(b => b.MatchId == id).Include(b => b.Match).Include(x => x.Player).Include(b => b.Match.Team1)
                .Include(b => b.Match.Team2).Include(b => b.Player.Team);

            return View(await playersInMatchesByMatch.ToListAsync());
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
            ViewBag.MatchId = matchId;
            ViewBag.MatchTeam1Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team1Id;
            ViewBag.MatchTeam2Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team2Id;
            ViewBag.MatchChampionsipId = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().ChampionshipId;
            ViewBag.MatchDate = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Date;
            
            int team1Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team1Id;
            int team2Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team2Id;

            ViewData["PlayerId"] = new SelectList(_context.Players.Where(c => c.TeamId == team1Id || c.TeamId == team2Id), "Id", "Name");
            return View();
        }

        // POST: PlayersInMatches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int matchId, [Bind("Id,PlayerId,MatchId,PlayerGoals")] PlayersInMatch playersInMatch)
        {
            playersInMatch.MatchId = matchId;

            if (IsUnique(playersInMatch.MatchId, playersInMatch.PlayerId))
            {
                if (ModelState.IsValid)
                {
                    _context.Add(playersInMatch);
                    await _context.SaveChangesAsync();
                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction("Index", "PlayersInMatches", new
                    {
                        id = matchId,
                        team1Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team1Id,
                        team2Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team2Id,
                        championsipId = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().ChampionshipId,
                        date = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Date
                    });
                } 
            }
            else
            {
                ViewData["ErrorMessage"] = "Цей гравець вже бере участь в цьому матчі!";
                int team1Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team1Id;
                int team2Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team2Id;
                ViewData["PlayerId"] = new SelectList(_context.Players.Where(c => c.TeamId == team1Id || c.TeamId == team2Id), "Id", "Name");
                ViewBag.MatchId = matchId;
                return View(playersInMatch);

            }
            //ViewData["MatchId"] = new SelectList(_context.Matches, "Id", "Id", playersInMatch.MatchId);
            //ViewData["PlayerId"] = new SelectList(_context.Players, "Id", "Id", playersInMatch.PlayerId);
            //return View(playersInMatch);

            return RedirectToAction("Index", "PlayersInMatches", new
            {
                id = matchId,
                team1Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team1Id,
                team2Id = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Team2Id,
                championsipId = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().ChampionshipId,
                date = _context.Matches.Where(c => c.Id == matchId).FirstOrDefault().Date
            });
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PlayerId,MatchId,PlayerGoals")] PlayersInMatch playersInMatch)
        {
            var p = _context.PlayersInMatches.Where(b => b.Id == id).Include(b => b.Match).Include(x => x.Player).Include(b => b.Match.Team1).Include(b => b.Match.Team2).Include(b => b.Player.Team).FirstOrDefault();
            bool flag = false;
            if (p.PlayerId == playersInMatch.PlayerId)
                flag = true;

            p.PlayerId = playersInMatch.PlayerId;
            p.PlayerGoals = playersInMatch.PlayerGoals;

            playersInMatch = p;

            if (id != playersInMatch.Id)
            {
                return NotFound();
            }

            if (IsUnique(playersInMatch.MatchId, playersInMatch.PlayerId) || flag) 
            { 
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(playersInMatch);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PlayersInMatchExists(playersInMatch.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
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
            }
            else
            {
                ViewData["ErrorMessage"] = "Цей гравець вже бере участь в цьому матчі!";
                int team1Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team1Id;
                int team2Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team2Id;
                ViewData["PlayerId"] = new SelectList(_context.Players.Where(c => c.TeamId == team1Id || c.TeamId == team2Id), "Id", "Name");
                ViewData["MatchId"] = new SelectList(_context.Matches, "Id", "Id", playersInMatch.MatchId);
                //ViewBag.MatchId = matchId;

            }
            return View(playersInMatch);
            //ViewData["MatchId"] = new SelectList(_context.Matches, "Id", "Id", playersInMatch.MatchId);
            //ViewData["PlayerId"] = new SelectList(_context.Players, "Id", "Name", playersInMatch.PlayerId);
            //return View(playersInMatch);
            //return RedirectToAction("Index", "PlayersInMatches", new
            //{
            //    id = playersInMatch.MatchId,
            //    team1Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team1Id,
            //    team2Id = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Team2Id,
            //    championsipId = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().ChampionshipId,
            //    date = _context.Matches.Where(c => c.Id == playersInMatch.MatchId).FirstOrDefault().Date
            //});
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
