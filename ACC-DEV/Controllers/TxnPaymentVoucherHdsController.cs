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


namespace ACC_DEV.Controllers
{
    public class TxnPaymentVoucherHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;


        public string jobNo { get; private set; }


        public TxnPaymentVoucherHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        // GET: TxnPaymentVoucherHds
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnPaymentVoucherHds = _context.TxnPaymentVoucherHds.OrderByDescending(p => p.PayVoucherNo);

            if (searchType == "Approve")
            {
                // Filter only not approved invoices
                txnPaymentVoucherHds = txnPaymentVoucherHds.Where(txnPaymentVoucherHds => txnPaymentVoucherHds.Approved != true).OrderByDescending(p => p.PayVoucherNo);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                if (searchType == "PaymentVoucherNote")
                {
                    txnPaymentVoucherHds = txnPaymentVoucherHds.Where(txnPaymentVoucherHds => txnPaymentVoucherHds.PayVoucherNo.Contains(searchString)).OrderByDescending(p => p.PayVoucherNo);
                }
                else if (searchType == "Job")
                {
                    txnPaymentVoucherHds = txnPaymentVoucherHds.Where(txnPaymentVoucherHds => txnPaymentVoucherHds.JobNo.Contains(searchString)).OrderByDescending(p => p.PayVoucherNo);
                }

                // Check if results are found
                if (txnPaymentVoucherHds.Any())
                {
                    ViewData["PayemntVocherFound"] = "";
                }
                else
                {
                    ViewData["PayemntVocherFound"] = $"{searchType} Number: {searchString} not found";
                }
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnPaymentVoucherHds.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnPaymentVoucherHds.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }


        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnPaymentVoucherHds = await _context.TxnPaymentVoucherHds
                .FirstOrDefaultAsync(m => m.PayVoucherNo == id);

            if (txnPaymentVoucherHds == null)
            {
                return NotFound();
            }

            jobNo = txnPaymentVoucherHds.JobNo; // Set the jobNo property

            var tables = new PayVoucherViewModel
            {
                PayVoucherHdMulti = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == id),
                PayVoucherDtlMulti = _context.TxnPaymentVoucherDtls.Where(t => t.PayVoucherNo == id),
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

            var txnPaymentVoucherHds = await _context.TxnPaymentVoucherHds
                .FirstOrDefaultAsync(m => m.PayVoucherNo == id);

            if (txnPaymentVoucherHds == null)
            {
                return NotFound();
            }

            /// Update the Approved property based on the form submission
            txnPaymentVoucherHds.Approved = approved;
            txnPaymentVoucherHds.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnPaymentVoucherHds.ApprovedDateTime = DateTime.Now;

            _context.Update(txnPaymentVoucherHds);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }


        // GET: TxnPaymentVoucherHds/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TxnPaymentVoucherHds == null)
            {
                return NotFound();
            }

            var txnPaymentVoucherHd = await _context.TxnPaymentVoucherHds
                .FirstOrDefaultAsync(m => m.PayVoucherNo == id);
            if (txnPaymentVoucherHd == null)
            {
                return NotFound();
            }
            var jobNo = txnPaymentVoucherHd.JobNo;
            var tables = new PayVoucherViewModel
            {
                PayVoucherHdMulti = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == id),
                PayVoucherDtlMulti = _context.TxnPaymentVoucherDtls.Where(t => t.PayVoucherNo == id),
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
            List<SelectListItem> paymentcurrency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "USD Payment", Text = "USD Payment" },
                    new SelectListItem { Value = "LKR Payment", Text = "LKR Payment" }
                };
            ViewData["paymentcurrencyList"] = new SelectList(paymentcurrency, "Value", "Text", "paymentcurrency");
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


        //// GET: TxnDebitNoteHds/Create
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
            var tables = new PayVoucherViewModel
            {
                PayVoucherHdMulti = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == "xxx"),
                PayVoucherDtlMulti = _context.TxnPaymentVoucherDtls.Where(t => t.PayVoucherNo == "xxx"),
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

            List<SelectListItem> paymentcurrency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "USD Payment", Text = "USD Payment" },
                    new SelectListItem { Value = "LKR Payment", Text = "LKR Payment" }
                };
            ViewData["paymentcurrencyList"] = new SelectList(paymentcurrency, "Value", "Text", "paymentcurrency");

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


        // POST: TxnPaymentVoucherHds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("PayVoucherNo,Date,JobNo,Customer,ExchangeRate,PayCurrency,TotalPayVoucherAmountLkr,TotalPayVoucherAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,DebitAcc,CreditAcc,Type")] TxnPaymentVoucherHd txnPaymentVoucherHd)
        {

            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_PaymentVoucherHd";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "PAY" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnPaymentVoucherHd.PayVoucherNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }

            txnPaymentVoucherHd.Canceled = false;
            txnPaymentVoucherHd.CreatedBy = "Admin";
            txnPaymentVoucherHd.CreatedDateTime = DateTime.Now;

            ModelState.Remove("PayVoucherNo");  // Auto genrated
            if (ModelState.IsValid)
            {
                // Adding CargoBreakDown records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnPaymentVoucherDtl>>(dtlItemsList);
                    if (DtailItemdataTable != null)
                    {
                        foreach (var item in DtailItemdataTable)
                        {
                            TxnPaymentVoucherDtl DetailItem = new TxnPaymentVoucherDtl();
                            DetailItem.PayVoucherNo = nextRefNo; // New PayVoucherNo Number
                            DetailItem.SerialNo = item.SerialNo;
                            DetailItem.ChargeItem = item.ChargeItem;
                            DetailItem.Unit = item.Unit ?? "DefaultUnit"; // Set a default value if 'Unit' is null
                            DetailItem.Rate = item.Rate;
                            DetailItem.Currency = item.Currency;
                            DetailItem.Qty = item.Qty;
                            DetailItem.Amount = item.Amount;
                            DetailItem.BlContainerNo = item.BlContainerNo;
                            _context.TxnPaymentVoucherDtls.Add(DetailItem);
                        }
                    }
                }
                _context.Add(txnPaymentVoucherHd);
                _context.RefLastNumbers.Update(refLastNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var tables = new PayVoucherViewModel
            {
                PayVoucherHdMulti = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == "xxx"),
                PayVoucherDtlMulti = _context.TxnPaymentVoucherDtls.Where(t => t.PayVoucherNo == "xxx"),

                ExportJobHdMulti = _operationcontext.TxnExportJobHds.Where(t => t.JobNo == "xxx"),
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == "xxx"),
            };

            List<SelectListItem> Currency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "USD", Text = "USD" },
                    new SelectListItem { Value = "LKR", Text = "LKR" }
                };
            ViewData["CurrencyList"] = new SelectList(Currency, "Value", "Text", "CurrencyList");
            List<SelectListItem> paymentcurrency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "USD Payment", Text = "USD Payment" },
                    new SelectListItem { Value = "LKR Payment", Text = "LKR Payment" }
                };
            ViewData["paymentcurrencyList"] = new SelectList(paymentcurrency, "Value", "Text", "paymentcurrency");
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






        // GET: Print Payment Voucher
        public async Task<IActionResult> RepPrintPaymentVoucher(string PayVoucherNo)
        {
            if (PayVoucherNo == null || _context.TxnPaymentVoucherHds == null)
            {
                return NotFound();
            }

            var txnPaymentVoucherHd = await _context.TxnPaymentVoucherHds
                .FirstOrDefaultAsync(m => m.PayVoucherNo == PayVoucherNo);

            if (txnPaymentVoucherHd == null)
            {
                return NotFound();
            }
            var strjobNo = txnPaymentVoucherHd.JobNo;
            var tables = new PayVoucherViewModel
            {
                PayVoucherHdMulti = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == PayVoucherNo),
                PayVoucherDtlMulti = _context.TxnPaymentVoucherDtls.Where(t => t.PayVoucherNo == PayVoucherNo)
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
            List<SelectListItem> paymentcurrency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "USD Payment", Text = "USD Payment" },
                    new SelectListItem { Value = "LKR Payment", Text = "LKR Payment" }
                };
            ViewData["paymentcurrencyList"] = new SelectList(paymentcurrency, "Value", "Text", "paymentcurrency");
            List<SelectListItem> Unit = new List<SelectListItem>
                {
                    new SelectListItem { Value = "CBM", Text = "CBM" },
                    new SelectListItem { Value = "BL", Text = "BL" },
                    new SelectListItem { Value = "CNT", Text = "CNT" }
                };
            ViewData["UnitList"] = new SelectList(Unit, "Value", "Text", "UnitList");
            ViewData["ChargeItemsList"] = new SelectList(_context.Set<RefChargeItem>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ChargeId", "Description", "ChargeId");
            ViewData["ShippingLines"] = new SelectList(_operationcontext.RefShippingLines.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            return View(tables);

        }


        // GET: TxnPaymentVoucherHds/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.TxnPaymentVoucherHds == null)
            {
                return NotFound();
            }

            var txnPaymentVoucherHd = await _context.TxnPaymentVoucherHds.FindAsync(id);
            if (txnPaymentVoucherHd == null)
            {
                return NotFound();
            }
            var PayVoucherNo = id;
            var JobNo = txnPaymentVoucherHd.JobNo;

            var tables = new PayVoucherViewModel
            {
                PayVoucherHdMulti = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == PayVoucherNo),
                PayVoucherDtlMulti = _context.TxnPaymentVoucherDtls.Where(t => t.PayVoucherNo == PayVoucherNo),
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
            List<SelectListItem> paymentcurrency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "USD Payment", Text = "USD Payment" },
                    new SelectListItem { Value = "LKR Payment", Text = "LKR Payment" }
                };
            ViewData["ChargeItemsList"] = new SelectList(_context.Set<RefChargeItem>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ChargeId", "Description", "ChargeId");
            ViewData["paymentcurrencyList"] = new SelectList(paymentcurrency, "Value", "Text", "paymentcurrency");
            ViewData["UnitList"] = new SelectList(Unit, "Value", "Text", "UnitList");
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

        // POST: TxnPaymentVoucherHds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string dtlItemsList, [Bind("PayVoucherNo,Date,JobNo,Customer,ShippingLine,ExchangeRate,PayCurrency,TotalPayVoucherAmountLkr,TotalPayVoucherAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,DebitAcc,CreditAcc,Type")] TxnPaymentVoucherHd txnPaymentVoucherHd)
        {
            if (id != txnPaymentVoucherHd.PayVoucherNo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(dtlItemsList))
                    {
                        var rowsToDelete = _context.TxnPaymentVoucherDtls.Where(t => t.PayVoucherNo == id);
                        if (rowsToDelete != null || rowsToDelete.Any())
                        {
                            // Remove the rows from the database context
                            _context.TxnPaymentVoucherDtls.RemoveRange(rowsToDelete);
                        }
                        var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnPaymentVoucherDtl>>(dtlItemsList);
                        if (DtailItemdataTable != null)
                        {
                            foreach (var item in DtailItemdataTable)
                            {
                                TxnPaymentVoucherDtl DetailItem = new TxnPaymentVoucherDtl();
                                DetailItem.PayVoucherNo = id; // New CreditNote Number
                                DetailItem.SerialNo = item.SerialNo;
                                DetailItem.ChargeItem = item.ChargeItem;

                                // Add a null check for 'Unit'
                                DetailItem.Unit = item.Unit ?? "DefaultUnit"; // Use a default value if 'Unit' is null

                                DetailItem.Rate = item.Rate;
                                DetailItem.Currency = item.Currency;
                                DetailItem.Qty = item.Qty;
                                DetailItem.Amount = item.Amount;
                                DetailItem.BlContainerNo = item.BlContainerNo;

                                _context.TxnPaymentVoucherDtls.Add(DetailItem);
                            }

                        }
                    }
                    _context.Update(txnPaymentVoucherHd);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TxnPaymentVoucherHdExists(txnPaymentVoucherHd.PayVoucherNo))
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
            return View(txnPaymentVoucherHd);
        }

        // GET: TxnPaymentVoucherHds/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TxnPaymentVoucherHds == null)
            {
                return NotFound();
            }

            var txnPaymentVoucherHd = await _context.TxnPaymentVoucherHds
                .FirstOrDefaultAsync(m => m.PayVoucherNo == id);
            if (txnPaymentVoucherHd == null)
            {
                return NotFound();
            }

            return View(txnPaymentVoucherHd);
        }

        // POST: TxnPaymentVoucherHds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TxnPaymentVoucherHds == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.TxnPaymentVoucherHds'  is null.");
            }
            var txnPaymentVoucherHd = await _context.TxnPaymentVoucherHds.FindAsync(id);
            if (txnPaymentVoucherHd != null)
            {
                _context.TxnPaymentVoucherHds.Remove(txnPaymentVoucherHd);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TxnPaymentVoucherHdExists(string id)
        {
          return (_context.TxnPaymentVoucherHds?.Any(e => e.PayVoucherNo == id)).GetValueOrDefault();
        }
    }
}
