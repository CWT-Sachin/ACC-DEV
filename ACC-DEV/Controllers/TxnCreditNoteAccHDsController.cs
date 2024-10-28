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

namespace ACC_DEV.Controllers
{
    public class TxnCreditNoteAccHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public TxnCreditNoteAccHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnCreditNoteACCHDs = _context.TxnCreditNoteAccHDs
                .Include(t => t.AgentNavigation).OrderByDescending(p => p.CreditNoteNo).ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                txnCreditNoteACCHDs = txnCreditNoteACCHDs.Where(t => t.CreditNoteNo.Contains(searchString)).OrderByDescending(t => t.CreditNoteNo).ToList();
            }
            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnCreditNoteACCHDs.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnCreditNoteACCHDs.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }

        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier)
        {

            var txnCreditNoteHDs = _context.TxnCreditNoteAccHDs.Where(t => t.CreditNoteNo == "xxx");

            var tables = new TxnCreditNoteAccViewModel
            {
                TxnCreditNoteAccHDMulti = _context.TxnCreditNoteAccHDs.Where(t => t.CreditNoteNo == "xxx"),
                TxnCreditNoteAccDtlMulti = _context.TxnCreditNoteAccDtls.Where(t => t.CreditNoteNo == "xxx"),
            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["ChargeItemsList"] = new SelectList(_context.RefChargeItems.OrderBy(c => c.Description), "ChargeId", "Description", "ChargeId");

            ViewData["ChartofAccounts"] = new SelectList(_context.RefChartOfAccs.OrderBy(c => c.AccName), "AccNo", "AccName", "AccNo");
            ViewData["RefBankList"] = new SelectList(_context.Set<RefBankAcc>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ID", "Description", "ID");
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

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("CreditNoteNo,Date,ExchangeRate,Agent,DocType,JobNo,BLNo,MainAcc,Narration,MainAccAmount,TotalAmountLKR,TotalAmountUSD,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,AmountPaid,AmountToBePaid,TotAmtWord,TotAmtWordUSD,JobType")] TxnCreditNoteAccHD txnCreditNoteAccHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_CreditNoteAccHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "CTA" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnCreditNoteAccHD.CreditNoteNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }

            txnCreditNoteAccHD.AmountPaid = 0;
            txnCreditNoteAccHD.AmountToBePaid = txnCreditNoteAccHD.TotalAmountLKR;
            txnCreditNoteAccHD.Approved = false;

            txnCreditNoteAccHD.Canceled = false;
            txnCreditNoteAccHD.CreatedBy = "Admin";
            txnCreditNoteAccHD.CreatedDateTime = DateTime.Now;

            // Amt LKR to word 
            var TotalAmount = txnCreditNoteAccHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnCreditNoteAccHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            // Amt USD to word 
            var TotalAmountUSd = txnCreditNoteAccHD.TotalAmountUSD;
            txnCreditNoteAccHD.TotAmtWordUSD = CommonMethodClass.ConvertToWords((decimal)TotalAmountUSd);

            ModelState.Remove("CreditNoteNo");  // Auto generated
            ModelState.Remove("BLNo");  // Not in use
            ModelState.Remove("CreatedBy");  // Assigned
            ModelState.Remove("CanceledReason");  // Not in use
            ModelState.Remove("CanceledBy");  // Not in use
            ModelState.Remove("ApprovedBy");  // Not in use
            ModelState.Remove("LastUpdatedBy");  // Not in use

            if (ModelState.IsValid)
            {
                // Adding TxnPaymentDtl records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedCreditNoteAccItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {

                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnCreditNoteAccDtl
                            {
                                CreditNoteNo = nextRefNo, // Set PaymentNo to nextRefNo
                                SerialNo = item.SerialNo,// (decimal)item.SerialNo,
                                LineAccNo = item.LineAccNo,
                                ChargeItem = item.ChargeItem,
                                Description = item.Description,
                                Amount = (decimal)item.Amount,
                                CreatedDateTime = DateTime.Now,

                            };
                            _context.TxnCreditNoteAccDtls.Add(detailItem);
                        }
                    }
                }
                try
                {

                    _context.Add(txnCreditNoteAccHD);
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




            var tables = new TxnCreditNoteAccViewModel
            {
                TxnCreditNoteAccHDMulti = _context.TxnCreditNoteAccHDs.Where(t => t.CreditNoteNo == "xxx"),
                TxnCreditNoteAccDtlMulti = _context.TxnCreditNoteAccDtls.Where(t => t.CreditNoteNo == "xxx"),
            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["ChargeItemsList"] = new SelectList(_context.RefChargeItems.OrderBy(c => c.Description), "ChargeId", "Description", "ChargeId");

            ViewData["ChartofAccounts"] = new SelectList(_context.RefChartOfAccs.OrderBy(c => c.AccName), "AccNo", "AccName", "AccNo");
            ViewData["RefBankList"] = new SelectList(_context.Set<RefBankAcc>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ID", "Description", "ID");
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

        // GET: Credit Sales/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var VoucherNo = "";

            if (string.IsNullOrWhiteSpace(id))
            {
                return View("Error");
            }
            else
            {
                VoucherNo = id;
            }

            var tables = new TxnCreditNoteAccViewModel
            {
                TxnCreditNoteAccHDMulti = _context.TxnCreditNoteAccHDs.Where(t => t.CreditNoteNo == id),
                TxnCreditNoteAccDtlMulti = _context.TxnCreditNoteAccDtls.Where(t => t.CreditNoteNo == id),

            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["ChargeItemsList"] = new SelectList(_context.RefChargeItems.OrderBy(c => c.Description), "ChargeId", "Description", "ChargeId");

            ViewData["ChartofAccounts"] = new SelectList(_context.RefChartOfAccs.OrderBy(c => c.AccName), "AccNo", "AccName", "AccNo");
            ViewData["RefBankList"] = new SelectList(_context.Set<RefBankAcc>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ID", "Description", "ID");
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

        //GET 
        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnCreditNoteAccHDs = await _context.TxnCreditNoteAccHDs
                .FirstOrDefaultAsync(m => m.CreditNoteNo == id);


            if (txnCreditNoteAccHDs == null)
            {
                return NotFound();
            }

            var jobNo = txnCreditNoteAccHDs.JobNo; // Set the jobNo property

            var tables = new TxnCreditNoteAccViewModel
            {
                TxnCreditNoteAccHDMulti = _context.TxnCreditNoteAccHDs.Where(t => t.CreditNoteNo == id),
                TxnCreditNoteAccDtlMulti = _context.TxnCreditNoteAccDtls.Where(t => t.CreditNoteNo == id),

            };

            return View(tables);
        }

        // POST 
        [HttpPost]
        public async Task<IActionResult> Approve(string id, bool approved)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnCreditNoteAccHDs = await _context.TxnCreditNoteAccHDs
                .FirstOrDefaultAsync(m => m.CreditNoteNo == id);

            if (txnCreditNoteAccHDs == null)
            {
                return NotFound();
            }

            // Inserting Transaction Data to Acccounts 
            var txnCreditNoteAccDtls = await _context.TxnCreditNoteAccDtls
                                .Where(m => m.CreditNoteNo == id)
                                .ToListAsync();

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

            if (txnCreditNoteAccHDs != null)
            {
                var SerialNo_AccTxn = 1;
                var AccTxnDescription = txnCreditNoteAccHDs.Narration;
                // First Acc transaction 
                TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
                NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
                NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
                NewRowAccTxnFirst.Date = txnCreditNoteAccHDs.Date; //  Jurnal Date 
                NewRowAccTxnFirst.Description = AccTxnDescription;
                NewRowAccTxnFirst.TxnAccCode = txnCreditNoteAccHDs.MainAcc;
                NewRowAccTxnFirst.Dr = (decimal)0;
                NewRowAccTxnFirst.Cr = (decimal)txnCreditNoteAccHDs.TotalAmountLKR;
                NewRowAccTxnFirst.RefNo = txnCreditNoteAccHDs.CreditNoteNo; // Invoice No
                NewRowAccTxnFirst.Note = "";
                NewRowAccTxnFirst.Reconciled = false;
                NewRowAccTxnFirst.DocType = "CreditNTAdd";
                NewRowAccTxnFirst.IsMonthEndDone = false;
                NewRowAccTxnFirst.CreatedBy = "Admin";
                NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
                NewRowAccTxnFirst.Canceled = false;

                NewRowAccTxnFirst.JobNo = txnCreditNoteAccHDs.JobNo;
                NewRowAccTxnFirst.JobType = txnCreditNoteAccHDs.JobType;

                _context.TxnTransactions.Add(NewRowAccTxnFirst);

                foreach (var item in txnCreditNoteAccDtls)
                {
                    // Transaction table Insert 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();

                    var lineLKRAmt = (decimal)item.Amount * txnCreditNoteAccHDs.ExchangeRate;
                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnCreditNoteAccHDs.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = item.Description;
                    NewRowAccTxn.TxnAccCode = item.LineAccNo;
                    NewRowAccTxn.Dr = (decimal)lineLKRAmt; // amount * exchange rate 
                    NewRowAccTxn.Cr = (decimal)0; 
                    NewRowAccTxn.RefNo = txnCreditNoteAccHDs.CreditNoteNo; // Invoice No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "CreditNTAdd";
                    NewRowAccTxn.IsMonthEndDone = false;
                    NewRowAccTxn.CreatedBy = "Admin";
                    NewRowAccTxn.CreatedDateTime = DateTime.Now;
                    NewRowAccTxn.Canceled = false;
                    NewRowAccTxn.JobNo = txnCreditNoteAccHDs.JobNo;
                    NewRowAccTxn.JobType = txnCreditNoteAccHDs.JobType;

                    _context.TxnTransactions.Add(NewRowAccTxn);
                }
            }
            // END Inserting Transaction Data to Acccounts 

            /// Update the Approved property based on the form submission
            txnCreditNoteAccHDs.Approved = approved;
            txnCreditNoteAccHDs.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnCreditNoteAccHDs.ApprovedDateTime = DateTime.Now;

            _context.Update(txnCreditNoteAccHDs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }

        // validate Job Number on the Create and Edit 
        [HttpPost]
        public JsonResult ValidateJobNo(string jobNo, string jobType)
        {
            bool isValid = false;
            string message = string.Empty;

            if (jobType == "Import")
            {
                isValid = _operationcontext.TxnImportJobHds.Any(j => j.JobNo == jobNo);
                if (!isValid)
                {
                    message = "Invalid Import Job No";
                }
                else
                {
                    message = "Import Job Number validated SUCCESSFULLY";
                }
            }
            else if (jobType == "Export")
            {
                isValid = _operationcontext.TxnExportJobHds.Any(j => j.JobNo == jobNo);
                if (!isValid)
                {
                    message = "Invalid Export Job No";
                }
                else
                {
                    message = "Export Job Number validated SUCCESSFULLY";
                }
            }
            else if (jobType == "AirExport" || jobType == "AirImport")
            {
                isValid = true;
                message = " ";
            }
                return Json(new { isValid = isValid, message = message });
        }
    }
}
