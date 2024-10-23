using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.Models;
using ACC_DEV.ViewModel;
using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using ACC_DEV.Data;

namespace ACC_DEV.Views
{
    public class TxnJournalHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
      

        public TxnJournalHdsController(FtlcolomboAccountsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnJournalHD = from p in _context.TxnJournalHD select p;

            if (!String.IsNullOrEmpty(searchString))
            {

                txnJournalHD = txnJournalHD.Where(txnJournalHD => txnJournalHD.JournalNo.Contains(searchString));
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnJournalHD.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnJournalHD.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }

        // GET: TxnImportJobDtls
        public async Task<IActionResult> Create(string searchString, int pg = 1)
        {
            var tables = new AccountsViewModel
            {
                TxnJournalHdMulti = _context.TxnJournalHD.Where(t => t.JournalNo == "xxx"),
                TxnJournalDtlMulti = _context.TxnJournalsDTL.Where(t => t.JournalNo == "xxx"),
            };

            ViewData["AccountsCodes"] = new SelectList(
                                                        _context.Set<RefChartOfAcc>()
                                                            .Where(a => a.IsInactive.Equals(false))
                                                            .OrderBy(p => p.AccCode)
                                                            .Select(a => new { AccNo = a.AccNo, DisplayValue = $"{a.AccCode} - {a.Description}" }),
                                                        "AccNo",
                                                        "DisplayValue",
                                                        "AccNo"
                                                        );

            return View(tables);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("JournalNo,Date,RefNo,Note,IsAdjustmentEntry,IsJobRelated,JobType,JobNo,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason")] TxnJournalHd txnJournalHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_JournalHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "JLN" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnJournalHD.JournalNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }
            txnJournalHD.Canceled = false;
            txnJournalHD.CreatedBy = "Admin";
            txnJournalHD.CreatedDateTime = DateTime.Now;

            ModelState.Remove("JournalNo");  // Auto genrated
            if (ModelState.IsValid)
            {
                // Adding CargoBreakDown records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnJournalDtl>>(dtlItemsList);
                    if (DtailItemdataTable != null)
                    {
                        foreach (var item in DtailItemdataTable)
                        {
                            if (item.SerialNo != 0)
                            {
                                TxnJournalDtl DetailItem = new TxnJournalDtl();
                                DetailItem.JournalNo = nextRefNo;
                                DetailItem.SerialNo = item.SerialNo;
                                DetailItem.AccNo = item.AccNo;
                                DetailItem.Description = item.Description;
                                DetailItem.Debit = item.Debit;
                                DetailItem.Credit = item.Credit;
                                _context.TxnJournalsDTL.Add(DetailItem);
                            }
                        }

                    }
                }
            


                _context.Add(txnJournalHD);
                _context.RefLastNumbers.Update(refLastNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var tables = new AccountsViewModel
            {
                TxnJournalHdMulti = _context.TxnJournalHD.Where(t => t.JournalNo == "xxx"),
                TxnJournalDtlMulti = _context.TxnJournalsDTL.Where(t => t.JournalNo == "xxx"),
            };


            return View(tables);
        }


    }
}
