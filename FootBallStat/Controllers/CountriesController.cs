using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FootBallStat;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;

namespace FootBallStat.Controllers
{
    public class CountriesController : Controller
    {
        private readonly DBFootballStatContext _context;

        public CountriesController(DBFootballStatContext context)
        {
            _context = context;
        }

        // GET: Countries
        public async Task<IActionResult> Index(string? f)
        {
            ViewBag.f = f;
            return View(await _context.Countries.ToListAsync());
        }

        // GET: Countries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.Country = (from Countries in _context.Countries
                               where Countries.Id == id
                               select Countries).Include(x => x.Championships).FirstOrDefault();

            return View();
        }

        private void SetDuplicateCountryMessage()
        {
            ViewData["ErrorMessage"] = "Така країна вже додана!";
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Country country)
        {
            if (IsUnique(country.Name))
            {
                if (ModelState.IsValid)
                {
                    _context.Add(country);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                SetDuplicateCountryMessage();
            }
            return View(country);
        }

        bool IsUnique(string name)
        {
            var q = (from country in _context.Countries
                     where country.Name == name
                     select country).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        // GET: Countries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        // POST: Countries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        private async Task<bool> TryUpdateCountryAsync(Country country)
        {
            try
            {
                _context.Update(country);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return CountryExists(country.Id);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Country country)
        {
            if (id != country.Id)
                return NotFound();

            if (!IsUnique(country.Name))
            {
                SetDuplicateCountryMessage();
                return View(country);
            }

            if (ModelState.IsValid && await TryUpdateCountryAsync(country))
                return RedirectToAction(nameof(Index));

            return View(country);
        }


        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (country == null)
            {
                return NotFound();
            }

            return View(country);
        }

        private bool HasChampionships(int countryId)
        {
            return _context.Championships.Any(c => c.CountryId == countryId);
        }


        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (HasChampionships(id))
            {
                ViewData["ErrorMessage"] = "Видалення не можливе!";
                return View(country);
            }
            else
            {
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }

        private IActionResult ReturnError(string reason)
        {
            return RedirectToAction("Index", "Countries", new { f = reason });
        }

        private string? ProcessWorksheet(IXLWorksheet worksheet)
        {
            var c = _context.Countries.Where(x => x.Name.Contains(worksheet.Name)).ToList();
            var newcoun = c.FirstOrDefault() ?? new Country { Name = worksheet.Name };
            if (c.Count == 0) _context.Countries.Add(newcoun);

            return ImportChampionshipsFromWorksheet(worksheet, newcoun);
        }

        private string? ImportChampionshipsFromWorksheet(IXLWorksheet worksheet, Country newcoun)
        {
            var championshipsToAdd = new List<Championship>();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var name = row.Cell(1).GetValue<string>().Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                if (!IsUniqueChamp(name, newcoun.Id) &&
                    championshipsToAdd.Any(c => c.Name == name && c.CountryId == newcoun.Id))
                    continue;

                var championship = new Championship
                {
                    Name = name,
                    Country = newcoun
                };

                championshipsToAdd.Add(championship);
                _context.Championships.Add(championship);
            }

            return null;
        }


        private string? ImportWorksheets(XLWorkbook workBook)
        {
            foreach (var worksheet in workBook.Worksheets)
            {
                var result = ProcessWorksheet(worksheet);
                if (result != null) return result;
            }
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            if (fileExcel == null || fileExcel.Length == 0)
                return ReturnError("Файл не прикріплений або порожній");

            using var stream = new FileStream(fileExcel.FileName, FileMode.Create);
            await fileExcel.CopyToAsync(stream);

            try
            {
                using var workBook = new XLWorkbook(stream, XLEventTracking.Disabled);
                var result = ImportWorksheets(workBook);
                if (result != null)
                    return ReturnError(result);
            }
            catch
            {
                return ReturnError("Некоректні дані");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        bool IsUniqueChamp(string name, int countryId)
        {
            var q = (from championship in _context.Championships
                     where championship.Name == name && championship.CountryId == countryId
                     select championship).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        bool IsUniqueChampFile(List<Championship> championships, string name, int countryId)
        {
            var q = (from championship in championships
                     where championship.Name == name && championship.CountryId == countryId
                     select championship).ToList();
            if (q.Count == 0) { return true; }
            return false;
        }

        public ActionResult Export()
        {
            using (XLWorkbook workbook = Exporting())
            {
                
                using(var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"footbollstat_{DateTime.UtcNow.ToLongDateString()}.xlsx"
                    };
                }
            }
        }

        public XLWorkbook Exporting()
        {
            XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled);
            var countries = _context.Countries.Include("Championships").ToList();
            foreach (var c in countries)
            {
                var worksheet = workbook.Worksheets.Add(c.Name);
                worksheet.Cells("A1").Value = "Назва чемпіонату";
                worksheet.Column("A").Width = 25;
                worksheet.Row(1).Style.Font.Bold = true;
                var championships = c.Championships.ToList();

                for (int i = 0; i < championships.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = championships[i].Name;
                }
            }
            return workbook;
        }
    }
}




