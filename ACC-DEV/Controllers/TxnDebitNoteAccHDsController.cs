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
using ACC_DEV.CommonMethods;
using Microsoft.AspNetCore.Authorization;


namespace ACC_DEV.Views
{
    //[Authorize]
    public class TxnDebitNoteAccHDsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnDebitNoteAccHDsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        //public async Task<IActionResult> ConvertToWord(string searchString)
        //{
        //    if (!String.IsNullOrEmpty(searchString))
        //    {
        //        var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 
        //        var Amount = Convert.ToDecimal(searchString);
        //        // Check if results are found

        //            ViewData["Amount"] = Amount;

        //        ViewData["AmountInword"] = CommonMethodClass.ConvertToWords(Amount);
                
        //    }

        //    return View();
        //}
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnDebitNoteACCHDs = await _context.TxnDebitNoteAccHDs.OrderByDescending(p => p.DebitNoteNo).ToListAsync();

            var viewModel = new DebitNoteAccViewModel
            {
                TxnDebitNoteAccHDMulti = txnDebitNoteACCHDs,
            };

            return View(viewModel);
        }


        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier)
        {


            var txnDebitNoteHDs = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == "xxx");


            var tables = new DebitNoteAccViewModel
            {
                TxnDebitNoteAccHDMulti = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == "xxx"),
                TxnDebitNoteAccDtlMulti = _context.TxnDebitNoteAccDtls.Where(t => t.DebitNoteNo == "xxx"),

            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpts.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["ChargeItemsList"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

            ViewData["ChartofAccounts"] = new SelectList(_context.RefChartOfAccs.OrderBy(c => c.AccName), "AccNo", "AccName", "AccNo");
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
        public async Task<IActionResult> Create(string dtlItemsList, string customerType, [Bind("DebitNoteNo,Date,ExchangeRate,Agent,DocType,JobNo,BLNo,MainAcc,Narration,MainAccAmount,TotalInvoiceAmountLKR,TotalInvoiceAmountUSD,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,AmountPaid,AmountToBePaid")] TxnDebitNoteAccHD txnDebitNoteHd)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_DebitNoteAccHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "DBACC" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
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


            ModelState.Remove("DebitNoteNo");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("ApprovedBy");
            ModelState.Remove("CanceledBy");
            ModelState.Remove("LastUpdatedBy");
            ModelState.Remove("CanceledReason"); 
            ModelState.Remove("BLNo");
            ModelState.Remove("customerType");
            if (ModelState.IsValid)
            {
                // Create Var for Additional Payment Voucher
                // swith 
                var voucherNo = "xxx";
                var DebitNoteTable = await _context.TxnDebitNoteHds.FindAsync(voucherNo);

                // Adding TxnPaymentDtl records

                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<TxnDebitNoteAccDtl>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnDebitNoteAccDtl
                            {
                                DebitNoteNo = nextRefNo, // Set PaymentNo to nextRefNo
                                SerialNo = item.SerialNo,
                                LineAccNo = item.LineAccNo,
                                ChargeItem = item.ChargeItem,
                                Description = item.Description,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,

                        };
                            _context.TxnDebitNoteAccDtls.Add(detailItem);
                        }
                    }
                }
                try
                {

                    _context.Add(txnDebitNoteHd);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log or debug the exception
                    Console.WriteLine(ex.Message);
                    throw; // Rethrow the exception to see details in the browser
                }
            }


            var tables = new DebitNoteAccViewModel
            {
                TxnDebitNoteAccHDMulti = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == "xxx"),
                TxnDebitNoteAccDtlMulti = _context.TxnDebitNoteAccDtls.Where(t => t.DebitNoteNo == "xxx"),

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
            ViewData["ChargeItemsList"] = new SelectList(_context.Set<RefChargeItem>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ChargeId", "Description", "ChargeId");

            return View(tables);
            }
        }

   

    }


