using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.Models;
using ACC_DEV.Data;

namespace ACC_DEV.Controllers
{
    public class RefLastNumbersController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;

        public RefLastNumbersController(FtlcolomboAccountsContext context)
        {
            _context = context;
        }

        // GET: RefLastNumbers
        public async Task<IActionResult> Index()
        {
              return View(await _context.RefLastNumbers.ToListAsync());
        }

        // GET: RefLastNumbers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.RefLastNumbers == null)
            {
                return NotFound();
            }

            var refPackage = await _context.RefLastNumbers
                .FirstOrDefaultAsync(m => m.TableID == id);
            if (refPackage == null)
            {
                return NotFound();
            }

            return View(refPackage);
        }

        // GET: RefLastNumbers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RefLastNumbers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TableID, LastNumber")] RefLastNumber refLastNumber)
        {
            if (ModelState.IsValid)
            {
                _context.Add(refLastNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(refLastNumber);
        }

        // GET: RefLastNumbers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.RefLastNumbers == null)
            {
                return NotFound();
            }

            var refLastNumber = await _context.RefLastNumbers.FindAsync(id);
            if (refLastNumber == null)
            {
                return NotFound();
            }
            return View(refLastNumber);
        }

        // POST: RefLastNumbers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TableID, LastNumber")] RefLastNumber refLastNumber)
        {
            if (id != refLastNumber.TableID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(refLastNumber);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RefLastNumberExists(refLastNumber.TableID))
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
            return View(refLastNumber);
        }

        // GET: RefLastNumbers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.RefLastNumbers == null)
            {
                return NotFound();
            }

            var refLastNumber = await _context.RefLastNumbers
                .FirstOrDefaultAsync(m => m.TableID == id);
            if (refLastNumber == null)
            {
                return NotFound();
            }

            return View(refLastNumber);
        }

        // POST: RefLastNumbers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.RefLastNumbers == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.RefLastNumbers'  is null.");
            }
            var refLastNumber = await _context.RefLastNumbers.FindAsync(id);
            if (refLastNumber != null)
            {
                _context.RefLastNumbers.Remove(refLastNumber);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RefLastNumberExists(string id)
        {
          return _context.RefLastNumbers.Any(e => e.TableID == id);
        }
    }
}
