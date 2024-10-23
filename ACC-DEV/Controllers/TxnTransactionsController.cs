using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.Models;
using ACC_DEV.ViewModel;
using Newtonsoft.Json;
using ACC_DEV.Data;
using ACC_DEV.DataOperation;
using Microsoft.AspNetCore.Authorization;


namespace ACC_DEV.Controllers
{
    public class TxnTransactionsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;


        public string jobNo { get; private set; }


        public TxnTransactionsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1 )
        {
            IQueryable<TxnTransactions> txnTransactions = _context.TxnTransactions;

            // Apply includes
            txnTransactions = txnTransactions.Include(t => t.TxnTransactionNavigation);

            // Filter based on the search criteria
            if (!String.IsNullOrEmpty(searchString))
            {
                if (searchType == "RefNo")
                {
                    txnTransactions = txnTransactions.Where(txnInvoiceExportHds => txnInvoiceExportHds.RefNo.Contains(searchString));
                }
                else if (searchType == "DocType")
                {
                    txnTransactions = txnTransactions.Where(txnInvoiceExportHds => txnInvoiceExportHds.DocType.Contains(searchString));
                }
            }

            txnTransactions = txnTransactions.OrderByDescending(p => p.TxnNo);

            const int pageSize = 25;
            if (pg < 1)
                pg = 1;
            int recsCount = await txnTransactions.CountAsync();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data =  txnTransactions.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }


        public async Task<IActionResult> TransactionLedger(string searchString, string searchType, int pg = 1, string selectedAccount = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<TxnTransactions> txnTransactions = _context.TxnTransactions;

            var AccCode = "xxx";
            // Filter by selected account if provided
            if (!string.IsNullOrEmpty(selectedAccount))
            {
                AccCode = selectedAccount;
                //txnTransactions = txnTransactions.Where(t => t.TxnAccCode == selectedAccount);
            }
            txnTransactions = txnTransactions.Where(t => t.TxnAccCode == AccCode);

            // Filter by fromDate and toDate if provided
            if (fromDate != null && toDate != null)
            {
                txnTransactions = txnTransactions.Where(t => t.Date >= fromDate && t.Date <= toDate);
            }

            // Apply the search filter based on the selected search type
            if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(searchType))
            {
                switch (searchType)
                {
                    case "DocNo":
                        txnTransactions = txnTransactions.Where(t => t.RefNo.Contains(searchString));
                        break;
                    case "DocType":
                        txnTransactions = txnTransactions.Where(t => t.DocType.Contains(searchString));
                        break;
                    case "Description":
                        txnTransactions = txnTransactions.Where(t => t.Description.Contains(searchString));
                        break;
                    case "Dr Amount":
                        if (decimal.TryParse(searchString, out decimal drAmount))
                        {
                            txnTransactions = txnTransactions.Where(t => t.Dr == drAmount);
                        }
                        break;
                    case "Cr Amount":
                        if (decimal.TryParse(searchString, out decimal crAmount))
                        {
                            txnTransactions = txnTransactions.Where(t => t.Cr == crAmount);
                        }
                        break;
                    default:
                        break;
                }
            }

            txnTransactions = txnTransactions.OrderByDescending(p => p.TxnNo);

            // Calculate the first date of the current month
            DateTime firstDayOfMonth = DateTime.Now;

            // Pass the calculated date to the view
            ViewBag.FirstDayOfMonth = firstDayOfMonth;

            ViewBag.FromDate = fromDate.HasValue ? fromDate.Value.ToShortDateString() : null;
            ViewBag.ToDate = toDate.HasValue ? toDate.Value.ToShortDateString() : null;

            ViewData["AccountsCodes"] = new SelectList(
                _context.Set<RefChartOfAcc>()
                    .Where(a => a.IsInactive.Equals(false))
                    .OrderBy(p => p.AccCode)
                    .Select(a => new { AccNo = a.AccNo, DisplayValue = $"{a.AccCode} - {a.Description}" }),
                "AccNo",
                "DisplayValue",
                selectedAccount // Set the selected account in the dropdown
            );

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = await txnTransactions.CountAsync();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = await txnTransactions.Skip(recSkip).Take(pager.PageSize).ToListAsync();

            this.ViewBag.Pager = pager;
            return View(data);
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnTransactions = await _context.TxnTransactions.Where(t => t.TxnNo == id).ToListAsync();
            if (txnTransactions == null || !txnTransactions.Any())
            {
                return NotFound();
            }

            var tables = new TxnTransactionsViewMode
            {
                TxnTransactionHdMulti = txnTransactions
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
        // POST: RefChargeItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TxnTransactionsViewMode model)
        {
            //if (id == null || model == null || model.TxnTransactionHdMulti == null)
            if (id == null || model == null )
            {
                return NotFound();
            }
            //ModelState.Remove("TxnNo");
            //if (ModelState.IsValid)
            //{
                try
                {
                    foreach (var txnTransaction in model.TxnTransactionHdMulti)
                    {
                        // Ensure that the TxnNo matches with the id
                        if (txnTransaction.TxnNo != id)
                        {
                            return BadRequest(); // The TxnNo doesn't match the id
                        }

                        // Update each TxnTransaction in the database
                        _context.Update(txnTransaction);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index)); // Redirect to Index after successful edit
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TxnTransactionsExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            //}

            // If ModelState is not valid, reload the view with the model
            ViewData["AccountsCodes"] = new SelectList(
                _context.Set<RefChartOfAcc>()
                    .Where(a => a.IsInactive.Equals(false))
                    .OrderBy(p => p.AccCode)
                    .Select(a => new { AccNo = a.AccNo, DisplayValue = $"{a.AccCode} - {a.Description}" }),
                "AccNo",
                "DisplayValue",
                "AccNo"
            );

            return View(model);
        }
        private bool TxnTransactionsExists(string id)
        {
            return (_context.TxnTransactions?.Any(e => e.TxnNo == id)).GetValueOrDefault();
        }
    }
}
