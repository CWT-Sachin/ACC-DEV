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
using Microsoft.Identity.Client;
using System.Data;

namespace ACC_DEV.Views
{
    public class TxnPurchasVoucherHDsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnPurchasVoucherHDsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        public async Task<IActionResult> ConvertToWord(string searchString)
        {
            if (!String.IsNullOrEmpty(searchString))
            {
                var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 
                var Amount = Convert.ToDecimal(searchString);
                // Check if results are found
                ViewData["Amount"] = Amount;
                ViewData["AmountInword"] = CommonMethodClass.ConvertToWords(Amount);
                
            }
            return View();
        }

        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnPurchasVoucherHDs = await _context.TxnPurchasVoucherHDs.OrderByDescending(p => p.PurchasVoucherNo)
                .Include(t=> t.SupplierNavigation)
                .Include(t => t.ShippingLineNavigation)
                .ToListAsync();


            if (!String.IsNullOrEmpty(searchString))
            {
                txnPurchasVoucherHDs = txnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo.Contains(searchString)).OrderByDescending(t => t.PurchasVoucherNo).ToList();
            }

            //var viewModel = new TxnPaymentViewMode
            //{
            //    TxnPaymentHdMulti = txnPaymentHDs,
            //    // Initialize other properties of the view model if needed
            //};

            //return View(viewModel);

            const int pageSize = 25;
            if (pg < 1)
                pg = 1;
            int recsCount = txnPurchasVoucherHDs.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnPurchasVoucherHDs.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }


        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier)
        {
            
            
            var txnPurchasVoucherHD = _context.TxnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo == "xxx");


            var tables = new TxnPurchasVoucherViewModel
            {
                TxnPurchasVoucherHDMulti = _context.TxnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo == "xxx"),
                TxnPurchasVoucherDtlMulti =_context.TxnPurchasVoucherDtls.Where(t => t.PurchasVoucherNo == "xxx"),

            };


            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["CustomerList"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList,  [Bind("PurchasVoucherNo,Date,ExchangeRate,CreditorType,Supplier,ShippingLine,DocType,JobNo,Blno,MainAcc,Narration,MainAccAmount,TotalAmountLKR,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,AmountPaid,AmountToBePaid,TotAmtWord,JobType")] TxnPurchasVoucherHD txnPurchasVoucherHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_PurchasVoucherHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "PV" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnPurchasVoucherHD.PurchasVoucherNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }

            txnPurchasVoucherHD.AmountPaid = 0;
            txnPurchasVoucherHD.AmountToBePaid = txnPurchasVoucherHD.TotalAmountLKR;
            txnPurchasVoucherHD.Approved = false;

            txnPurchasVoucherHD.Canceled = false;
            txnPurchasVoucherHD.CreatedBy = "Admin";
            txnPurchasVoucherHD.CreatedDateTime = DateTime.Now;


            var TotalAmount = txnPurchasVoucherHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnPurchasVoucherHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            ModelState.Remove("PurchasVoucherNo");  // Auto generated
            if (ModelState.IsValid)
            {
                // Create Var for Additional Payment Voucher
                // swith 
                var voucherNo = "xxx";
                var PayVoutuerTable = await _context.TxnPaymentVoucherHds.FindAsync(voucherNo);

                // Adding TxnPaymentDtl records

                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedPurchasItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {



                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnPurchasVoucherDtl
                            {
                                PurchasVoucherNo = nextRefNo, // Set PaymentNo to nextRefNo
                                SerialNo = (decimal)item.SerialNo,
                                LineAccNo = item.LineAccNo,
                                Description = item.Description, 
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,

                            };
                            _context.TxnPurchasVoucherDtls.Add(detailItem);
                        }
                    }
                }
            try
                {

                    _context.Add(txnPurchasVoucherHD);
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

            var tables = new TxnPurchasVoucherViewModel
            {
                TxnPurchasVoucherHDMulti = _context.TxnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo == "xxx"),
                TxnPurchasVoucherDtlMulti = _context.TxnPurchasVoucherDtls.Where(t => t.PurchasVoucherNo == "xxx"),

            };



            ViewData["ShippingLine"] = new SelectList(_operationcontext.RefShippingLines.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["CustomerList"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

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

            return Json(new { isValid = isValid, message = message });
        }

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

            var tables = new TxnPurchasVoucherViewModel
            {
                TxnPurchasVoucherHDMulti = _context.TxnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo == VoucherNo),
                TxnPurchasVoucherDtlMulti = _context.TxnPurchasVoucherDtls.Where(t => t.PurchasVoucherNo == VoucherNo),
            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["CustomerList"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

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

        // GET: Approve
        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var txnPurchasVoucherHD = await _context.TxnPurchasVoucherHDs.FirstOrDefaultAsync(m => m.PurchasVoucherNo== id);    
            //var txnInvoiceExportHd = await _context.TxnInvoiceExportHds
            //    .FirstOrDefaultAsync(m => m.InvoiceNo == id);

            if (txnPurchasVoucherHD == null)
            {
                return NotFound();
            }


            jobNo = txnPurchasVoucherHD.JobNo; // Set the jobNo property

            var tables = new TxnPurchasVoucherViewModel
            {
                TxnPurchasVoucherHDMulti = _context.TxnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo == id),
                TxnPurchasVoucherDtlMulti = _context.TxnPurchasVoucherDtls.Where(t => t.PurchasVoucherNo == id),
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

            var txnPurchasVoucherHD = await _context.TxnPurchasVoucherHDs.FirstOrDefaultAsync(m => m.PurchasVoucherNo == id);

            if (txnPurchasVoucherHD == null)
            {
                return NotFound();
            }

            // Inserting Transaction Data to Acccounts 
            var txnPurchasVoucherDtls = await _context.TxnPurchasVoucherDtls
                .Where(m => m.PurchasVoucherNo == id)
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

            if (txnPurchasVoucherHD != null)
            {
                var SerialNo_AccTxn = 1;
                var AccTxnDescription = txnPurchasVoucherHD.Narration;
                // First Acc transaction 
                TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
                NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
                NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
                NewRowAccTxnFirst.Date = txnPurchasVoucherHD.Date; //  Jurnal Date 
                NewRowAccTxnFirst.Description = AccTxnDescription;
                NewRowAccTxnFirst.TxnAccCode = txnPurchasVoucherHD.MainAcc;
                NewRowAccTxnFirst.Dr = (decimal)0;
                NewRowAccTxnFirst.Cr = (decimal)txnPurchasVoucherHD.TotalAmountLKR;
                NewRowAccTxnFirst.RefNo = txnPurchasVoucherHD.PurchasVoucherNo; // Invoice No
                NewRowAccTxnFirst.Note = "";
                NewRowAccTxnFirst.Reconciled = false;
                NewRowAccTxnFirst.DocType = "PurchasVoucher";
                NewRowAccTxnFirst.IsMonthEndDone = false;
                NewRowAccTxnFirst.CreatedBy = "Admin";
                NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
                NewRowAccTxnFirst.Canceled = false;

                NewRowAccTxnFirst.JobNo = txnPurchasVoucherHD.JobNo;
                NewRowAccTxnFirst.JobType = txnPurchasVoucherHD.JobType;

                _context.TxnTransactions.Add(NewRowAccTxnFirst);

                foreach (var item in txnPurchasVoucherDtls)
                {
                    // Transaction table Insert 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();

                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnPurchasVoucherHD.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = item.Description;
                    NewRowAccTxn.TxnAccCode = item.LineAccNo;
                    NewRowAccTxn.Dr = (decimal)item.Amount;
                    NewRowAccTxn.Cr =  (decimal)0;

                    NewRowAccTxn.RefNo = txnPurchasVoucherHD.PurchasVoucherNo; // Invoice No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "PurchasVoucher";
                    NewRowAccTxn.IsMonthEndDone = false;
                    NewRowAccTxn.CreatedBy = "Admin";
                    NewRowAccTxn.CreatedDateTime = DateTime.Now;
                    NewRowAccTxn.Canceled = false;

                    NewRowAccTxn.JobNo = txnPurchasVoucherHD.JobNo;
                    NewRowAccTxn.JobType = txnPurchasVoucherHD.JobType;

                    _context.TxnTransactions.Add(NewRowAccTxn);
                }
            }
            // END Inserting Transaction Data to Acccounts 

            /// Update the Approved property based on the form submission
            txnPurchasVoucherHD.Approved = approved;
            txnPurchasVoucherHD.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnPurchasVoucherHD.ApprovedDateTime = DateTime.Now;

            _context.Update(txnPurchasVoucherHD);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }

        // GET: TxnPurchasVoucherHDs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.TxnPurchasVoucherHDs == null)
            {
                return NotFound();
            }

            var txnPurchasVoucherHD = await _context.TxnPurchasVoucherHDs.FindAsync(id);
            if (txnPurchasVoucherHD == null)
            {
                return NotFound();
            }

            var tables = new TxnPurchasVoucherViewModel
            {
                TxnPurchasVoucherHDMulti = _context.TxnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo == id),
                TxnPurchasVoucherDtlMulti = _context.TxnPurchasVoucherDtls.Where(t => t.PurchasVoucherNo == id),
            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", txnPurchasVoucherHD.ShippingLine);
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", txnPurchasVoucherHD.Supplier);
            ViewData["CustomerList"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", txnPurchasVoucherHD.Supplier);
            ViewData["ChartofAccounts"] = new SelectList(_context.RefChartOfAccs.OrderBy(c => c.AccName), "AccNo", "AccName", txnPurchasVoucherHD.MainAcc);
            ViewData["RefBankList"] = new SelectList(_context.Set<RefBankAcc>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ID", "Description");
            ViewData["AccountsCodes"] = new SelectList(
                _context.Set<RefChartOfAcc>()
                    .Where(a => a.IsInactive.Equals(false))
                    .OrderBy(p => p.AccCode)
                    .Select(a => new { AccNo = a.AccNo, DisplayValue = $"{a.AccCode} - {a.Description}" }),
                "AccNo",
                "DisplayValue",
                txnPurchasVoucherHD.MainAcc
            );

            ViewData["AgentIDNomination"] = new SelectList(_operationcontext.RefAgents.Join(_operationcontext.RefPorts,
                a => a.PortId,
                b => b.PortCode,
                (a, b) => new
                {
                    AgentId = a.AgentId,
                    AgentName = a.AgentName + " - " + b.PortName,
                    IsActive = a.IsActive
                }).Where(a => a.IsActive.Equals(true)).OrderBy(a => a.AgentName), "AgentId", "AgentName", txnPurchasVoucherHD.JobNo);

            return View(tables);
        }
        // POST: TxnPurchasVoucherHDs/Edit/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string dtlItemsList, [Bind("PurchasVoucherNo,Date,ExchangeRate,CreditorType,Supplier,ShippingLine,DocType,JobNo,Blno,MainAcc,Narration,MainAccAmount,TotalAmountLKR,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,AmountPaid,AmountToBePaid,TotAmtWord,JobType")] TxnPurchasVoucherHD txnPurchasVoucherHD)
        {
            if (id != txnPurchasVoucherHD.PurchasVoucherNo)
            {
                return NotFound();
            }

            txnPurchasVoucherHD.LastUpdatedBy = "Admin";
            txnPurchasVoucherHD.LastUpdatedDateTime = DateTime.Now;

            txnPurchasVoucherHD.AmountPaid = 0;
            txnPurchasVoucherHD.AmountToBePaid = txnPurchasVoucherHD.TotalAmountLKR;

            var TotalAmount = txnPurchasVoucherHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnPurchasVoucherHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(dtlItemsList))
                    {
                        var rowsToDelete = _context.TxnPurchasVoucherDtls.Where(t => t.PurchasVoucherNo == id);
                        if (rowsToDelete != null && rowsToDelete.Any())
                        {
                            _context.TxnPurchasVoucherDtls.RemoveRange(rowsToDelete);
                        }

                        var detailItemList = JsonConvert.DeserializeObject<List<SelectedPurchasItem>>(dtlItemsList);
                        if (detailItemList != null)
                        {
                            foreach (var item in detailItemList)
                            {
                                var detailItem = new TxnPurchasVoucherDtl
                                {
                                    PurchasVoucherNo = id,
                                    SerialNo = (decimal)item.SerialNo,
                                    LineAccNo = item.LineAccNo,
                                    Description = item.Description,
                                    Amount = item.Amount,
                                    CreatedDateTime = DateTime.Now,
                                };
                                _context.TxnPurchasVoucherDtls.Add(detailItem);
                            }
                        }
                    }

                    _context.Update(txnPurchasVoucherHD);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TxnPurchasVoucherHdExists(txnPurchasVoucherHD.PurchasVoucherNo))
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

            return View(txnPurchasVoucherHD);
        }

        private bool TxnPurchasVoucherHdExists(string id)
        {
            return _context.TxnPurchasVoucherHDs.Any(e => e.PurchasVoucherNo == id);
        }

        // following function will e used in Tools -sample only - remaove later
        public async Task<IActionResult> SyncRefChargeItemAcc()
        {
            // Fetch all records from _context.Ref_ChargeItem  -ACCOUNTS
            var refChargeItems = await _context.RefChargeItems.ToListAsync();

            foreach (var item in refChargeItems)
            {
                // Check if the record exists in _operationcontext.Ref_ChargeItemAcc  - OPERATION
                var existingItem = await _operationcontext.RefChargeItemAccs
                    .FirstOrDefaultAsync(x => x.ChargeId == item.ChargeId);

                if (existingItem != null)
                {
                    // Update the existing record
                    existingItem.Description = item.Description; // Update properties as needed
                    existingItem.IsActive = item.IsActive;
                    // Continue for all properties
                }
                else
                {
                    // Insert new record
                    _operationcontext.RefChargeItemAccs.Add(new RefChargeItemAcc
                    {
                        ChargeId = item.ChargeId,
                        Description = item.Description,
                        IsActive = item.IsActive,
                        // Continue for all properties
                    });
                }
            }

            // Save changes to the database
            await _operationcontext.SaveChangesAsync();

            return Ok("Upsert operation completed.");
        }

    }
}


