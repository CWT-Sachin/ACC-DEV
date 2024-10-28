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
    public class TxnDebitNoteAccHdsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public TxnDebitNoteAccHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnDebitNoteACCHDs = _context.TxnDebitNoteAccHDs
                .Include(t => t.AgentNavigation).OrderByDescending(p => p.DebitNoteNo).ToList();



            if (!String.IsNullOrEmpty(searchString))
            {
                txnDebitNoteACCHDs = txnDebitNoteACCHDs.Where(t => t.DebitNoteNo.Contains(searchString)).OrderByDescending(t => t.DebitNoteNo).ToList();
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnDebitNoteACCHDs.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnDebitNoteACCHDs.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }

        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier)
        {

            var txnDebitNoteHDs = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == "xxx");

            var tables = new TxnDebitNoteAccViewModel
            {
                TxnDebitNoteAccHDMulti = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == "xxx"),
                TxnDebitNoteAccDtlMulti = _context.TxnDebitNoteAccDtls.Where(t => t.DebitNoteNo == "xxx"),
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
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("DebitNoteNo,Date,ExchangeRate,Agent,DocType,JobNo,BLNo,MainAcc,Narration,MainAccAmount,TotalAmountLKR,TotalAmountUSD,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,AmountPaid,AmountToBePaid,TotAmtWord,TotAmtWordUSD,JobType")] TxnDebitNoteAccHD txnDebitNoteAccHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_DebitNoteAccHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "DTA" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnDebitNoteAccHD.DebitNoteNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }

            txnDebitNoteAccHD.AmountPaid = 0;
            txnDebitNoteAccHD.AmountToBePaid = txnDebitNoteAccHD.TotalAmountLKR;
            txnDebitNoteAccHD.Approved = false;

            txnDebitNoteAccHD.Canceled = false;
            txnDebitNoteAccHD.CreatedBy = "Admin";
            txnDebitNoteAccHD.CreatedDateTime = DateTime.Now;

            // Amt LKR to word 
            var TotalAmount = txnDebitNoteAccHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnDebitNoteAccHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            // Amt USD to word 
            var TotalAmountUSd = txnDebitNoteAccHD.TotalAmountUSD;
            txnDebitNoteAccHD.TotAmtWordUSD = CommonMethodClass.ConvertToWords((decimal)TotalAmountUSd);

            ModelState.Remove("DebitNoteNo");  // Auto generated
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
                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedDebitNoteAccItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {

                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnDebitNoteAccDtl
                            {
                                DebitNoteNo = nextRefNo, // Set PaymentNo to nextRefNo
                                SerialNo = item.SerialNo,// (decimal)item.SerialNo,
                                LineAccNo = item.LineAccNo,
                                ChargeItem = item.ChargeItem,
                                Description = item.Description,
                                Amount = (decimal)item.Amount,
                                CreatedDateTime = DateTime.Now,

                            };
                            _context.TxnDebitNoteAccDtls.Add(detailItem);
                        }
                    }
                }
                try
                {

                    _context.Add(txnDebitNoteAccHD);
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


            var tables = new TxnDebitNoteAccViewModel
            {
                TxnDebitNoteAccHDMulti = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == "xxx"),
                TxnDebitNoteAccDtlMulti = _context.TxnDebitNoteAccDtls.Where(t => t.DebitNoteNo == "xxx"),

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

            var tables = new TxnDebitNoteAccViewModel
            {
                TxnDebitNoteAccHDMulti = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == id),
                TxnDebitNoteAccDtlMulti = _context.TxnDebitNoteAccDtls.Where(t => t.DebitNoteNo == id),

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

        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnDebitNoteAccHDs = await _context.TxnDebitNoteAccHDs
                .FirstOrDefaultAsync(m => m.DebitNoteNo == id);


            if (txnDebitNoteAccHDs == null)
            {
                return NotFound();
            }

            var jobNo = txnDebitNoteAccHDs.JobNo; // Set the jobNo property

            var tables = new TxnDebitNoteAccViewModel
            {
                TxnDebitNoteAccHDMulti = _context.TxnDebitNoteAccHDs.Where(t => t.DebitNoteNo == id),
                TxnDebitNoteAccDtlMulti = _context.TxnDebitNoteAccDtls.Where(t => t.DebitNoteNo == id),

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

            var txnDebitNoteAccHDs = await _context.TxnDebitNoteAccHDs
                .FirstOrDefaultAsync(m => m.DebitNoteNo == id);

            if (txnDebitNoteAccHDs == null)
            {
                return NotFound();
            }

            // Inserting Transaction Data to Acccounts 
            var txnDebitNoteAccDtls = await _context.TxnDebitNoteAccDtls
                                .Where(m => m.DebitNoteNo == id)
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

            if (txnDebitNoteAccHDs != null)
            {
                var SerialNo_AccTxn = 1;
                var AccTxnDescription = txnDebitNoteAccHDs.Narration;
                // First Acc transaction 
                TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
                NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
                NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
                NewRowAccTxnFirst.Date = txnDebitNoteAccHDs.Date; //  Jurnal Date 
                NewRowAccTxnFirst.Description = AccTxnDescription;
                NewRowAccTxnFirst.TxnAccCode = txnDebitNoteAccHDs.MainAcc;
                NewRowAccTxnFirst.Dr = (decimal)txnDebitNoteAccHDs.TotalAmountLKR;
                NewRowAccTxnFirst.Cr = (decimal)0;
                NewRowAccTxnFirst.RefNo = txnDebitNoteAccHDs.DebitNoteNo; // Invoice No
                NewRowAccTxnFirst.Note = "";
                NewRowAccTxnFirst.Reconciled = false;
                NewRowAccTxnFirst.DocType = "DebitNTAdd";
                NewRowAccTxnFirst.IsMonthEndDone = false;
                NewRowAccTxnFirst.CreatedBy = "Admin";
                NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
                NewRowAccTxnFirst.Canceled = false;

                NewRowAccTxnFirst.JobNo = txnDebitNoteAccHDs.JobNo;
                NewRowAccTxnFirst.JobType = txnDebitNoteAccHDs.JobType;

                _context.TxnTransactions.Add(NewRowAccTxnFirst);

                foreach (var item in txnDebitNoteAccDtls)
                {
                    // Transaction table Insert 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();

                    var lineLKRAmt = (decimal)item.Amount * txnDebitNoteAccHDs.ExchangeRate;
                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnDebitNoteAccHDs.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = item.Description;
                    NewRowAccTxn.TxnAccCode = item.LineAccNo;
                    NewRowAccTxn.Dr = (decimal)0;
                    NewRowAccTxn.Cr = (decimal)lineLKRAmt; // amount 8 exchange rate 
                    NewRowAccTxn.RefNo = txnDebitNoteAccHDs.DebitNoteNo; // Invoice No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "DebitNTAdd";
                    NewRowAccTxn.IsMonthEndDone = false;
                    NewRowAccTxn.CreatedBy = "Admin";
                    NewRowAccTxn.CreatedDateTime = DateTime.Now;
                    NewRowAccTxn.Canceled = false;
                    NewRowAccTxn.JobNo = txnDebitNoteAccHDs.JobNo;
                    NewRowAccTxn.JobType = txnDebitNoteAccHDs.JobType;

                    _context.TxnTransactions.Add(NewRowAccTxn);
                }
            }
            // END Inserting Transaction Data to Acccounts 

            /// Update the Approved property based on the form submission
            txnDebitNoteAccHDs.Approved = approved;
            txnDebitNoteAccHDs.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnDebitNoteAccHDs.ApprovedDateTime = DateTime.Now;

            _context.Update(txnDebitNoteAccHDs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }

    }
} 
