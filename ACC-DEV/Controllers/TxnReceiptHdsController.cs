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

namespace ACC_DEV.Views
{
    public class TxnReceiptHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnReceiptHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnReceiptHds = _context.TxnReceiptHDs.OrderByDescending(p => p.ReceiptNo);



            //const int pageSize = 7;
            //if (pg < 1)
            //    pg = 1;
            //int recsCount = txnReceiptHds.Count();
            //var pager = new Pager(recsCount, pg, pageSize);
            //int recSkip = (pg - 1) * pageSize;
            //var data = txnReceiptHds.Skip(recSkip).Take(pager.PageSize).ToList();

            //this.ViewBag.Pager = pager;
            return View(txnReceiptHds);
        }

        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchString)
        {

            var tables = new TxnReceiptViewMode
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == "xxx"),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == "xxx"),
                InvoiceHdMulti = _context.TxnInvoiceHds.ToList(),
                DebitNoteHdMulti = _context.TxnDebitNoteHds.ToList(),
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
            ViewData["CustomerList"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");
            ViewData["RefBankList"] = new SelectList(_context.Set<Ref_BankAcc>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ID", "Description", "ID");
            ViewData["AccountsCodes"] = new SelectList(
                                               _context.Set<RefChartOfAcc>()
                                                   .Where(a => a.IsInactive.Equals(false))
                                                   .OrderBy(p => p.AccCode)
                                                   .Select(a => new { AccNo = a.AccNo, DisplayValue = $"{a.AccCode} - {a.Description}" }),
                                               "AccNo",
                                               "DisplayValue",
                                               "AccNo"
            );
            ViewData["AgentIDNomination"] = new SelectList(_operationcontext.RefAgents.Join(_operationcontext.RefPorts,
     a => a.PortId,
     b => b.PortCode,
     (a, b) => new
     {
         AgentId = a.AgentId,
         AgentName = a.AgentName + " - " + b.PortName,
         IsActive = a.IsActive
     }).Where(a => a.IsActive.Equals(true)).OrderBy(a => a.AgentName), "AgentId", "AgentName", "AgentId");
            return View(tables);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("ReceiptNo,Date,ReceiptType,ExchangeRate,CustomerID,ChequeNo,ChequeDate,ChequeBankID,ChequeAmount,Remarks,ReceiptTotalAmt,DebitAcc,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason")] TxnReceiptHD txnReceiptHd)
        {

            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_ReceiptHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "RCPT" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnReceiptHd.ReceiptNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }
            txnReceiptHd.Canceled = false;
            txnReceiptHd.CreatedBy = "Admin";
            txnReceiptHd.CreatedDateTime = DateTime.Now;

            ModelState.Remove("ReceiptNo");  // Auto genrated
            if (ModelState.IsValid)
            {
                // Adding TxnReceiptDtl records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<TxnReceiptDtl>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnReceiptDtl
                            {
                                ReceiptNo = nextRefNo, // Set ReceiptNo to nextRefNo
                                RefNo = item.RefNo,
                                RefType = item.RefType,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now
                            };
                            _context.TxnReceiptDtls.Add(detailItem);
                        }
                    }
                }
            }
                try
                {

                    _context.Add(txnReceiptHd);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log or debug the exception
                    Console.WriteLine(ex.Message);
                    throw; // Rethrow the exception to see details in the browser
                }

                var tables = new TxnReceiptViewMode
                {
                    TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == "xxx"),
                    TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == "xxx"),
                    InvoiceHdMulti = _context.TxnInvoiceHds.ToList(),
                    DebitNoteHdMulti = _context.TxnDebitNoteHds.ToList(),
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

                ViewData["RefBankList"] = new SelectList(_context.Set<Ref_BankAcc>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ID", "Description", "ID");
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
        }
    }


