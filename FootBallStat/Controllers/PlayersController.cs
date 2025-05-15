using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallStat;
using ClosedXML.Excel;

namespace FootBallStat.Controllers
{
    public class PlayersController : Controller
    {
        private readonly DBFootballStatContext _context;

        public PlayersController(DBFootballStatContext context)
        {
            _context = context;
        }

        // GET: Players
        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null) return RedirectToAction("Teams", "Index");
            if(name == null)
                name = _context.Teams.Where(x => x.Id == id).FirstOrDefault().Name;
            ViewBag.TeamId = id;
            ViewBag.TeamName = name;
            var playersByTeam = _context.Players.Where(b => b.TeamId == id).Include(b => b.Team).Include(x => x.Position);

            return View(await playersByTeam.ToListAsync());
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // GET: Players/Create
        public IActionResult Create(int teamId)
        {
            ViewBag.TeamId = teamId;
            ViewBag.TeamName = _context.Teams.Where(c => c.Id == teamId).FirstOrDefault().Name;
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");
            return View();
        }

        // POST: Players/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int teamId, [Bind("Id,TeamId,PositionId,DateOfBirth,Name,Number")] Player player)
        {
            player.TeamId = teamId;
            if (IsUnique(player.Name, player.Number, player.TeamId, player.DateOfBirth) == 1)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(player);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Players", new { id = teamId, name = _context.Teams.Where(c => c.Id == teamId).FirstOrDefault().Name });
                }
            }
            else if(IsUnique(player.Name, player.Number, player.TeamId, player.DateOfBirth) == 2)
            {
                ViewData["ErrorMessage"] = "Цей гравець вже доданий!";
                ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");
                ViewBag.TeamId = teamId;
                ViewBag.TeamName = _context.Teams.Where(c => c.Id == teamId).FirstOrDefault().Name;
                return View(player);
            }
            else if (IsUnique(player.Name, player.Number, player.TeamId, player.DateOfBirth) == 3)
            {
                ViewData["ErrorMessage"] = "Номер в цій команді вже зайнятий!";
                ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");
                ViewBag.TeamId = teamId;
                ViewBag.TeamName = _context.Teams.Where(c => c.Id == teamId).FirstOrDefault().Name;
                return View(player);
            }
            //return RedirectToAction("Index", "Players", new { id = teamId, name = _context.Teams.Where(c => c.Id == teamId).FirstOrDefault().Name });
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");
            ViewBag.TeamId = teamId;
            ViewBag.TeamName = _context.Teams.Where(c => c.Id == teamId).FirstOrDefault().Name;
            return View(player);
        }

        int IsUnique(string name, int number, int teamId, DateTime dateOfBirth)
        {
            var q = (from player in _context.Players
                     where player.Name == name && player.DateOfBirth.Date == dateOfBirth.Date
                     select player).ToList();
            
            var g = (from player in _context.Players
                     where player.Number == number && player.TeamId == teamId
                     select player).ToList();
            if (q.Count == 0 && g.Count == 0) return 1;
            if (q.Count != 0) return 2;
            return 3;
        }

        // GET: Players/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", player.PositionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Name", player.TeamId);
            return View(player);
        }

        // POST: Players/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeamId,PositionId,DateOfBirth,Name,Number")] Player player)
        {
            var p =  _context.Players.Where(m => m.Id == id).Include(m => m.Position).Include(m => m.Team).Include(m => m.PlayersInMatches).FirstOrDefault();
            bool number = p.Number == player.Number;
            bool team = p.TeamId == player.TeamId;
            p.TeamId = player.TeamId;
            p.PositionId = player.PositionId;
            p.Name = player.Name;
            p.Number = player.Number;
            p.DateOfBirth = player.DateOfBirth;
            player = p;
            if (id != player.Id)
            {
                return NotFound();
            }

            if (IsUniqueEdit(player.Number, player.TeamId) || (number && team)) 
            { 
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(player);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PlayerExists(player.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction("Index", "Players", new { id = player.TeamId, name = _context.Teams.Where(c => c.Id == player.TeamId).FirstOrDefault().Name });
                } 
            }
            else
            {
                ViewData["ErrorMessage"] = "Номер в цій команді вже зайнятий!";
                ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", player.PositionId);
                ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Name", player.TeamId);
                //var p = _context.Players.Where(x => x.Id == id).Include(x => x.Team).FirstOrDefault();
                
                return View(player);
            }
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", player.PositionId);
            ViewData["TeamId"] = new SelectList(_context.Teams, "Id", "Name", player.TeamId);
            return View(player);
            //return RedirectToAction("Index", "Players", new { id = player.TeamId, name = _context.Teams.Where(c => c.Id == player.TeamId).FirstOrDefault().Name });
        }

        bool IsUniqueEdit(int number, int teamId)
        {
                var g = (from player in _context.Players
                         where player.Number == number && player.TeamId == teamId
                         select player).ToList();

                if (g.Count == 0) return true;
                
            return false; 
        }

        // GET: Players/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var player = await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (player == null)
            {
                return NotFound();
            }

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            var playersInMatches = _context.PlayersInMatches.Where(m => m.PlayerId == id);
            if (playersInMatches.Any())
            {
                foreach (var playerInMatch in playersInMatches)
                {
                    _context.PlayersInMatches.Remove(playerInMatch);
                }
            }
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Index", "Players", new { id = player.TeamId, name = _context.Teams.Where(c => c.Id == player.TeamId).FirstOrDefault().Name });
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.Id == id);
        }
    }
}
