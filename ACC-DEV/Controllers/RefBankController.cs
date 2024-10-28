using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.ViewModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ACC_DEV.Models;

using ACC_DEV.Data;
using ACC_DEV.DataOperation;
namespace SWAT_DEV.Controllers
{
    
    public class RefBankController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public RefBankController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        // GET: RefCountries
        public async Task<IActionResult> Index(string searchString, int pg = 1)
        {
            var Banks = from p in _context.RefBankAcc select p;
            Banks = Banks.OrderBy(x => x.Description);
            if (!String.IsNullOrEmpty(searchString))
            {

                Banks = Banks.Where(Banks => Banks.Description.Contains(searchString));
                Banks = Banks.OrderBy(x => x.Description);
            }
            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = Banks.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = Banks.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
            //return View(await _context.refCountry.ToListAsync());
        }

        // GET: refCountry/Detail/5
        public async Task<IActionResult> Detail(string id)
        {
            if (id == null || _context.RefBankAcc == null)
            {
                return NotFound();
            }

            var refLastNumber = await _context.RefBankAcc
                .FirstOrDefaultAsync(m => m.ID == id);
            if (refLastNumber == null)
            {
                return NotFound();
            }

            return View(refLastNumber);
        }

        // GET: RefPorts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: refCountry/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,BankCode,Description,IsActive,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime")] RefBankAcc refBanks)
        {
            var TableID = "Ref_BankAcc";  // Table ID in the Ref_Last
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                var IDNumber = "BNK" + nextNumber.ToString().PadLeft(5, '0');
                refBanks.ID = IDNumber;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return View(refBanks);
            }
            refBanks.IsActive = true;
            refBanks.CreatedBy = "Admin";
            refBanks.CreatedDateTime = DateTime.Now;

            ModelState.Remove("ID");

            if (ModelState.IsValid)
            {
                _context.Add(refBanks);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(refBanks);
        }

        // GET: refCountry/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.RefBankAcc == null)
            {
                return NotFound();
            }

            var refBanks = await _context.RefBankAcc.FindAsync(id);
            if (refBanks == null)
            {
                return NotFound();
            }
            return View(refBanks);
        }
        // POST: refCountry/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ID,BankCode,Description,IsActive,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime")] RefBankAcc refBanks)
        {
            if (id != refBanks.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(refBanks);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RefBanksNoExists(refBanks.ID))
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
            return View(refBanks);
        }

        // GET: refCountry/Delete/5

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.RefBankAcc == null)
            {
                return NotFound();
            }

            var refBanks = await _context.RefBankAcc
                .FirstOrDefaultAsync(m => m.ID == id);
            if (refBanks == null)
            {
                return NotFound();
            }

            return View(refBanks);
        }
        // POST: refCountry/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.RefBankAcc == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.RefBanks'  is null.");
            }
            var refBanks = await _context.RefBankAcc.FindAsync(id);
            if (refBanks != null)
            {
                _context.RefBankAcc.Remove(refBanks);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RefBanksNoExists(string id)
        {
            return _context.RefBankAcc.Any(e => e.ID == id);
        }
    }
}
