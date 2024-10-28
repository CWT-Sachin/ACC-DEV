using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using ACC_DEV.ViewModel;
using Newtonsoft.Json;
using ACC_DEV.Data;
using ACC_DEV.DataOperation;
using ACC_DEV.CommonMethods;
using Microsoft.Identity.Client;
using System.Data;


namespace ACC_DEV.Views
{
    public class TxnPettyCashHDsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;



        public TxnPettyCashHDsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }

        // validate Purchas / Payment Voucer  Number on the Create and Edit 
        [HttpPost]
        public JsonResult ValidatePVNo(string jobNo, string jobType)
        {
            bool isValid = false;
            string message = string.Empty;
            string TotalAmt = string.Empty;
            string PaidAmt = string.Empty;
            string BalanceAmt = string.Empty;
            string CreditorName = string.Empty;

            if (jobType == "Import")
            {
                isValid = _operationcontext.TxnPaymentVoucherImportHds.Any(j => j.PayVoucherNo == jobNo);
                if (!isValid)
                {
                    message = "Invalid Import PaymentVoucher No";
                }
                else
                {
                    message = "Import Payment Voucher Number validated SUCCESSFULLY";

                    decimal totalAmountLKR = 0;
                    decimal AmountPaidDec = 0;
                    decimal AmountToBePaidDec = 0;
                    var CreditorType = "";
                    var AgentName = "";
                    var ShippingLineName = "";

                    var txnPaymentVoucherImpHDs = _operationcontext.TxnPaymentVoucherImportHds.Where(j => j.PayVoucherNo == jobNo)
                       .ToList();
                    var joinedData = from t in txnPaymentVoucherImpHDs
                                     join a in _operationcontext.RefAgents on t.AgentID equals a.AgentId into agentGroup
                                     from a in agentGroup.DefaultIfEmpty()
                                     join s in _operationcontext.RefShippingLines on t.Customer equals s.ShippingLineId into shippingLineGroup
                                     from s in shippingLineGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         t.TotalPayVoucherAmountLkr,
                                         t.AmountPaid,
                                         t.AmountToBePaid,
                                         t.Type,
                                         AgentName = a != null ? a.AgentName : null,
                                         ShippingLineName = s != null ? s.Name : null
                                     };


                    var firstRecord = joinedData.FirstOrDefault();

                    if (firstRecord != null)
                    {
                        totalAmountLKR = (decimal)firstRecord.TotalPayVoucherAmountLkr;
                        AmountPaidDec = (decimal)firstRecord.AmountPaid;
                        AmountToBePaidDec = (decimal)firstRecord.AmountToBePaid;
                        CreditorType = firstRecord.Type;
                        AgentName = firstRecord.AgentName;
                        ShippingLineName = firstRecord.ShippingLineName;
                    }

                    TotalAmt = totalAmountLKR.ToString("N2");

                    PaidAmt = AmountPaidDec.ToString("N2");

                    BalanceAmt = AmountToBePaidDec.ToString("N2");

                    if (CreditorType == "Local")
                    {
                        CreditorName = ShippingLineName;
                    }
                    else// Supplier
                    {
                        CreditorName = AgentName;
                    }

                }
            }
            else if (jobType == "Export")
            {
                isValid = _operationcontext.TxnPaymentVoucherExportHds.Any(j => j.PayVoucherNo == jobNo);
                if (!isValid)
                {
                    message = "Invalid Export PaymentVoucher No";
                }
                else
                {
                    message = "Export PaymentVoucher Number validated SUCCESSFULLY";
                    decimal totalAmountLKR = 0;
                    decimal AmountPaidDec = 0;
                    decimal AmountToBePaidDec = 0;
                    var CreditorType = "";
                    var AgentName = "";
                    var ShippingLineName = "";

                    var txnPaymentVoucherExpHDs = _operationcontext.TxnPaymentVoucherExportHds.Where(j => j.PayVoucherNo == jobNo)
                       .ToList();

                    var joinedData = from t in txnPaymentVoucherExpHDs
                                     join a in _operationcontext.RefAgents on t.AgentID equals a.AgentId into agentGroup
                                     from a in agentGroup.DefaultIfEmpty()
                                     join s in _operationcontext.RefShippingLines on t.Customer equals s.ShippingLineId into shippingLineGroup
                                     from s in shippingLineGroup.DefaultIfEmpty()
                                     select new
                                     {
                                         t.TotalPayVoucherAmountLkr,
                                         t.AmountPaid,
                                         t.AmountToBePaid,
                                         t.Type,
                                         AgentName = a != null ? a.AgentName : null,
                                         ShippingLineName = s != null ? s.Name : null
                                     };
                    var firstRecord = joinedData.FirstOrDefault();

                    if (firstRecord != null)
                    {
                        totalAmountLKR = (decimal)firstRecord.TotalPayVoucherAmountLkr;
                        AmountPaidDec = (decimal)firstRecord.AmountPaid;
                        AmountToBePaidDec = (decimal)firstRecord.AmountToBePaid;
                        CreditorType = firstRecord.Type;
                        AgentName = firstRecord.AgentName;
                        ShippingLineName = firstRecord.ShippingLineName;
                    }

                    TotalAmt = totalAmountLKR.ToString("N2");

                    PaidAmt = AmountPaidDec.ToString("N2");

                    BalanceAmt = AmountToBePaidDec.ToString("N2");
                    if (CreditorType == "Local")
                    {
                        CreditorName = ShippingLineName;
                    }
                    else// Supplier
                    {
                        CreditorName = AgentName;
                    }

                }
            }
            else if (jobType == "Additional")
            {
                isValid = _context.TxnPurchasVoucherHDs.Any(j => j.PurchasVoucherNo == jobNo);
                if (!isValid)
                {
                    message = "Invalid Acc Purchas Voucher No.";
                }
                else
                {
                    message = " Acc Purchas Voucher No. validated SUCCESSFULLY";
                    var txnPurchasVoucherHDs = _context.TxnPurchasVoucherHDs.Where(j => j.PurchasVoucherNo == jobNo)
                        .Include(t => t.ShippingLineNavigation)
                        .Include(t => t.SupplierNavigation)
                        .ToList();
                    var totalAmountLKR = txnPurchasVoucherHDs.FirstOrDefault()?.TotalAmountLKR;
                    var AmountPaidDec = txnPurchasVoucherHDs.FirstOrDefault()?.AmountPaid;
                    var AmountToBePaidDec = txnPurchasVoucherHDs.FirstOrDefault()?.AmountToBePaid;
                    var CreditorType = txnPurchasVoucherHDs.FirstOrDefault()?.CreditorType;

                    if (totalAmountLKR.HasValue)
                    {
                        TotalAmt = totalAmountLKR.Value.ToString("N2");
                    }
                    if (AmountPaidDec.HasValue)
                    {
                        PaidAmt = AmountPaidDec.Value.ToString("N2");
                    }
                    if (AmountToBePaidDec.HasValue)
                    {
                        BalanceAmt = AmountToBePaidDec.Value.ToString("N2");
                    }
                    if (CreditorType == "ShippingLine")
                    {
                        CreditorName = txnPurchasVoucherHDs.FirstOrDefault().ShippingLineNavigation.Name;
                    }
                    else// Supplier
                    {
                        CreditorName = txnPurchasVoucherHDs.FirstOrDefault().SupplierNavigation.Name;
                    }

                }
            }
            return Json(new { isValid = isValid, message = message, totalAmt = TotalAmt, amountPaid = PaidAmt, amountToBePaid = BalanceAmt, creditorName = CreditorName });
        }

        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnPettyCashHDs = await _context.TxnPettyCashHDs.Include(t => t.SupplierPettyNavigation)
                .OrderByDescending(p => p.PettyCashNo).ToListAsync();


            if (!String.IsNullOrEmpty(searchString))
            {
                txnPettyCashHDs = txnPettyCashHDs.Where(t => t.PettyCashNo.Contains(searchString)).OrderByDescending(t => t.PettyCashNo).ToList();
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnPettyCashHDs.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnPettyCashHDs.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;
            return View(data);
        }


        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string Supplier)
        {


            var tables = new TxnPettyCashViewModel
            {
                TxnPettyCashHDMulti = _context.TxnPettyCashHDs.Where(t => t.PettyCashNo == "xxx"),
                TxnPettyCashDtlMulti = _context.TxnPettyCashDtls.Where(t => t.PettyCashNo == "xxx"),

            };


            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("PettyCashNo,Date,Supplier,DocType,VoucherNo,VoucherType,MainAcc,Narration,MainAccAmount,TotalAmountLKR,Remarks,AmountPaid,AmountToBePaid,TotAmtWord,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,CreditorName")] TxnPettyCashHD txnPettyCashHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_PettyCashHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "PTV" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnPettyCashHD.PettyCashNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }
            txnPettyCashHD.Canceled = false;
            txnPettyCashHD.CreatedBy = "Admin";
            txnPettyCashHD.CreatedDateTime = DateTime.Now;
            txnPettyCashHD.Approved = false;


            var TotalAmount = txnPettyCashHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnPettyCashHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            ModelState.Remove("PettyCashNo");  // Auto generated
            ModelState.Remove("BLNo");  // Not in use
            ModelState.Remove("CreatedBy");  // Assigned
            ModelState.Remove("CanceledReason");  // Not in use
            ModelState.Remove("CanceledBy");  // Not in use
            ModelState.Remove("ApprovedBy");  // Not in use
            ModelState.Remove("LastUpdatedBy");  // Not in use
            ModelState.Remove("TotAmtWord");  // Assigned
            if (ModelState.IsValid)
            {

                // Adding TxnPaymentDtl records

                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedPettyCashItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnPettyCashDtl
                            {
                                PettyCashNo = nextRefNo, // Set PaymentNo to nextRefNo
                                SerialNo = item.SerialNo,
                                LineAccNo = item.LineAccNo,
                                Description = item.Description,
                                Amount = (decimal)item.Amount,
                                CreatedDateTime = DateTime.Now,

                            };
                            _context.TxnPettyCashDtls.Add(detailItem);
                        }
                    }
                }
                try
                {

                    _context.Add(txnPettyCashHD);
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


            var tables = new TxnPettyCashViewModel
            {
                TxnPettyCashHDMulti = _context.TxnPettyCashHDs.Where(t => t.PettyCashNo == "xxx"),
                TxnPettyCashDtlMulti = _context.TxnPettyCashDtls.Where(t => t.PettyCashNo == "xxx"),

            };


            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

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

        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnPettyCashHD = await _context.TxnPettyCashHDs
                .FirstOrDefaultAsync(m => m.PettyCashNo == id);


            if (txnPettyCashHD == null)
            {
                return NotFound();
            }

            var tables = new TxnPettyCashViewModel
            {
                TxnPettyCashHDMulti = _context.TxnPettyCashHDs.Where(t => t.PettyCashNo == id),
                TxnPettyCashDtlMulti = _context.TxnPettyCashDtls.Where(t => t.PettyCashNo == id),
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

            var txnPettyCashHD = await _context.TxnPettyCashHDs
                .FirstOrDefaultAsync(m => m.PettyCashNo == id);

            if (txnPettyCashHD == null)
            {
                return NotFound();
            }

            // Inserting Transaction Data to Acccounts 
            var txnPettyCashDtl = await _context.TxnPettyCashDtls
                                .Where(m => m.PettyCashNo == id)
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

            if (txnPettyCashHD != null)
            {
                var SerialNo_AccTxn = 1;
                var AccTxnDescription = txnPettyCashHD.Narration;
                // First Acc transaction 
                TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
                NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
                NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
                NewRowAccTxnFirst.Date = txnPettyCashHD.Date; //  Jurnal Date 
                NewRowAccTxnFirst.Description = AccTxnDescription;
                NewRowAccTxnFirst.TxnAccCode = txnPettyCashHD.MainAcc;
                NewRowAccTxnFirst.Dr = (decimal)0;
                NewRowAccTxnFirst.Cr = (decimal)txnPettyCashHD.TotalAmountLKR;
                NewRowAccTxnFirst.RefNo = txnPettyCashHD.PettyCashNo; // Invoice No
                NewRowAccTxnFirst.Note = "";
                NewRowAccTxnFirst.Reconciled = false;
                NewRowAccTxnFirst.DocType = "PettyCash";
                NewRowAccTxnFirst.IsMonthEndDone = false;
                NewRowAccTxnFirst.CreatedBy = "Admin";
                NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
                NewRowAccTxnFirst.Canceled = false;

                NewRowAccTxnFirst.JobNo = txnPettyCashHD.VoucherNo;
                NewRowAccTxnFirst.JobType = txnPettyCashHD.DocType;

                _context.TxnTransactions.Add(NewRowAccTxnFirst);

                foreach (var item in txnPettyCashDtl)
                {
                    // Transaction table Insert 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();

                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnPettyCashHD.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = item.Description;
                    NewRowAccTxn.TxnAccCode = item.LineAccNo;
                    NewRowAccTxn.Dr = (decimal)item.Amount;
                    NewRowAccTxn.Cr = (decimal)0;
                    NewRowAccTxn.RefNo = txnPettyCashHD.PettyCashNo; //  No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "PettyCash";
                    NewRowAccTxn.IsMonthEndDone = false;
                    NewRowAccTxn.CreatedBy = "Admin";
                    NewRowAccTxn.CreatedDateTime = DateTime.Now;
                    NewRowAccTxn.Canceled = false;
                    NewRowAccTxn.JobNo = txnPettyCashHD.VoucherNo;
                    NewRowAccTxn.JobType = txnPettyCashHD.DocType;

                    _context.TxnTransactions.Add(NewRowAccTxn);
                }
            }
            // END Inserting Transaction Data to Acccounts 

            // ************
            // Update paid amount/ Balance Amount in PurchasVRelated
            var docType = txnPettyCashHD.DocType; //PurchasVRelated/ Other
            var voucherType = txnPettyCashHD.VoucherType; // Import / Export / Additional 
            var voucherNo = txnPettyCashHD.VoucherNo;
            if (docType == "PurchasVRelated")
            {
                if (voucherType == "Additional")
                {
                    var txnPurchasVoucherHDs = _context.TxnPurchasVoucherHDs.Where(t => t.PurchasVoucherNo == voucherNo);
                    var totalPaidAmt = txnPurchasVoucherHDs.FirstOrDefault().AmountPaid + txnPettyCashHD.TotalAmountLKR;
                    txnPurchasVoucherHDs.FirstOrDefault().AmountPaid = totalPaidAmt;
                    txnPurchasVoucherHDs.FirstOrDefault().AmountToBePaid = txnPurchasVoucherHDs.FirstOrDefault().TotalAmountLKR - totalPaidAmt;
                }
                else if (voucherType == "Import")
                {
                    var txnPaymentVoucherImportHds = _operationcontext.TxnPaymentVoucherImportHds.Where(t => t.PayVoucherNo == voucherNo);
                    var totalPaidAmt = txnPaymentVoucherImportHds.FirstOrDefault().AmountPaid + txnPettyCashHD.TotalAmountLKR;
                    txnPaymentVoucherImportHds.FirstOrDefault().AmountPaid = totalPaidAmt;
                    txnPaymentVoucherImportHds.FirstOrDefault().AmountToBePaid = txnPaymentVoucherImportHds.FirstOrDefault().TotalPayVoucherAmountLkr - totalPaidAmt;
                    _operationcontext.Update(txnPaymentVoucherImportHds.FirstOrDefault());
                    await _operationcontext.SaveChangesAsync();
                }
                else if (voucherType == "Export")
                {
                    var txnPaymentVoucherExportHds = _operationcontext.TxnPaymentVoucherExportHds.Where(t => t.PayVoucherNo == voucherNo);
                    var totalPaidAmt = txnPaymentVoucherExportHds.FirstOrDefault().AmountPaid + txnPettyCashHD.TotalAmountLKR;
                    txnPaymentVoucherExportHds.FirstOrDefault().AmountPaid = totalPaidAmt;
                    txnPaymentVoucherExportHds.FirstOrDefault().AmountToBePaid = txnPaymentVoucherExportHds.FirstOrDefault().TotalPayVoucherAmountLkr - totalPaidAmt;
                    _operationcontext.Update(txnPaymentVoucherExportHds.FirstOrDefault());
                    await _operationcontext.SaveChangesAsync();
                }
            }
            // ************

            /// Update the Approved property based on the form submission
            txnPettyCashHD.Approved = approved;
            txnPettyCashHD.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnPettyCashHD.ApprovedDateTime = DateTime.Now;

            _context.Update(txnPettyCashHD);
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }


        // GET: 
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

            var tables = new TxnPettyCashViewModel
            {
                TxnPettyCashHDMulti = _context.TxnPettyCashHDs.Where(t => t.PettyCashNo == id),
                TxnPettyCashDtlMulti = _context.TxnPettyCashDtls.Where(t => t.PettyCashNo == id),

            };


            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

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

        // GET: txnInvoiceExportHds/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var tables = new TxnPettyCashViewModel
            {
                TxnPettyCashHDMulti = _context.TxnPettyCashHDs.Where(t => t.PettyCashNo == id),
                TxnPettyCashDtlMulti = _context.TxnPettyCashDtls.Where(t => t.PettyCashNo == id),

            };


            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

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

        // POST: Credit sales/Edit/5 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string dtlItemsList, [Bind("PettyCashNo,Date,Supplier,DocType,VoucherNo,VoucherType,MainAcc,Narration,MainAccAmount,TotalAmountLKR,Remarks,AmountPaid,AmountToBePaid,TotAmtWord,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,CreditorName")] TxnPettyCashHD txnPettyCashHD)
        {
            if (id != txnPettyCashHD.PettyCashNo)
            {
                return NotFound();
            }

            txnPettyCashHD.LastUpdatedBy = "Admin";
            txnPettyCashHD.LastUpdatedDateTime = DateTime.Now;

            var TotalAmount = txnPettyCashHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnPettyCashHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            if (ModelState.IsValid)
            {

                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var rowsToDelete = _context.TxnPettyCashDtls.Where(t => t.PettyCashNo == id);
                    if (rowsToDelete != null && rowsToDelete.Any())
                    {
                        _context.TxnPettyCashDtls.RemoveRange(rowsToDelete);
                    }

                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedPettyCashItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnPettyCashDtl
                            {
                                PettyCashNo = id, // Set PaymentNo 
                                SerialNo = item.SerialNo,
                                LineAccNo = item.LineAccNo,
                                Description = item.Description,
                                Amount = (decimal)item.Amount,
                                CreatedDateTime = DateTime.Now,

                            };
                            _context.TxnPettyCashDtls.Add(detailItem);
                        }
                    }
                }
                try
                {

                    _context.Update(txnPettyCashHD);
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


            var tables = new TxnPettyCashViewModel
            {
                TxnPettyCashHDMulti = _context.TxnPettyCashHDs.Where(t => t.PettyCashNo == id),
                TxnPettyCashDtlMulti = _context.TxnPettyCashDtls.Where(t => t.PettyCashNo == id),

            };


            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

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


    }
}


