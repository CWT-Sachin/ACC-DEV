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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ACC_DEV.Views
{
    //[Authorize]
    public class TxnJournalHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        //private readonly UserManager<DefaultUser> _userManager;

        //public TxnJournalHdsController(FtlcolomboAccountsContext context, UserManager<DefaultUser> userManager)

        public TxnJournalHdsController(FtlcolomboAccountsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            //var txnJournalHD = from p in _context.TxnJournalHD select p;
            var txnJournalHD = _context.TxnJournalHD.OrderByDescending(t=> t.JournalNo);

            if (!String.IsNullOrEmpty(searchString))
            {
                txnJournalHD = txnJournalHD.Where(txnJournalHD => txnJournalHD.JournalNo.Contains(searchString)).OrderByDescending(t=> t.JournalNo);
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
                        var SerialNo_AccTxn = 0;
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
                _context.RefLastNumbers.Update(refLastNumber); // Jornal Last Number 
                //_context.RefLastNumbers.Update(refLastNumberAccTxn); // Transaction last number

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

        // GET: TxnImportJobDtls
        public async Task<IActionResult> Details(string id)
        {
            var tables = new AccountsViewModel
            {
                TxnJournalHdMulti = _context.TxnJournalHD.Where(t => t.JournalNo == id),
                TxnJournalDtlMulti = _context.TxnJournalsDTL.Where(t => t.JournalNo == id),
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


        // GET: /TxnJournalHds/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the TxnJournalHD record to edit
            var txnJournalHd = await _context.TxnJournalHD.FindAsync(id);
            if (txnJournalHd == null)
            {
                return NotFound();
            }

            // Retrieve related TxnJournalsDTL records
            var txnJournalDtls = await _context.TxnJournalsDTL.Where(t => t.JournalNo == id).ToListAsync();

            var tables = new AccountsViewModel
            {
                TxnJournalHdMulti = await _context.TxnJournalHD.Where(t => t.JournalNo == id).ToListAsync(),
                TxnJournalDtlMulti = txnJournalDtls,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string dtlItemsList, string id, [Bind("JournalNo,Date,RefNo,Note,IsAdjustmentEntry,IsJobRelated,JobType,JobNo,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason")] TxnJournalHd txnJournalHD)
        {
            if (id != txnJournalHD.JournalNo)
            {
                return NotFound();
            }

            txnJournalHD.LastUpdatedBy = "Admin";
            txnJournalHD.LastUpdatedDateTime = DateTime.Now;

            if (ModelState.IsValid)
            {
                // Adding CargoBreakDown records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var rowsToDelete = _context.TxnJournalsDTL .Where(t => t.JournalNo == id);
                    if (rowsToDelete != null || rowsToDelete.Any())
                    {
                        // Remove the rows from the database context
                        _context.TxnJournalsDTL.RemoveRange(rowsToDelete);
                    }
                    var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnJournalDtl>>(dtlItemsList);
                    if (DtailItemdataTable != null)
                    {
                        var SerialNo_AccTxn = 0;
                        foreach (var item in DtailItemdataTable)
                        {
                            if (item.SerialNo != 0)
                            {
                                TxnJournalDtl DetailItem = new TxnJournalDtl();
                                DetailItem.JournalNo = id;
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

                _context.Update(txnJournalHD);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            var tables = new AccountsViewModel
            {
                TxnJournalHdMulti = await _context.TxnJournalHD.Where(t => t.JournalNo == id).ToListAsync(),
                TxnJournalDtlMulti = await _context.TxnJournalsDTL.Where(t => t.JournalNo == id).ToListAsync()
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
        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnJournalHD = await _context.TxnJournalHD
                .FirstOrDefaultAsync(m => m.JournalNo == id);

            if (txnJournalHD == null)
            {
                return NotFound();
            }

            var jobNo = txnJournalHD.JournalNo; // Set the jobNo property

            //var tables = new DebitNoteViewModel
            //{
            //    DebitNoteHdMulti = _context.TxnDebitNoteHds.Where(t => t.DebitNoteNo == id),
            //    DebitNoteDtlMulti = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == id),

            //};
            var tables = new AccountsViewModel
            {
                TxnJournalHdMulti = _context.TxnJournalHD.Where(t => t.JournalNo == id),
                TxnJournalDtlMulti = _context.TxnJournalsDTL.Where(t => t.JournalNo == id),
            };

            return View(tables);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize]
        public async Task<IActionResult> Approve(string id, bool approved)
        {
            // Get the current logged-in user
            string userName = "Admin"; // admin@ftl.lk 
            //var user = await _userManager.GetUserAsync(User);
            //if (user != null)
            //{
            //    // Access the user's ID
            //    string userID = user.Id;
            //    userName = user.UserName; // admin@ftl.lk 
            //    string userFirstName = user.FirstName; // Nishantha 
            //    string userLastName = user.LastName;  // Bopearachchi
            //}

            if (id == null)
            {
                return NotFound();
            }

            var txnJournalHD = await _context.TxnJournalHD
                .FirstOrDefaultAsync(m => m.JournalNo == id);

            if (txnJournalHD == null)
            {
                return NotFound();
            }

            // Inserting Transaction Data to Acccounts 

            var txnJournalsDTL = await _context.TxnJournalsDTL
                .Where(m => m.JournalNo == id)
                .ToListAsync();

            var JobNo ="";
            var JobType = "";
            if (txnJournalHD.IsJobRelated)
            {
                JobNo = txnJournalHD.JobNo;
                JobType = txnJournalHD.JobType;
            }

            // Next RefNumber for Txn_Transactions
            var nextAccTxnNo = "";
            var TableIDAccTxn = "Txn_Transactions";
            var refLastNumberAccTxn = await _context.RefLastNumbers.FindAsync(TableIDAccTxn);
            if (refLastNumberAccTxn != null)
            {
                var nextNumberAccTxn = refLastNumberAccTxn.LastNumber + 1;
                refLastNumberAccTxn.LastNumber = nextNumberAccTxn;
                nextAccTxnNo = "TXN" + DateTime.Now.Year.ToString() + nextNumberAccTxn.ToString().PadLeft(5, '0');
                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }
            if (txnJournalsDTL != null)
            {
                var SerialNo_AccTxn = 0;

                foreach (var item in txnJournalsDTL)
                {
                    // Transaction table Insert 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();

                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnJournalHD.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = item.Description;
                    NewRowAccTxn.TxnAccCode = item.AccNo;
                    NewRowAccTxn.Dr = (decimal)item.Debit;
                    NewRowAccTxn.Cr = (decimal)item.Credit;
                    
                    NewRowAccTxn.RefNo = txnJournalHD.RefNo; // RefNo No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "Journal";
                    NewRowAccTxn.IsMonthEndDone = false;
                    NewRowAccTxn.CreatedBy = userName;
                    NewRowAccTxn.CreatedDateTime = DateTime.Now;
                    NewRowAccTxn.Canceled = false;
                    NewRowAccTxn.JobNo = JobNo;
                    NewRowAccTxn.JobType = JobType;

                    _context.TxnTransactions.Add(NewRowAccTxn);
                }
            }
            
            // END Inserting Transaction Data to Acccounts 

            /// Update the Approved property based on the form submission
            txnJournalHD.Approved = approved;
            txnJournalHD.ApprovedBy = userName; // Replace with the actual user name
            txnJournalHD.ApprovedDateTime = DateTime.Now;

            _context.Update(txnJournalHD);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }


    }
}
