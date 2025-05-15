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
    public class ChampionshipsController : Controller
    {
        private readonly DBFootballStatContext _context;

        public ChampionshipsController(DBFootballStatContext context)
        {
            _context = context;
        }

        // GET: Championships
        public async Task<IActionResult> Index()
        {
            var dBFootballStatContext = _context.Championships.Include(c => c.Country);
            return View(await dBFootballStatContext.ToListAsync());
        }

        // GET: Championships/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var champ = (from Championships in _context.Championships
                         where Championships.Id == id
                         select Championships).Include(x => x.Matches).ThenInclude(x => x.Team1).Include(x => x.Matches).ThenInclude(x => x.Team2).FirstOrDefault();
            List<Team> teams = new List<Team>();
            foreach (var m in champ.Matches)
            {
                teams.Add(m.Team1);
                teams.Add(m.Team2);
            }
            ViewBag.Teams = teams.Distinct();
            ViewBag.Championship = champ;
            return View();
        }

        // GET: Championships/Matches/5
        public async Task<IActionResult> Matches(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var matchesByChampionship = _context.Matches.Where(b => b.ChampionshipId == id).Include(m => m.Championship).Include(b => b.Team1).Include(x => x.Team2);
            var champ = (from Championships in _context.Championships
                         where Championships.Id == id
                         select Championships).FirstOrDefault();

            ViewBag.Championship = champ;
            return View(await matchesByChampionship.ToListAsync());
        }

        // GET: Championships/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name");
            return View();
        }

        // POST: Championships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CountryId,Name")] Championship championship)
        {
            if (IsUnique(championship.Name, championship.CountryId)) 
            {
                if (ModelState.IsValid)
                {
                    _context.Add(championship);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                } 
            }
            else
            {
                ViewData["ErrorMessage"] = "Чемпіонат з такою назвою в обраній країні вже існує!";
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", championship.CountryId);
            return View(championship);
        }

        bool IsUnique(string name, int countryId)
        {
            var q = (from championship in _context.Championships
                     where championship.Name == name && championship.CountryId == countryId
                     select championship).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        // GET: Championships/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var championship = await _context.Championships.FindAsync(id);
            if (championship == null)
            {
                return NotFound();
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", championship.CountryId);
            return View(championship);
        }

        // POST: Championships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CountryId,Name")] Championship championship)
        {
            if (id != championship.Id)
            {
                return NotFound();
            }

            if (IsUnique(championship.Name, championship.CountryId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(championship);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ChampionshipExists(championship.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Чемпіонат з такою назвою в обраній країні вже існує!";
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", championship.CountryId);
            return View(championship);
        }

        // GET: Championships/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var championship = await _context.Championships
                .Include(c => c.Country)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (championship == null)
            {
                return NotFound();
            }

            return View(championship);
        }

        // POST: Championships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var championship =  _context.Championships.Where(m => m.Id == id).Include(m => m.Country).FirstOrDefault();
            int count_matches = _context.Matches.Where(m => m.ChampionshipId == id).Count();
            if (count_matches != 0)
            {
                ViewData["ErrorMessage"] = "Видалення не можливе, бо в чемпіонаті є матчі!";
                return View(championship);
            }
            else
            {
                _context.Championships.Remove(championship);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ChampionshipExists(int id)
        {
            return _context.Championships.Any(e => e.Id == id);
        }
    }
}
