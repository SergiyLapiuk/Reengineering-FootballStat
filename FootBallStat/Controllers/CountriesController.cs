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

            //var country = await _context.Countries
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (country == null)
            //{
            //    return NotFound();
            //}

            ViewBag.Country = (from Countries in _context.Countries
                               where Countries.Id == id
                               select Countries).Include(x => x.Championships).FirstOrDefault();

            return View();
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                ViewData["ErrorMessage"] = "Така країна вже додана!";
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Country country)
        {
            if (id != country.Id)
            {
                return NotFound();
            }

            if (IsUnique(country.Name))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(country);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CountryExists(country.Id))
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
                ViewData["ErrorMessage"] = "Така країна вже додана!";
            }
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

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            var count_champ = _context.Championships.Where(b => b.CountryId == id).Count();
            if (count_champ != 0)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel)
        {
            if(ModelState.IsValid)
            {
                if (fileExcel != null)
                {
                    var stream = new FileStream(fileExcel.FileName, FileMode.Create);
                    await fileExcel.CopyToAsync(stream);
                    try {
                        XLWorkbook workBook = new XLWorkbook(stream, XLEventTracking.Disabled);
                        foreach (IXLWorksheet worksheet in workBook.Worksheets)
                        {
                            Country newcoun;
                            var c = (from coun in _context.Countries
                                     where coun.Name.Contains(worksheet.Name)
                                     select coun).ToList();
                            if (c.Count > 0)
                            {
                                newcoun = c[0];
                            }
                            else
                            {
                                newcoun = new Country();
                                newcoun.Name = worksheet.Name;
                                _context.Countries.Add(newcoun);
                            }
                            var ex = AllRows(worksheet, newcoun);
                            if(ex != null)
                            {
                                workBook.Dispose();
                                stream.Dispose();
                                return RedirectToAction("Index", "Countries", new { f = "Некоректні дані" });
                            }
                        }
                        workBook.Dispose();
                        stream.Dispose();
                    }
                    catch
                    {
                        return RedirectToAction("Index", "Countries", new { f = "Некоректні дані" });
                    }           
                }
                else
                {
                    return RedirectToAction("Index", "Countries", new { f = "Не прикріплений файл" });
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public string? AllRows(IXLWorksheet worksheet, Country newcoun)
        {
                var champfil = new List<Championship>();
                foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
                {
                    try
                    {
                        Championship championship = new Championship();
                        championship.Name = row.Cell(1).Value.ToString();
                        championship.Country = newcoun;
                        //champfil.Add(championship);
                        if (IsUniqueChamp(championship.Name, newcoun.Id) && IsUniqueChampFile(champfil, championship.Name, newcoun.Id))
                        {
                            _context.Championships.Add(championship);
                            champfil.Add(championship);
                        }

                    }
                    catch (Exception e)
                    {
                        return "Не коректні дані.";
                    }
                }
            return null;
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




