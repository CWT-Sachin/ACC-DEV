using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.ViewModel;
using Newtonsoft.Json;
using ACC_DEV.Data;
using ACC_DEV.DataOperation;
using ACC_DEV.Models;

namespace ACC_DEV.Controllers
{
    public class TxnDebitNoteHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnDebitNoteHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        // GET: TxnDebitNoteHds
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1 )
        {
            var txnDebitNoteHds = _context.TxnDebitNoteHds.OrderByDescending(p => p.DebitNoteNo);

            if (searchType == "Approve")
            {
                // Filter only not approved invoices
                txnDebitNoteHds = txnDebitNoteHds.Where(txnDebitNoteHds => txnDebitNoteHds.Approved != true).OrderByDescending(p => p.DebitNoteNo);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                if (searchType == "CreditNote")
                {
                    txnDebitNoteHds = txnDebitNoteHds.Where(txnDebitNoteHds => txnDebitNoteHds.DebitNoteNo.Contains(searchString)).OrderByDescending(p => p.DebitNoteNo);
                }
                else if (searchType == "Job")
                {
                    txnDebitNoteHds = txnDebitNoteHds.Where(txnDebitNoteHds => txnDebitNoteHds.JobNo.Contains(searchString)).OrderByDescending(p => p.DebitNoteNo);
                }

                // Check if results are found
                if (txnDebitNoteHds.Any())
                {
                    ViewData["DebitNoteFound"] = "";
                }
                else
                {
                    ViewData["DebitNoteFound"] = $"{searchType} Number: {searchString} not found";
                }
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnDebitNoteHds.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnDebitNoteHds.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }


        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnDebitNoteHds = await _context.TxnDebitNoteHds
                .FirstOrDefaultAsync(m => m.DebitNoteNo == id);

            if (txnDebitNoteHds == null)
            {
                return NotFound();
            }

            jobNo = txnDebitNoteHds.JobNo; // Set the jobNo property

            var tables = new DebitNoteViewModel
            {
                DebitNoteHdMulti = _context.TxnDebitNoteHds.Where(t => t.DebitNoteNo == id),
                DebitNoteDtlMulti = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == id),

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

            var txnDebitNoteHds = await _context.TxnDebitNoteHds
                .FirstOrDefaultAsync(m => m.DebitNoteNo == id);

            if (txnDebitNoteHds == null)
            {
                return NotFound();
            }

            /// Update the Approved property based on the form submission
            txnDebitNoteHds.Approved = approved;
            txnDebitNoteHds.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnDebitNoteHds.ApprovedDateTime = DateTime.Now;

            _context.Update(txnDebitNoteHds);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }



        // GET: TxnDebitNoteHds/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TxnDebitNoteHds == null)
            {
                return NotFound();
            }

            var txnDebitNoteHd = await _context.TxnDebitNoteHds
                .FirstOrDefaultAsync(m => m.DebitNoteNo == id);
            if (txnDebitNoteHd == null)
            {
                return NotFound();
            }
            var jobNo = txnDebitNoteHd.JobNo;
            var tables = new DebitNoteViewModel
            {
                DebitNoteHdMulti = _context.TxnDebitNoteHds.Where(t => t.DebitNoteNo == id),
                DebitNoteDtlMulti = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == id),
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
            ViewData["AgentIDNomination"] = new SelectList(_operationcontext.RefAgents.Join(_operationcontext.RefPorts,
                 a => a.PortId,
                 b => b.PortCode,
                 (a, b) => new
                 {
                     AgentId = a.AgentId,
                     AgentName = a.AgentName + " - " + b.PortName,
                     IsActive = a.IsActive
                 }).Where(a => a.IsActive.Equals(true)).OrderBy(a => a.AgentName), "AgentId", "AgentName", "AgentId");
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
            ViewData["ShippingLine"] = new SelectList(_operationcontext.RefShippingLines.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");
            return View(tables);
        }

        //// GET: TxnDebitNoteHds/Create
        public IActionResult Create(string searchString)
        {
            var JobNo = "xxx";

            if (!string.IsNullOrEmpty(searchString))
            {


                JobNo = searchString;

                // Check if results are found
                if (_operationcontext.TxnExportJobHds.Any(t => t.JobNo == JobNo))
                {
                    ViewData["JobFound"] = "";
                }
                else
                {
                    ViewData["JobFound"] = "Job Number: " + searchString + " not found";
                }

            }
            var tables = new DebitNoteViewModel
            {
                DebitNoteHdMulti = _context.TxnDebitNoteHds.Where(t => t.DebitNoteNo == "xxx"),
                DebitNoteDtlMulti = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == "xxx"),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == JobNo),
                ExportJobHdMulti = _operationcontext.TxnExportJobHds
                    .Include(t => t.AgentExportNavigation)
                    .Include(t => t.HandlebyExportJobNavigation)
                    .Include(t => t.CreatedByExportJobNavigation)
                    .Include(t => t.ShippingLineExportNavigation)
                    .Include(t => t.ColoaderExportNavigation)
                    .Include(t => t.VesselExportJobDtlNavigation)
                    .Include(t => t.PODExportJobNavigation)
                    .Include(t => t.FDNExportJobNavigation)
                    .Where(t => t.JobNo == JobNo),

            };
            ViewData["AgentIDNomination"] = new SelectList(_operationcontext.RefAgents.Join(_operationcontext.RefPorts,
                 a => a.PortId,
                 b => b.PortCode,
                 (a, b) => new
                 {
                     AgentId = a.AgentId,
                     AgentName = a.AgentName + " - " + b.PortName,
                     IsActive = a.IsActive
                 }).Where(a => a.IsActive.Equals(true)).OrderBy(a => a.AgentName), "AgentId", "AgentName", "AgentId");
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
            ViewData["ShippingLine"] = new SelectList(_operationcontext.RefShippingLines.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
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

        // POST: TxnDebitNoteHds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("DebitNoteNo,Date,JobNo,Customer,ExchangeRate,TotalDebitAmountLkr,TotalDebitAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Type")] TxnDebitNoteHd txnDebitNoteHd)
        {

            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_DebitNoteHd";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "DEB" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnDebitNoteHd.DebitNoteNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }

            txnDebitNoteHd.Canceled = false;
            txnDebitNoteHd.CreatedBy = "Admin";
            txnDebitNoteHd.CreatedDateTime = DateTime.Now;

            ModelState.Remove("DebitNoteNo");  // Auto genrated
            if (ModelState.IsValid)
            {
                // Adding CargoBreakDown records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnDebitNoteDtl>>(dtlItemsList);
                    if (DtailItemdataTable != null)
                    {
                        foreach (var item in DtailItemdataTable)
                        {
                            TxnDebitNoteDtl DetailItem = new TxnDebitNoteDtl();
                            DetailItem.DebitNoteNo = nextRefNo; // New CreditNote Number
                            DetailItem.SerialNo = item.SerialNo;
                            DetailItem.ChargeItem = item.ChargeItem;
                            DetailItem.Unit = item.Unit ?? "DefaultUnit"; // Set a default value if 'Unit' is null
                            DetailItem.Rate = item.Rate;
                            DetailItem.Currency = item.Currency;
                            DetailItem.Qty = item.Qty;
                            DetailItem.Amount = item.Amount;
                            DetailItem.BlContainerNo = item.BlContainerNo;
                            DetailItem.AccNo = GetChargeItemAccNo(item.ChargeItem);
                            _context.TxnDebitNoteDtls.Add(DetailItem);
                        }
                    }
                }
                _context.Add(txnDebitNoteHd);
                _context.RefLastNumbers.Update(refLastNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var tables = new DebitNoteViewModel
            {
                DebitNoteHdMulti = _context.TxnDebitNoteHds.Where(t => t.DebitNoteNo == "xxx"),
                DebitNoteDtlMulti = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == "xxx"),

                ExportJobHdMulti = _operationcontext.TxnExportJobHds.Where(t => t.JobNo == "xxx"),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == "xxx"),
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
            //ViewData["Customer"] = new SelectList(_context.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");
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


        // GET: Print Invoice 
        public async Task<IActionResult> RepPrintDebitNote(string DebitNoteNo)
        {
            if (DebitNoteNo == null)
            {
                return NotFound();
            }

            var txnDebitNoteHd = await _context.TxnDebitNoteHds
                .FirstOrDefaultAsync(m => m.DebitNoteNo == DebitNoteNo);

            if (txnDebitNoteHd == null)
            {
                return NotFound();
            }

            var strjobNo = txnDebitNoteHd.JobNo;

            if (string.IsNullOrEmpty(strjobNo))
            {
                return NotFound(); // Handle the case where JobNo is null or empty
            }

            var containerNo = _operationcontext.TxnStuffingPlanHds
                .Where(s => s.JobNumber == strjobNo)
                .Select(s => s.ContainerNo)
                .FirstOrDefault();

            var tables = new DebitNoteViewModel
            {
                DebitNoteHdMulti = _context.TxnDebitNoteHds.Where(t => t.DebitNoteNo == DebitNoteNo),
                DebitNoteDtlMulti = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == DebitNoteNo)
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





        // GET: TxnDebitNoteHds/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.TxnDebitNoteHds == null)
            {
                return NotFound();
            }

            var txnDebitNoteHd = await _context.TxnDebitNoteHds.FindAsync(id);
            if (txnDebitNoteHd == null)
            {
                return NotFound();
            }
            var DebitNoteNo = id;
            var JobNo = txnDebitNoteHd.JobNo;
            var tables = new DebitNoteViewModel
            {
                DebitNoteHdMulti = _context.TxnDebitNoteHds.Where(t => t.DebitNoteNo == DebitNoteNo),
                DebitNoteDtlMulti = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == DebitNoteNo),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == JobNo),
                ExportJobHdMulti = _operationcontext.TxnExportJobHds
                    .Include(t => t.AgentExportNavigation)
                    .Include(t => t.HandlebyExportJobNavigation)
                    .Include(t => t.CreatedByExportJobNavigation)
                    .Include(t => t.ShippingLineExportNavigation)
                    .Include(t => t.ColoaderExportNavigation)
                    .Include(t => t.VesselExportJobDtlNavigation)
                    .Include(t => t.PODExportJobNavigation)
                    .Include(t => t.FDNExportJobNavigation)
                    .Where(t => t.JobNo == JobNo),

            };
            ViewData["AgentIDNomination"] = new SelectList(_operationcontext.RefAgents.Join(_operationcontext.RefPorts,
                 a => a.PortId,
                 b => b.PortCode,
                 (a, b) => new
                 {
                     AgentId = a.AgentId,
                     AgentName = a.AgentName + " - " + b.PortName,
                     IsActive = a.IsActive
                 }).Where(a => a.IsActive.Equals(true)).OrderBy(a => a.AgentName), "AgentId", "AgentName", "AgentId");
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
            ViewData["ShippingLine"] = new SelectList(_operationcontext.RefShippingLines.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");
            return View(tables);
        }

        // POST: TxnDebitNoteHds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string dtlItemsList, [Bind("DebitNoteNo,Date,JobNo,Customer,ExchangeRate,TotalDebitAmountLkr,TotalDebitAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Type")] TxnDebitNoteHd txnDebitNoteHd)
        {
            if (id != txnDebitNoteHd.DebitNoteNo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(dtlItemsList))
                    {
                        var rowsToDelete = _context.TxnDebitNoteDtls.Where(t => t.DebitNoteNo == id);
                        if (rowsToDelete != null || rowsToDelete.Any())
                        {
                            // Remove the rows from the database context
                            _context.TxnDebitNoteDtls.RemoveRange(rowsToDelete);
                        }
                        var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnCreditNoteDtl>>(dtlItemsList);
                        if (DtailItemdataTable != null)
                        {
                            foreach (var item in DtailItemdataTable)
                            {
                                TxnDebitNoteDtl DetailItem = new TxnDebitNoteDtl();
                                DetailItem.DebitNoteNo = id; // New CreditNote Number
                                DetailItem.SerialNo = item.SerialNo;
                                DetailItem.ChargeItem = item.ChargeItem;
                                DetailItem.Unit = item.Unit;
                                DetailItem.Rate = item.Rate;
                                DetailItem.Currency = item.Currency;
                                DetailItem.Qty = item.Qty;
                                DetailItem.Amount = item.Amount;
                                DetailItem.BlContainerNo = item.BlContainerNo;
                                _context.TxnDebitNoteDtls.Add(DetailItem);
                            }
                        }
                    }
                    _context.Update(txnDebitNoteHd);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TxnDebitNoteHdExists(txnDebitNoteHd.DebitNoteNo))
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
            return View(txnDebitNoteHd);
        }

        // GET: TxnDebitNoteHds/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TxnDebitNoteHds == null)
            {
                return NotFound();
            }

            var txnDebitNoteHd = await _context.TxnDebitNoteHds
                .FirstOrDefaultAsync(m => m.DebitNoteNo == id);
            if (txnDebitNoteHd == null)
            {
                return NotFound();
            }

            return View(txnDebitNoteHd);
        }

        // POST: TxnDebitNoteHds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TxnDebitNoteHds == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.TxnDebitNoteHds'  is null.");
            }
            var txnDebitNoteHd = await _context.TxnDebitNoteHds.FindAsync(id);
            if (txnDebitNoteHd != null)
            {
                _context.TxnDebitNoteHds.Remove(txnDebitNoteHd);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TxnDebitNoteHdExists(string id)
        {
          return (_context.TxnDebitNoteHds?.Any(e => e.DebitNoteNo == id)).GetValueOrDefault();
        }
    }
}
