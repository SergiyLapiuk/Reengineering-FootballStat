using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallStat;

namespace FootBallStat.Controllers
{
    public class PlayersController : Controller
    {
        private readonly DBFootballStatContext _context;

        public PlayersController(DBFootballStatContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id, string? name)
        {
            if (id == null) return RedirectToAction("Teams", "Index");
            name ??= _context.Teams.FirstOrDefault(x => x.Id == id)?.Name;
            ViewBag.TeamId = id;
            ViewBag.TeamName = name;
            var playersByTeam = _context.Players
                .Where(b => b.TeamId == id)
                .Include(b => b.Team)
                .Include(x => x.Position);
            return View(await playersByTeam.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);

            return player == null ? NotFound() : View(player);
        }

        public IActionResult Create(int teamId)
        {
            PopulatePlayerDropdowns(teamId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int teamId, [Bind("Id,TeamId,PositionId,DateOfBirth,Name,Number")] Player player)
        {
            player.TeamId = teamId;
            var validationCode = IsUnique(player.Name, player.Number, player.TeamId, player.DateOfBirth);
            if (validationCode == 1 && ModelState.IsValid)
            {
                _context.Add(player);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Players", new { id = teamId, name = _context.Teams.Find(teamId)?.Name });
            }

            ViewData["ErrorMessage"] = validationCode switch
            {
                2 => "Цей гравець вже доданий!",
                3 => "Номер в цій команді вже зайнятий!",
                _ => null
            };

            PopulatePlayerDropdowns(teamId, player.PositionId);
            return View(player);
        }

        private int IsUnique(string name, int number, int teamId, DateTime dateOfBirth)
        {
            var playerExists = _context.Players.Any(p => p.Name == name && p.DateOfBirth.Date == dateOfBirth.Date);
            var numberTaken = _context.Players.Any(p => p.Number == number && p.TeamId == teamId);
            return playerExists ? 2 : numberTaken ? 3 : 1;
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();
            PopulatePlayerDropdowns(player.TeamId, player.PositionId);
            return View(player);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeamId,PositionId,DateOfBirth,Name,Number")] Player player)
        {
            var existingPlayer = _context.Players
                .Include(p => p.Position)
                .Include(p => p.Team)
                .FirstOrDefault(p => p.Id == id);
            if (existingPlayer == null || id != player.Id) return NotFound();

            bool numberUnchanged = existingPlayer.Number == player.Number && existingPlayer.TeamId == player.TeamId;
            if (!IsUniqueEdit(player.Number, player.TeamId) && !numberUnchanged)
            {
                ViewData["ErrorMessage"] = "Номер в цій команді вже зайнятий!";
                PopulatePlayerDropdowns(player.TeamId, player.PositionId);
                return View(player);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingPlayer.TeamId = player.TeamId;
                    existingPlayer.PositionId = player.PositionId;
                    existingPlayer.Name = player.Name;
                    existingPlayer.Number = player.Number;
                    existingPlayer.DateOfBirth = player.DateOfBirth;
                    _context.Update(existingPlayer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Players", new { id = existingPlayer.TeamId, name = _context.Teams.Find(existingPlayer.TeamId)?.Name });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.Id)) return NotFound();
                    throw;
                }
            }

            PopulatePlayerDropdowns(player.TeamId, player.PositionId);
            return View(player);
        }

        private bool IsUniqueEdit(int number, int teamId)
        {
            return !_context.Players.Any(p => p.Number == number && p.TeamId == teamId);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Team)
                .FirstOrDefaultAsync(m => m.Id == id);

            return player == null ? NotFound() : View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null) return NotFound();

            var matches = _context.PlayersInMatches.Where(p => p.PlayerId == id);
            _context.PlayersInMatches.RemoveRange(matches);

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Players", new { id = player.TeamId, name = _context.Teams.Find(player.TeamId)?.Name });
        }

        private bool PlayerExists(int id) => _context.Players.Any(e => e.Id == id);

        private void PopulatePlayerDropdowns(int teamId, int? selectedPositionId = null)
        {
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", selectedPositionId);
            ViewBag.TeamId = teamId;
            ViewBag.TeamName = _context.Teams.FirstOrDefault(t => t.Id == teamId)?.Name;
        }
    }
}
