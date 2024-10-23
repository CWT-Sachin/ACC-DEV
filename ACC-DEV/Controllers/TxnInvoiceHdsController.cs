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
using System.Data;


namespace ACC_DEV.Controllers
{
    public class TxnInvoiceHdsController : Controller
    {

        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnInvoiceHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }


        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnInvoiceExportHds = _context.TxnInvoiceHds.OrderByDescending(p => p.InvoiceNo);

            if (searchType == "Approve")
            {
                // Filter only not approved invoices
                txnInvoiceExportHds = txnInvoiceExportHds.Where(txnInvoiceExportHds => txnInvoiceExportHds.Approved != true).OrderByDescending(p => p.InvoiceNo);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                if (searchType == "Invoice")
                {
                    txnInvoiceExportHds = txnInvoiceExportHds.Where(txnInvoiceExportHds => txnInvoiceExportHds.InvoiceNo.Contains(searchString)).OrderByDescending(p => p.InvoiceNo);
                }
                else if (searchType == "Job")
                {
                    txnInvoiceExportHds = txnInvoiceExportHds.Where(txnInvoiceExportHds => txnInvoiceExportHds.JobNo.Contains(searchString)).OrderByDescending(p => p.InvoiceNo);
                }

                // Check if results are found 
                if (txnInvoiceExportHds.Any()) 
                {
                    ViewData["InvoiceFound"] = "";
                }
                else
                {
                    ViewData["InvoiceFound"] = $"{searchType} Number: {searchString} not found";
                }
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnInvoiceExportHds.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnInvoiceExportHds.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }



        // GET: Print Invoice 
        public async Task<IActionResult> RepPrintInvoice(string InvoiceNo)
        {
            if (InvoiceNo == null || _context.TxnInvoiceHds == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == InvoiceNo);

            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            var strjobNo = txnInvoiceExportHd.JobNo;

            var containerNo = _operationcontext.TxnStuffingPlanHds
                .Where(s => s.JobNumber == strjobNo)
                .Select(s => s.ContainerNo)
                .FirstOrDefault();

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == InvoiceNo)
                   /* .Include(t => t.InvoiceHdAcc)*/,
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == InvoiceNo)
                    .Include(t => t.ChargeItemNavigation),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == strjobNo),
                ExportJobHdMulti = _operationcontext.TxnExportJobHds
                    .Include(t => t.AgentExportNavigation)
                    .Include(t => t.HandlebyExportJobNavigation)
                    .Include(t => t.CreatedByExportJobNavigation)
                    .Include(t => t.ShippingLineExportNavigation)
                    .Include(t => t.ColoaderExportNavigation)
                    .Include(t => t.VesselExportJobDtlNavigation)
                    .Include(t => t.PODExportJobNavigation)
                    .Include(t => t.FDNExportJobNavigation)
                    .Where(t => t.JobNo == strjobNo),
                    ContainerNo = containerNo // I Just Set the Container Number 
            };
            List<SelectListItem> Currency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "USD", Text = "USD" },
                    new SelectListItem { Value = "LKR", Text = "LKR" }
                };
            ViewData["CurrencyList"] = new SelectList(Currency, "Value", "Text", "CurrencyList");
            List<SelectListItem> Unit = new List<SelectListItem>
                {
                    new SelectListItem { Value = "CBM", Text = "CBM" },
                    new SelectListItem { Value = "BL", Text = "BL" },
                    new SelectListItem { Value = "CNT", Text = "CNT" }
                };
            ViewData["UnitList"] = new SelectList(Unit, "Value", "Text", "UnitList");
            ViewData["ChargeItemsList"] = new SelectList(_context.Set<RefChargeItem>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ChargeId", "Description", "ChargeId");
            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");
            return View(tables);

        }


        // GET: txnInvoiceExportHds/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TxnInvoiceHds == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == id);

            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            var jobNo = txnInvoiceExportHd.JobNo;

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == id),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == id),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == jobNo),
                ExportJobHdMulti = _operationcontext.TxnExportJobHds
                    .Include(t => t.AgentExportNavigation)
                    .Include(t => t.HandlebyExportJobNavigation)
                    .Include(t => t.CreatedByExportJobNavigation)
                    .Include(t => t.ShippingLineExportNavigation)
                    .Include(t => t.ColoaderExportNavigation)
                    .Include(t => t.VesselExportJobDtlNavigation)
                    .Include(t => t.PODExportJobNavigation)
                    .Include(t => t.FDNExportJobNavigation)
                    .Where(t => t.JobNo == jobNo),
            };

            List<SelectListItem> Currency = new List<SelectListItem>
    {
        new SelectListItem { Value = "USD", Text = "USD" },
        new SelectListItem { Value = "LKR", Text = "LKR" }
    };
            ViewData["CurrencyList"] = new SelectList(Currency, "Value", "Text", "CurrencyList");

            List<SelectListItem> Unit = new List<SelectListItem>
    {
        new SelectListItem { Value = "CBM", Text = "CBM" },
        new SelectListItem { Value = "BL", Text = "BL" },
        new SelectListItem { Value = "CNT", Text = "CNT" }
    };
            ViewData["UnitList"] = new SelectList(Unit, "Value", "Text", "UnitList");

            ViewData["ChargeItemsList"] = new SelectList(_context.Set<RefChargeItem>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ChargeId", "Description", "ChargeId");
            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");
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

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == id);

            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            jobNo = txnInvoiceExportHd.JobNo; // Set the jobNo property

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == id),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == id),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == jobNo),
                ExportJobHdMulti = _operationcontext.TxnExportJobHds
                    .Include(t => t.AgentExportNavigation)
                    .Include(t => t.HandlebyExportJobNavigation)
                    .Include(t => t.CreatedByExportJobNavigation)
                    .Include(t => t.ShippingLineExportNavigation)
                    .Include(t => t.ColoaderExportNavigation)
                    .Include(t => t.VesselExportJobDtlNavigation)
                    .Include(t => t.PODExportJobNavigation)
                    .Include(t => t.FDNExportJobNavigation)
                    .Where(t => t.JobNo == jobNo),
            };

            return View(tables);
        }


        [HttpPost]
        public async Task<IActionResult> Approve(string id, bool approved)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == id);

            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            /// Update the Approved property based on the form submission
            txnInvoiceExportHd.Approved = approved;
            txnInvoiceExportHd.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnInvoiceExportHd.ApprovedDateTime = DateTime.Now;

            _context.Update(txnInvoiceExportHd);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }



        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchString, string jobType)
        {
            var jobNo = "xxx";

            if (!string.IsNullOrEmpty(searchString))
            {
                jobNo = searchString;
                if (jobType == "Import")
                {
                    if (!_operationcontext.TxnImportJobDtls.Any(t => t.JobNo == jobNo))
                    {
                        ViewData["JobFound"] = "Import Job Number: " + searchString + " not found";
                    }
                }
                else if (jobType == "Export")
                {
                    if (!_operationcontext.TxnExportJobHds.Any(t => t.JobNo == jobNo))
                    {
                        ViewData["JobFound"] = "Export Job Number: " + searchString + " not found";
                    }
                }
            }

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == "xxx"),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == "xxx"),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == jobNo),
                ExportJobHdMulti = _operationcontext.TxnExportJobHds
                    .Include(t => t.AgentExportNavigation)
                    .Include(t => t.HandlebyExportJobNavigation)
                    .Include(t => t.CreatedByExportJobNavigation)
                    .Include(t => t.ShippingLineExportNavigation)
                    .Include(t => t.ColoaderExportNavigation)
                    .Include(t => t.VesselExportJobDtlNavigation)
                    .Include(t => t.PODExportJobNavigation)
                    .Include(t => t.FDNExportJobNavigation)
                    .Where(t => t.JobNo == jobNo),
                ImportJobDtlMulti = _operationcontext.TxnImportJobDtls.Where(t => t.JobNo == jobNo),
                ImportJobHdMulti = _operationcontext.TxnImportJobHds
            .Include(t => t.AgentNavigation)
            .Include(t => t.HandlebyImportJobNavigation)
            .Include(t => t.ShippingLineNavigation)
            .Include(t => t.VesselImportJobDtlNavigation)
            .Include(t => t.PolNavigation)
            .Include(t => t.LastPortNavigation)
            .Where(t => t.JobNo == jobNo),
            };

            List <SelectListItem> Currency = new List<SelectListItem>
    {
        new SelectListItem { Value = "USD", Text = "USD" },
        new SelectListItem { Value = "LKR", Text = "LKR" }
    };
            ViewData["CurrencyList"] = new SelectList(Currency, "Value", "Text", "CurrencyList");

            List<SelectListItem> Unit = new List<SelectListItem>
    {
        new SelectListItem { Value = "CBM", Text = "CBM" },
        new SelectListItem { Value = "BL", Text = "BL" },
        new SelectListItem { Value = "CNT", Text = "CNT" }
    };
            ViewData["UnitList"] = new SelectList(Unit, "Value", "Text", "UnitList");

            ViewData["ChargeItemsList"] = new SelectList(_context.Set<RefChargeItem>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ChargeId", "Description", "ChargeId");

            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

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


        //I added a private function GetChargeItemAccNo that takes a ChargeId as a 
        //parameter and returns the corresponding AccNo by querying the RefChargeItems table in your database.
        //Then, inside your loop that iterates through DtailItemdataTable,
        //I call this function to get the AccNo for each ChargeItem and set it in the DetailItem.AccNo property.


        private string GetChargeItemAccNo(string chargeItemNo)
        {
            var chargeItem = _context.RefChargeItems.FirstOrDefault(x => x.ChargeId == chargeItemNo);
            return chargeItem?.AccNo;
        }



        // POST: txnInvoiceExportHds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: txnInvoiceExportHds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList,[Bind("InvoiceNo,Date,JobNo,Blno,Customer,ExchangeRate,TotalInvoiceAmountLkr,TotalInvoiceAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,CreditAcc,DebitAcc,PaidAmtLKR,BalanceAmtLKR,JobType,JobMode")] TxnInvoiceHd txnInvoiceHd)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_InvoiceHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "INVEX" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnInvoiceHd.InvoiceNo = nextRefNo;
            }
            else
            {
                return NotFound();
            }

            txnInvoiceHd.Canceled = false;
            txnInvoiceHd.Approved = false;
            txnInvoiceHd.CreatedBy = "Admin";
            txnInvoiceHd.CreatedDateTime = DateTime.Now;
            // Retrieve the selected radio button value from the form collection
            string jobType = Request.Form["JobType"];

            // Set the JobType property with the selected value from radio buttons
            txnInvoiceHd.JobType = jobType;

            ModelState.Remove("InvoiceNo");  // Auto generated

            if (ModelState.IsValid)
            {
                // Adding CargoBreakDown records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<TxnInvoiceDtl>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            TxnInvoiceDtl detailItem = new TxnInvoiceDtl();
                            detailItem.InvoiceNo = nextRefNo; // New Invoice Number
                            detailItem.SerialNo = item.SerialNo;
                            detailItem.ChargeItem = item.ChargeItem;
                            detailItem.Unit = item.Unit;
                            detailItem.Rate = item.Rate;
                            detailItem.Currency = item.Currency;
                            detailItem.Qty = item.Qty;
                            detailItem.Amount = item.Amount;
                            detailItem.CreatedDate = DateTime.Now;
                            detailItem.AccNo = GetChargeItemAccNo(item.ChargeItem);
                            _context.TxnInvoiceDtls.Add(detailItem);
                        }
                    }
                }

                _context.Add(txnInvoiceHd);
                _context.RefLastNumbers.Update(refLastNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == "xxx"),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == "xxx"),
                ExportJobHdMulti = _operationcontext.TxnExportJobHds.Where(t => t.JobNo == txnInvoiceHd.JobNo),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == txnInvoiceHd.JobNo),
                ImportJobHdMulti = _operationcontext.TxnImportJobHds.Where(t => t.JobNo == txnInvoiceHd.JobNo),
                ImportJobDtlMulti = _operationcontext.TxnImportJobDtls.Where(t => t.JobNo == txnInvoiceHd.JobNo),
            };

            List<SelectListItem> Currency = new List<SelectListItem>
    {
        new SelectListItem { Value = "USD", Text = "USD" },
        new SelectListItem { Value = "LKR", Text = "LKR" }
    };
            ViewData["CurrencyList"] = new SelectList(Currency, "Value", "Text", "CurrencyList");

            List<SelectListItem> Unit = new List<SelectListItem>
    {
        new SelectListItem { Value = "CBM", Text = "CBM" },
        new SelectListItem { Value = "BL", Text = "BL" },
        new SelectListItem { Value = "CNT", Text = "CNT" }
    };
            ViewData["UnitList"] = new SelectList(Unit, "Value", "Text", "UnitList");

            ViewData["ChargeItemsList"] = new SelectList(_context.Set<RefChargeItem>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ChargeId", "Description", "ChargeId");

            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

            return View(tables);
        }
        // POST: txnInvoiceExportHds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string dtlItemsList, string id, [Bind("InvoiceNo,Date,JobNo,Blno,Customer,ExchangeRate,TotalInvoiceAmountLkr,TotalInvoiceAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,CreditAcc,DebitAcc,PaidAmtLKR,BalanceAmtLKR,JobType,JobMode")] TxnInvoiceHd txnInvoiceHd)
        {
            if (id != txnInvoiceHd.InvoiceNo)
            {
                return NotFound();
            }
            txnInvoiceHd.LastUpdatedBy = "Admin";
            txnInvoiceHd.LastUpdatedDateTime = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
                    // Adding Invoice Items records
                    if (!string.IsNullOrWhiteSpace(dtlItemsList))
                    {
                        var rowsToDelete = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == id);
                        if (rowsToDelete != null || rowsToDelete.Any())
                        {
                            // Remove the rows from the database context
                            _context.TxnInvoiceDtls.RemoveRange(rowsToDelete);
                        }
                        var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnInvoiceDtl>>(dtlItemsList);
                        if (DtailItemdataTable != null)
                        {
                            foreach (var item in DtailItemdataTable)
                            {
                                TxnInvoiceDtl DetailItem = new TxnInvoiceDtl();
                                DetailItem.InvoiceNo = id; // New Invoice Number
                                DetailItem.SerialNo = item.SerialNo;
                                DetailItem.ChargeItem = item.ChargeItem;
                                DetailItem.Unit = item.Unit ?? "DefaultUnit"; // Set a default value if 'Unit' is null
                                DetailItem.Rate = item.Rate;
                                DetailItem.Currency = item.Currency;
                                DetailItem.Qty = item.Qty;
                                DetailItem.Amount = item.Amount;
                                DetailItem.CreatedDate = DateTime.Now;
                                DetailItem.AccNo = GetChargeItemAccNo(item.ChargeItem);
                                _context.TxnInvoiceDtls.Add(DetailItem);
                            }
                        }
                    }

                    _context.Update(txnInvoiceHd);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!txnInvoiceExportHdExists(txnInvoiceHd.InvoiceNo))
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
            return View(txnInvoiceHd);
        }

        // GET: txnInvoiceExportHds/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TxnInvoiceHds == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == id);
            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            return View(txnInvoiceExportHd);
        }

        // POST: txnInvoiceExportHds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TxnInvoiceHds == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.TxnInvoiceExportHds'  is null.");
            }
            var txnInvoiceExportHd = await _context.TxnInvoiceHds.FindAsync(id);
            if (txnInvoiceExportHd != null)
            {
                _context.TxnInvoiceHds.Remove(txnInvoiceExportHd);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool txnInvoiceExportHdExists(string id)
        {
          return (_context.TxnInvoiceHds?.Any(e => e.InvoiceNo == id)).GetValueOrDefault();
        }
    }
}
