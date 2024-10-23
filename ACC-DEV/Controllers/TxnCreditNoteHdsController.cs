using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.ViewModel;
using Newtonsoft.Json;
using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;

using ACC_DEV.Data;
using ACC_DEV.DataOperation;


namespace ACC_DEV.Controllers
{
    public class TxnCreditNoteHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnCreditNoteHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        // GET: TxnCreditNoteHds
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnCreditNoteHds = _context.TxnCreditNoteHds.OrderByDescending(p => p.CreditNoteNo);

            if (searchType == "Approve")
            {
                // Filter only not approved invoices
                txnCreditNoteHds = txnCreditNoteHds.Where(txnCreditNoteHds => txnCreditNoteHds.Approved != true).OrderByDescending(p => p.CreditNoteNo);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                if (searchType == "CreditNote")
                {
                    txnCreditNoteHds = txnCreditNoteHds.Where(txnCreditNoteHds => txnCreditNoteHds.CreditNoteNo.Contains(searchString)).OrderByDescending(p => p.CreditNoteNo);
                }
                else if (searchType == "Job")
                {
                    txnCreditNoteHds = txnCreditNoteHds.Where(txnCreditNoteHds => txnCreditNoteHds.JobNo.Contains(searchString)).OrderByDescending(p => p.CreditNoteNo);
                }

                // Check if results are found
                if (txnCreditNoteHds.Any())
                {
                    ViewData["CreditNoteFound"] = "";
                }
                else
                {
                    ViewData["CreditNoteFound"] = $"{searchType} Number: {searchString} not found";
                }
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnCreditNoteHds.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnCreditNoteHds.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }


        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnCreditNoteHds = await _context.TxnCreditNoteHds
                .FirstOrDefaultAsync(m => m.CreditNoteNo == id);

            if (txnCreditNoteHds == null)
            {
                return NotFound();
            }

            jobNo = txnCreditNoteHds.JobNo; // Set the jobNo property

            var tables = new CreditNoteViewModel
            {
                CreditNoteHdMulti = _context.TxnCreditNoteHds.Where(t => t.CreditNoteNo == id),
                CreditNoteDtlMulti = _context.TxnCreditNoteDtls.Where(t => t.CreditNoteNo == id),
               

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

            var txnCreditNoteHds = await _context.TxnCreditNoteHds
                .FirstOrDefaultAsync(m => m.CreditNoteNo == id);

            if (txnCreditNoteHds == null)
            {
                return NotFound();
            }

            /// Update the Approved property based on the form submission
            txnCreditNoteHds.Approved = approved;
            txnCreditNoteHds.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnCreditNoteHds.ApprovedDateTime = DateTime.Now;

            _context.Update(txnCreditNoteHds);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }

        // GET: TxnCreditNoteHds/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TxnCreditNoteHds == null)
            {
                return NotFound();
            }

            var txnCreditNoteHd = await _context.TxnCreditNoteHds
                .FirstOrDefaultAsync(m => m.CreditNoteNo == id);
            if (txnCreditNoteHd == null)
            {
                return NotFound();
            }
            var jobNo = txnCreditNoteHd.JobNo;

            var tables = new CreditNoteViewModel
            {
                CreditNoteHdMulti = _context.TxnCreditNoteHds.Where(t => t.CreditNoteNo == id),
                CreditNoteDtlMulti = _context.TxnCreditNoteDtls.Where(t => t.CreditNoteNo == id),
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

        //// GET: TxnCreditNoteHds/Create
        public IActionResult Create(string searchString)
        {
            var jobNo = "xxx";

            if (!string.IsNullOrEmpty(searchString))
            {


                jobNo = searchString;

                // Check if results are found
                if (_operationcontext.TxnExportJobHds.Any(t => t.JobNo == jobNo))
                {
                    ViewData["JobFound"] = "";
                }
                else
                {
                    ViewData["JobFound"] = "Job Number: " + searchString + " not found";
                }

            }
            var tables = new CreditNoteViewModel
            {
                CreditNoteHdMulti = _context.TxnCreditNoteHds.Where(t => t.CreditNoteNo == "xxx"),
                CreditNoteDtlMulti = _context.TxnCreditNoteDtls.Where(t => t.CreditNoteNo == "xxx"),
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

        // POST: TxnCreditNoteHds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("CreditNoteNo,Date,JobNo,Customer,ExchangeRate,TotalCreditAmountLkr,TotalCreditAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,CreditAcc,DebitAcc,Type")] TxnCreditNoteHd txnCreditNoteHd)
        {

            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_CreditNoteHd";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);

            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "CRE" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnCreditNoteHd.CreditNoteNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }

            txnCreditNoteHd.Canceled = false;
            txnCreditNoteHd.Canceled = false;
            txnCreditNoteHd.CreatedBy = "Admin";
            txnCreditNoteHd.CreatedDateTime = DateTime.Now;

            ModelState.Remove("CreditNoteNo");  // Auto genrated
            if (ModelState.IsValid)
            {
                // Adding CargoBreakDown records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnCreditNoteDtl>>(dtlItemsList);
                    if (DtailItemdataTable != null)
                    {
                        foreach (var item in DtailItemdataTable)
                        {
                            TxnCreditNoteDtl DetailItem = new TxnCreditNoteDtl();
                            DetailItem.CreditNoteNo = nextRefNo; // New CreditNote Number
                            DetailItem.SerialNo = item.SerialNo;
                            DetailItem.ChargeItem = item.ChargeItem;
                            DetailItem.Unit = item.Unit ?? "DefaultUnit"; // Set a default value if 'Unit' is null
                            DetailItem.Rate = item.Rate;
                            DetailItem.Currency = item.Currency;
                            DetailItem.Qty = item.Qty;
                            DetailItem.Amount = item.Amount;


                            DetailItem.BlContainerNo = item.BlContainerNo;
                            //DetailItem.BlContainerNo = "BlContainerno";
                            DetailItem.AccNo = GetChargeItemAccNo(item.ChargeItem);



                            _context.TxnCreditNoteDtls.Add(DetailItem);
                        }
                    }
                }
                _context.Add(txnCreditNoteHd);
                _context.RefLastNumbers.Update(refLastNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var tables = new CreditNoteViewModel
            {
                CreditNoteHdMulti = _context.TxnCreditNoteHds.Where(t => t.CreditNoteNo == "xxx"),
                CreditNoteDtlMulti = _context.TxnCreditNoteDtls.Where(t => t.CreditNoteNo == "xxx"),

                //ImportJobHdMulti = _context.TxnImportJobHds.Where(t => t.JobNo == "xxx"), 
                //ImportJobDtlMulti = _context.TxnImportJobDtls.Where(t => t.JobNo == "xxx"), 

                ExportJobHdMulti = _operationcontext.TxnExportJobHds.Where(t => t.JobNo == jobNo),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == jobNo),
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
            ViewData["RefAgentList"] = new SelectList(_context.Set<RefAgent>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.AgentName), "AgentID", "AgentName", "AgentID");
            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

            return View(tables);
        }
        // GET: Print CreditNote 
        public async Task<IActionResult> RepPrintCreditNote(string CreditNoteNo)
        {
            if (CreditNoteNo == null || _context.TxnCreditNoteHds == null)
            {
                return NotFound();
            }

            var txnCreditNoteHd = await _context.TxnCreditNoteHds
                .FirstOrDefaultAsync(m => m.CreditNoteNo == CreditNoteNo);

            if (txnCreditNoteHd == null)
            {
                return NotFound();
            }

            var strjobNo = txnCreditNoteHd.JobNo;
            var tables = new CreditNoteViewModel
            {
                CreditNoteHdMulti = _context.TxnCreditNoteHds.Where(t =>t.CreditNoteNo == CreditNoteNo),
                CreditNoteDtlMulti = _context.TxnCreditNoteDtls.Where(t => t.CreditNoteNo == CreditNoteNo)
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
            ViewData["ShippingLine"] = new SelectList(_operationcontext.RefShippingLines.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            return View(tables);

        }

        // GET: TxnCreditNoteHds/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.TxnCreditNoteHds == null)
            {
                return NotFound();
            }

            var txnCreditNoteHd = await _context.TxnCreditNoteHds.FindAsync(id);
            if (txnCreditNoteHd == null)
            {
                return NotFound();
            }
            var CreditNoteNo = id;
            var JobNo = txnCreditNoteHd.JobNo;

            var tables = new CreditNoteViewModel
            {
                CreditNoteHdMulti = _context.TxnCreditNoteHds.Where(t => t.CreditNoteNo == CreditNoteNo),
                CreditNoteDtlMulti = _context.TxnCreditNoteDtls.Where(t => t.CreditNoteNo == CreditNoteNo),
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

        // POST: TxnCreditNoteHds/Edit/5 
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string dtlItemsList, [Bind("CreditNoteNo,Date,JobNo,Customer,ExchangeRate,TotalCreditAmountLkr,TotalCreditAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Type")] TxnCreditNoteHd txnCreditNoteHd)
        {
            if (id != txnCreditNoteHd.CreditNoteNo)
            {
                return NotFound();
            }
            txnCreditNoteHd.LastUpdatedBy = "Admin";
            txnCreditNoteHd.LastUpdatedDateTime = DateTime.Now;
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(dtlItemsList))
                    {
                        var rowsToDelete = _context.TxnCreditNoteDtls.Where(t => t.CreditNoteNo == id);
                        if (rowsToDelete != null || rowsToDelete.Any())
                        {
                            // Remove the rows from the database context
                            _context.TxnCreditNoteDtls.RemoveRange(rowsToDelete);
                        }
                        var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnCreditNoteDtl>>(dtlItemsList);
                        if (DtailItemdataTable != null)
                        {
                            foreach (var item in DtailItemdataTable)
                            {
                                TxnCreditNoteDtl DetailItem = new TxnCreditNoteDtl();
                                DetailItem.CreditNoteNo = id; // New CreditNote Number
                                DetailItem.SerialNo = item.SerialNo;
                                DetailItem.ChargeItem = item.ChargeItem;
                                DetailItem.Unit = item.Unit;
                                DetailItem.Rate = item.Rate;
                                DetailItem.Currency = item.Currency;
                                DetailItem.Qty = item.Qty;
                                DetailItem.Amount = item.Amount;
                                DetailItem.BlContainerNo = item.BlContainerNo;
                                //DetailItem.BlContainerNo = "BlContainerno";
                                _context.TxnCreditNoteDtls.Add(DetailItem);
                            }
                        }
                    }
                    _context.Update(txnCreditNoteHd);
                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!TxnCreditNoteHdExists(txnCreditNoteHd.CreditNoteNo))
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
            return View(txnCreditNoteHd);
        }

        // GET: TxnCreditNoteHds/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TxnCreditNoteHds == null)
            {
                return NotFound();
            }

            var txnCreditNoteHd = await _context.TxnCreditNoteHds
                .FirstOrDefaultAsync(m => m.CreditNoteNo == id);
            if (txnCreditNoteHd == null)
            {
                return NotFound();
            }

            return View(txnCreditNoteHd);
        }

        // POST: TxnCreditNoteHds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TxnCreditNoteHds == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.TxnCreditNoteHds'  is null.");
            }
            var txnCreditNoteHd = await _context.TxnCreditNoteHds.FindAsync(id);
            if (txnCreditNoteHd != null)
            {
                _context.TxnCreditNoteHds.Remove(txnCreditNoteHd);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TxnCreditNoteHdExists(string id)
        {
          return (_context.TxnCreditNoteHds?.Any(e => e.CreditNoteNo == id)).GetValueOrDefault();
        }
    }
}
