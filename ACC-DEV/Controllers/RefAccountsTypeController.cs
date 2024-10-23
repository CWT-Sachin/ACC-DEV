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
    public class RefAccountsTypeController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;

        public RefAccountsTypeController(FtlcolomboAccountsContext context)
        {
            _context = context;
        }

        // GET: RefChargeItems
        public async Task<IActionResult> Index()
        {
              return _context.AccountTypes != null ? 
                          View(await _context.AccountTypes.ToListAsync()) :
                          Problem("Entity set 'FtlcolombOperationContext.AccountTypes'  is null.");
        }

        // GET: RefChargeItems/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.AccountTypes == null)
            {
                return NotFound();
            }

            var accountType = await _context.AccountTypes
                .FirstOrDefaultAsync(m => m.AccTypeID == id);
            if (accountType == null)
            {
                return NotFound();
            }

            return View(accountType);
        }

        // GET: RefChargeItems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RefChargeItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccTypeID,AccTypeName,IsActive,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime")] RefAccountType accountType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(accountType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(accountType);
        }

        // GET: RefChargeItems/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.AccountTypes == null)
            {
                return NotFound();
            }

            var accountType = await _context.AccountTypes.FindAsync(id);
            if (accountType == null)
            {
                return NotFound();
            }
            return View(accountType);
        }

        // POST: RefChargeItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("AccTypeID,AccTypeName,IsActive,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime")] RefAccountType accountType)
        {
            if (id != accountType.AccTypeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(accountType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!accountTypeExists(accountType.AccTypeID))
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
            return View(accountType);
        }

        // GET: RefChargeItems/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.AccountTypes == null)
            {
                return NotFound();
            }

            var accountType = await _context.AccountTypes
                .FirstOrDefaultAsync(m => m.AccTypeID == id);
            if (accountType == null)
            {
                return NotFound();
            }

            return View(accountType);
        }

        // POST: RefChargeItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.AccountTypes == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.accountType'  is null.");
            }
            var accountType = await _context.AccountTypes.FindAsync(id);
            if (accountType != null)
            {
                _context.AccountTypes.Remove(accountType);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool accountTypeExists(string id)
        {
          return (_context.AccountTypes?.Any(e => e.AccTypeID == id)).GetValueOrDefault();
        }
    }
}
