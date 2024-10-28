using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ACC_DEV.Data;
using ACC_DEV.DataOperation;
using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using ACC_DEV.ViewModel;
using ACC_DEV.CommonMethods;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ACC_DEV.Controllers
{
    public class TxnPaymentHDsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnPaymentHDsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
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

        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnPaymentHDs = await _context.TxnPaymentHDs
                .FirstOrDefaultAsync(m => m.PaymentNo == id);


            if (txnPaymentHDs == null)
            {
                return NotFound();
            }

            //var jobNo = txnPaymentHDs.JobNo; // Set the jobNo property

            var tables = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = _context.TxnPaymentHDs.Where(t => t.PaymentNo == id),
                TxnPaymentDtMulti = _context.TxnPaymentDtls.Where(t => t.PaymentNo == id),
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

            var txnPaymentHDs = await _context.TxnPaymentHDs
                .FirstOrDefaultAsync(m => m.PaymentNo == id);

            if (txnPaymentHDs == null)
            {
                return NotFound();
            }

            // Inserting Transaction Data to Acccounts 


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

            var txnPaymentDtls = await _context.TxnPaymentDtls
                    .Where(m => m.PaymentNo == id)
                    .ToListAsync();
            var SerialNo_AccTxn = 0;

            if (txnPaymentHDs != null)
            {
                SerialNo_AccTxn = 1;
                var AccTxnDescription = txnPaymentHDs.Narration ;
                // First Acc transaction 
                TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
                NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
                NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
                NewRowAccTxnFirst.Date = txnPaymentHDs.Date; //  Jurnal Date 
                NewRowAccTxnFirst.Description = AccTxnDescription;
                NewRowAccTxnFirst.TxnAccCode = txnPaymentHDs.CreditAcc;
                NewRowAccTxnFirst.Dr = (decimal)0;
                NewRowAccTxnFirst.Cr = (decimal)txnPaymentHDs.PaymentTotalAmt;  //  Credit "Local creditor "
                NewRowAccTxnFirst.RefNo = txnPaymentHDs.PaymentNo; // Invoice No
                NewRowAccTxnFirst.Note = "";
                NewRowAccTxnFirst.Reconciled = false;
                NewRowAccTxnFirst.DocType = "Payment";
                NewRowAccTxnFirst.IsMonthEndDone = false;
                NewRowAccTxnFirst.CreatedBy = "Admin";
                NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
                NewRowAccTxnFirst.Canceled = false;

                NewRowAccTxnFirst.JobNo = "";// txnDebitNoteAccHDs.JobNo;
                NewRowAccTxnFirst.JobType = "";// txnDebitNoteAccHDs.JobType;

                _context.TxnTransactions.Add(NewRowAccTxnFirst);

                // Other amount has a value
                if (txnPaymentHDs.OtherAmount>0) 
                {
                    SerialNo_AccTxn = SerialNo_AccTxn+1;
                    AccTxnDescription = txnPaymentHDs.Narration;
                    // Other Amount Acc transaction 
                    TxnTransactions NewRowAccTxnother = new TxnTransactions();
                    NewRowAccTxnother.TxnNo = nextAccTxnNo;
                    NewRowAccTxnother.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxnother.Date = txnPaymentHDs.Date; //  Jurnal Date 
                    NewRowAccTxnother.Description = AccTxnDescription+"OtherAmt: "+ txnPaymentHDs.OtherAmountDescr;
                    NewRowAccTxnother.TxnAccCode = txnPaymentHDs.DebitAcc;
                    NewRowAccTxnother.Dr = (decimal)txnPaymentHDs.OtherAmount;  //  Debit the bank account 
                    NewRowAccTxnother.Cr = (decimal)0;
                    NewRowAccTxnother.RefNo = txnPaymentHDs.PaymentNo; // Invoice No
                    NewRowAccTxnother.Note = "";
                    NewRowAccTxnother.Reconciled = false;
                    NewRowAccTxnother.DocType = "Payment";
                    NewRowAccTxnother.IsMonthEndDone = false;
                    NewRowAccTxnother.CreatedBy = "Admin";
                    NewRowAccTxnother.CreatedDateTime = DateTime.Now;
                    NewRowAccTxnother.Canceled = false;

                    NewRowAccTxnother.JobNo = "";// txnDebitNoteAccHDs.JobNo;
                    NewRowAccTxnother.JobType = "";// txnDebitNoteAccHDs.JobType;

                    _context.TxnTransactions.Add(NewRowAccTxnother);
                }

                // For details 
                foreach (var item in txnPaymentDtls)
                {
                    // Second row 
                    AccTxnDescription = txnPaymentHDs.Narration;
                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    // First Acc transaction 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnPaymentHDs.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = AccTxnDescription+" DocNo: "+item.DocNo;
                    NewRowAccTxn.TxnAccCode = txnPaymentHDs.DebitAcc;
                    NewRowAccTxn.Dr = (decimal)item.Amount;  //  Debit the bank account 
                    NewRowAccTxn.Cr = (decimal)0;
                    NewRowAccTxn.RefNo = txnPaymentHDs.PaymentNo; // Invoice No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "Payment";
                    NewRowAccTxn.IsMonthEndDone = false;
                    NewRowAccTxn.CreatedBy = "Admin";
                    NewRowAccTxn.CreatedDateTime = DateTime.Now;
                    NewRowAccTxn.Canceled = false;

                    NewRowAccTxn.JobNo = item.JobNo;// txnDebitNoteAccHDs.JobNo;
                    NewRowAccTxn.JobType = item.JobType;// txnDebitNoteAccHDs.JobType;

                    _context.TxnTransactions.Add(NewRowAccTxn);
                }
                // END Inserting Transaction Data to Acccounts 

                /// Update the Approved property based on the form submission
                txnPaymentHDs.Approved = approved;
                txnPaymentHDs.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
                txnPaymentHDs.ApprovedDateTime = DateTime.Now;

                // DocType Table initializing 
                // Accounts 2
                var txnPurchasVoucherHDs = await _context.TxnPurchasVoucherHDs
               .FirstOrDefaultAsync(m => m.PurchasVoucherNo == "xxx");
                var txnCreditNoteAccHDs = await _context.TxnCreditNoteAccHDs
                .FirstOrDefaultAsync(m => m.CreditNoteNo == "xxx");

                //Operation 4
                var txnPaymentVoucherExportHds = await _operationcontext.TxnPaymentVoucherExportHds
                .FirstOrDefaultAsync(m => m.PayVoucherNo == "xxx");
                var txnPaymentVoucherImportHds = await _operationcontext.TxnPaymentVoucherImportHds
                .FirstOrDefaultAsync(m => m.PayVoucherNo == "xxx");
                var txnCreditNoteExportHds = await _operationcontext.TxnCreditNoteExportHds
               .FirstOrDefaultAsync(m => m.CreditNoteNo == "xxx");
                var TxnCreditNoteImportHds = await _operationcontext.TxnCreditNoteImportHds
               .FirstOrDefaultAsync(m => m.CreditNoteNo == "xxx");

                // Update Amount paid 
                foreach (var item in txnPaymentDtls)
                {
                    var strDocType = item.DocType;
                    var strDocNo = item.DocNo;
                    var amount =item.Amount;

                    var totAmt = (decimal)0;
                    var PaidAmt = (decimal)0;
                    var totalPaidAmt = (decimal)0;
                    var AmtTobePaid = (decimal)0;

                    switch (strDocType)
                    {
                        case "PurchasVoucher":  //Accounts 1/2
                            txnPurchasVoucherHDs = await _context.TxnPurchasVoucherHDs.FirstOrDefaultAsync(t => t.PurchasVoucherNo == strDocNo);
                            totAmt = (decimal)txnPurchasVoucherHDs.TotalAmountLKR;
                            PaidAmt = (decimal)txnPurchasVoucherHDs.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnPurchasVoucherHDs.AmountPaid = totalPaidAmt;
                            txnPurchasVoucherHDs.AmountToBePaid = AmtTobePaid;
                            _context.Update(txnPurchasVoucherHDs);
                            break;
                        case "CreditNTAdd":  //Accounts 2/2
                            txnCreditNoteAccHDs = await _context.TxnCreditNoteAccHDs.FirstOrDefaultAsync(m => m.CreditNoteNo == strDocNo);
                            totAmt = (decimal)txnCreditNoteAccHDs.TotalAmountLKR;
                            PaidAmt = (decimal)txnCreditNoteAccHDs.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnCreditNoteAccHDs.AmountPaid = totalPaidAmt;
                            txnCreditNoteAccHDs.AmountToBePaid = AmtTobePaid;
                            _context.Update(txnCreditNoteAccHDs);
                            break;
                        case "PayVCExport":  //Operation 1/4
                            txnPaymentVoucherExportHds = await _operationcontext.TxnPaymentVoucherExportHds.FirstOrDefaultAsync(m => m.PayVoucherNo == strDocNo);
                            totAmt = (decimal)txnPaymentVoucherExportHds.TotalPayVoucherAmountLkr;
                            PaidAmt = (decimal)txnPaymentVoucherExportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnPaymentVoucherExportHds.AmountPaid = totalPaidAmt;
                            txnPaymentVoucherExportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(txnPaymentVoucherExportHds);
                            break;
                        case "PayVCImport":  // Operation 2/4
                            txnPaymentVoucherImportHds = await _operationcontext.TxnPaymentVoucherImportHds.FirstOrDefaultAsync(m => m.PayVoucherNo == strDocNo);
                            totAmt = (decimal)txnPaymentVoucherImportHds.TotalPayVoucherAmountLkr;
                            PaidAmt = (decimal)txnPaymentVoucherImportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnPaymentVoucherImportHds.AmountPaid = totalPaidAmt;
                            txnPaymentVoucherImportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(txnPaymentVoucherImportHds);
                            break;
                        case "CreditNTExport":  // Operation 3/4
                            txnCreditNoteExportHds = await _operationcontext.TxnCreditNoteExportHds.FirstOrDefaultAsync(m => m.CreditNoteNo == strDocNo);
                            totAmt = (decimal)txnCreditNoteExportHds.TotalCreditAmountLkr;
                            PaidAmt = (decimal)txnCreditNoteExportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnCreditNoteExportHds.AmountPaid = totalPaidAmt;
                            txnCreditNoteExportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(txnCreditNoteExportHds);

                            break;
                        case "CreditNTImport":  // Operation 4/4
                            TxnCreditNoteImportHds = await _operationcontext.TxnCreditNoteImportHds.FirstOrDefaultAsync(m => m.CreditNoteNo == strDocNo);
                            totAmt = (decimal)TxnCreditNoteImportHds.TotalCreditAmountLkr;
                            PaidAmt = (decimal)TxnCreditNoteImportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            TxnCreditNoteImportHds.AmountPaid = totalPaidAmt;
                            TxnCreditNoteImportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(TxnCreditNoteImportHds);
                            break;
                    }

                }

                 _context.Update(txnPaymentHDs);

                 await _context.SaveChangesAsync();
                await _operationcontext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnPaymentHDs = await _context.TxnPaymentHDs
                .Include(t => t.RefShippingLineNavigation)
                .Include(t => t.RefSupplierNavigation)
                .Include(t => t.RefAgentNavigation)
                .OrderByDescending(p => p.PaymentNo).ToListAsync();

            var viewModel = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = txnPaymentHDs,
                // Initialize other properties of the view model if needed
            };
            if (!String.IsNullOrEmpty(searchString))
            {
                viewModel.TxnPaymentHdMulti = viewModel.TxnPaymentHdMulti.Where(t => t.PaymentNo.Contains(searchString)).OrderByDescending(t => t.PaymentNo).ToList();
            }

            const int pageSize = 20;
            if (pg < 1)
                pg = 1;
            int recsCount = viewModel.TxnPaymentHdMulti.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = viewModel.TxnPaymentHdMulti.Skip(recSkip).Take(pager.PageSize).ToList();

            viewModel.TxnPaymentHdMulti = data;
            this.ViewBag.Pager = pager;

            return View(viewModel);
        }


        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier)
        {
            var PayCustomerID = "";
            var CustomerName = "";
            ViewData["CustomerType"] = "Agent";
            switch (customerType)
            {
                case "Agent":  // Agent
                    ViewData["CustomerType"] = "Agent";
                    PayCustomerID = POAgent;
                    ViewData["AgentID"] = POAgent;
                    var tblCustomerAg = _operationcontext.RefAgents.Where(t => t.AgentId == PayCustomerID);
                    if (tblCustomerAg != null)
                    {
                        CustomerName = tblCustomerAg.FirstOrDefault().AgentName;
                    }
                    break;

                case "ShippingLine": // Shipping Line
                    ViewData["CustomerType"] = "ShippingLine";
                    PayCustomerID = ShippingLine;
                    ViewData["ShippingLineID"] = ShippingLine;
                    var tblCustomer = _operationcontext.RefShippingLines.Where(t => t.ShippingLineId == PayCustomerID);
                    if (tblCustomer != null)
                    {
                        CustomerName = tblCustomer.FirstOrDefault().Name;
                    }
                    break;

                case "Supplier": // Supplier 
                    ViewData["CustomerType"] = "Supplier";
                    PayCustomerID = Supplier;
                    ViewData["SupplierID"] = Supplier;
                    var tblSupplier = _context.RefSuppliers.Where(t => t.SupplierId == PayCustomerID);
                    if (tblSupplier != null)
                    {
                        CustomerName = tblSupplier.FirstOrDefault().Name;
                    }
                    break;
            }


            ViewData["Customer"] = CustomerName;

            // Apply search functionality based on searchType
            var txnPayments = _context.TxnPaymentHDs.OrderByDescending(p => p.PaymentNo);

            var txnPurchasVoucherAccounts = _context.TxnPurchasVoucherHDs.Where(p => p.PurchasVoucherNo == "xxx");

            var txnPaymentVoucherAddtional = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == "xxx");
            var txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(t => t.PayVoucherNo == "xxx");
            var txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(t => t.PayVoucherNo == "xxx");
            // Credit notes/ Credit Note Additional 
            var txnCreditNoterExport = _operationcontext.TxnCreditNoteExportHds.Where(t => t.CreditNoteNo == "xxx");
            var txnCreditNoterImport = _operationcontext.TxnCreditNoteImportHds.Where(t => t.CreditNoteNo == "xxx");
            var txnCreditNoteAcc = _context.TxnCreditNoteAccHDs.Where(t => t.CreditNoteNo == "xxx");

            IEnumerable<SelectedPaymentVocher> payVoucherData = null;
            IEnumerable<SelectedPaymentVocher> payVoucherDataImport = null;
            IEnumerable<SelectedPaymentVocher> payVoucherDataExport = null;

            IEnumerable<SelectedPaymentVocher> payCreditDataExport = null;
            IEnumerable<SelectedPaymentVocher> payCreditDataImport = null;
            IEnumerable<SelectedPaymentVocher> payCreditDataAcc = null;


            //  Account(2) 1. Purches voucher
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnPurchasVoucherAccounts = _context.TxnPurchasVoucherHDs.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.ShippingLine == PayCustomerID).OrderByDescending(p => p.PurchasVoucherNo);
            }
            else if (customerType == "Supplier")
            {
                txnPurchasVoucherAccounts = _context.TxnPurchasVoucherHDs.Where(p => p.AmountToBePaid != 0 && p.Approved == true  && p.Supplier == PayCustomerID).OrderByDescending(p => p.PurchasVoucherNo);
            }


            payVoucherData = txnPurchasVoucherAccounts.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.PurchasVoucherNo,
                DocType = "PurchasVoucher",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType
            });

            // Account(2) 2.Credit Note Additional 
            if (customerType == "Agent")  // Agent
            {
                txnCreditNoteAcc = _context.TxnCreditNoteAccHDs.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Agent == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }

            payCreditDataAcc = txnCreditNoteAcc.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.CreditNoteNo,
                DocType = "CreditNTAdd",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType
            });

            // Concatenate payCreditDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payCreditDataAcc);

            // Operation(4) 1 Payment Voucher Import 
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true  && p.Customer == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true  && p.AgentID == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }

            payVoucherDataImport = txnPaymentVoucherimport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.PayVoucherNo,
                DocType = "PayVCImport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalPayVoucherAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });

            // Concatenate payVoucherDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payVoucherDataImport);

            // Operation(4) 2.Payment Voucher Export 
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Customer == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true  && p.AgentID == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }

            payVoucherDataExport = txnPaymentVoucherExport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.PayVoucherNo,
                DocType = "PayVCExport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalPayVoucherAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate payVoucherDataExport into payVoucherData
            payVoucherData = payVoucherData.Concat(payVoucherDataExport);

            // Operation(4) 3.Credit Note Export Normal and TS 
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnCreditNoterExport = _operationcontext.TxnCreditNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Customer == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnCreditNoterExport = _operationcontext.TxnCreditNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.AgentID == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }

            payCreditDataExport = txnCreditNoterExport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.CreditNoteNo,
                DocType = "CreditNTExport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalCreditAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate payCreditDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payCreditDataExport);

            // Operation(4) 4.Credit Note Import
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnCreditNoterImport = _operationcontext.TxnCreditNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true  && p.Customer == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnCreditNoterImport = _operationcontext.TxnCreditNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true  && p.AgentID == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }
            
            payCreditDataImport = txnCreditNoterImport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.CreditNoteNo,
                DocType = "CreditNTImport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalCreditAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });

            // Concatenate payCreditDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payCreditDataImport);


            ViewData["PaymentVoucherType"] = "Select payment Voucher Type";

            var tables = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = _context.TxnPaymentHDs.Where(t => t.PaymentNo == "xxx"),
                TxnPaymentDtMulti = _context.TxnPaymentDtls.Where(t => t.PaymentNo == "xxx"),
                PayVoucherMulti = payVoucherData
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, string customerType, string paymentVouchType, [Bind("PaymentNo,Date,PaymentMethod,ExchangeRate,CustomerID,ChequeNo,ChequeDate,ChequeBankID,ChequeAmount,Remarks,PaymentTotalAmt,DebitAcc,CreditAcc,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason, IsAcPayeeOnly, AgentID, ShippingLineID,OtherAmountDescr,OtherAmount,Narration")] TxnPaymentHDs txnPaymentHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_PaymentHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "PMT" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnPaymentHD.PaymentNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }
            txnPaymentHD.Approved = false;
            txnPaymentHD.Canceled = false;
            txnPaymentHD.CreatedBy = "Admin";
            txnPaymentHD.CreatedDateTime = DateTime.Now;
            txnPaymentHD.CustomerType = customerType;



            var TotalAmount = txnPaymentHD.PaymentTotalAmt;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnPaymentHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            ModelState.Remove("PaymentNo");  // Auto generated
            if (ModelState.IsValid)
            {
                // Adding TxnPaymentDtl records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedPaymentItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnPaymentDtl
                            {
                                PaymentNo = nextRefNo, // Set PaymentNo to nextRefNo
                                DocNo = item.RefNo,
                                DocType = item.RefType,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,
                                JobNo = item.JobNo,
                                JobType = item.JobType,
                            };
                            _context.TxnPaymentDtls.Add(detailItem);

                        }
                    }
                }
                try
                {
                    _context.Add(txnPaymentHD);
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


            var tables = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = _context.TxnPaymentHDs.Where(t => t.PaymentNo == "xxx"),
                TxnPaymentDtMulti = _context.TxnPaymentDtls.Where(t => t.PaymentNo == "xxx"),
                //PayVoucherHdMulti = _context.TxnPaymentVoucherHds.ToList(),
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

            return View(tables);
        }

        public async Task<IActionResult> Details(string id)
        {
            IEnumerable<SelectedPaymentVocher> payVoucherData = null;

            var tables = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = _context.TxnPaymentHDs.Where(t => t.PaymentNo == id),
                TxnPaymentDtMulti = _context.TxnPaymentDtls.Where(t => t.PaymentNo == id),
                PayVoucherMulti = payVoucherData
            };
            var PayCustomerID = "";
            var CustomerName = "";
            var customerType = tables.TxnPaymentHdMulti.FirstOrDefault().CustomerType;
           

            switch (customerType)
            {
                case "Agent":  // Agent
                    ViewData["CustomerType"] = "Agent";
                    PayCustomerID = tables.TxnPaymentHdMulti.FirstOrDefault().AgentID;
                    ViewData["AgentID"] = PayCustomerID;
                    var tblCustomerAg = _operationcontext.RefAgents.Where(t => t.AgentId == PayCustomerID);
                    if (tblCustomerAg != null)
                    {
                        CustomerName = tblCustomerAg.FirstOrDefault().AgentName;
                    }
                    break;

                case "ShippingLine": // Shipping Line
                    ViewData["CustomerType"] = "ShippingLine";
                    PayCustomerID = tables.TxnPaymentHdMulti.FirstOrDefault().ShippingLineID;
                    ViewData["ShippingLineID"] = PayCustomerID;
                    var tblCustomer = _operationcontext.RefShippingLines.Where(t => t.ShippingLineId == PayCustomerID);
                    if (tblCustomer != null)
                    {
                        CustomerName = tblCustomer.FirstOrDefault().Name;
                    }
                    break;

                case "Supplier": // Supplier 
                    ViewData["CustomerType"] = "Supplier";
                    PayCustomerID = tables.TxnPaymentHdMulti.FirstOrDefault().CustomerID;
                    ViewData["SupplierID"] = PayCustomerID;
                    var tblSupplier = _context.RefSuppliers.Where(t => t.SupplierId == PayCustomerID);
                    if (tblSupplier != null)
                    {
                        CustomerName = tblSupplier.FirstOrDefault().Name;
                    }
                    break;
            }
            ViewData["Customer"] = CustomerName;
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
            [HttpGet]
        public JsonResult CheckChequeNumber(string chequeNo)
        {
            bool isChequeNumberExists = _context.TxnPaymentHDs.Any(x => x.ChequeNo == chequeNo);
            return Json(isChequeNumberExists);
        }

        // GET: Receipt/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var PaymentNo = "";

            if (string.IsNullOrWhiteSpace(id))
            {
                return View("Error");
            }
            else
            {
                PaymentNo = id;
            }

            // Account(2) 1. Purches voucher 2.Credit Note Additional 
            var txnPurchasVoucherAccounts = _context.TxnPurchasVoucherHDs.Where(p => p.PurchasVoucherNo == "xxx");
            var txnCreditNoteAcc = _context.TxnCreditNoteAccHDs.Where(t => t.CreditNoteNo == "xxx");

            // Operation(4) 1 Payment Voucher Import 2.  Payment Voucher Export 3.Credit Note Export Normal and TS 4 3.Credit Note Import
            var txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(t => t.PayVoucherNo == "xxx");
            var txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(t => t.PayVoucherNo == "xxx");
            var txnCreditNoterExport = _operationcontext.TxnCreditNoteExportHds.Where(t => t.CreditNoteNo == "xxx");
            var txnCreditNoterImport = _operationcontext.TxnCreditNoteImportHds.Where(t => t.CreditNoteNo == "xxx");

            IEnumerable<SelectedPaymentVocher> payVoucherData = null;
            IEnumerable<SelectedPaymentVocher> payVoucherDataImport = null;
            IEnumerable<SelectedPaymentVocher> payVoucherDataExport = null;

            IEnumerable<SelectedPaymentVocher> payCreditDataExport = null;
            IEnumerable<SelectedPaymentVocher> payCreditDataImport = null;
            IEnumerable<SelectedPaymentVocher> payCreditDataAcc = null;

            IEnumerable<SelectedPaymentDtlTableForEdit> PaymentDtlTableData = null;

            var tables = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = _context.TxnPaymentHDs.Where(t => t.PaymentNo == PaymentNo),
                TxnPaymentDtMulti = _context.TxnPaymentDtls.Where(t => t.PaymentNo == PaymentNo),
                PayVoucherMulti = payVoucherData
            };

            var PayCustomerID = tables.TxnPaymentHdMulti.FirstOrDefault().CustomerID;
            var CustomerName = "";
            var customerType = tables.TxnPaymentHdMulti.FirstOrDefault().CustomerType;
            ViewData["CustomerType"] = "";

            switch (customerType)
            {
                case "Agent":  // Agent
                    ViewData["CustomerType"] = "Agent";
                    PayCustomerID = tables.TxnPaymentHdMulti.FirstOrDefault().AgentID;
                    ViewData["AgentID"] = PayCustomerID;
                    var tblCustomerAg = _operationcontext.RefAgents.Where(t => t.AgentId == PayCustomerID);
                    if (tblCustomerAg != null)
                    {
                        CustomerName = tblCustomerAg.FirstOrDefault().AgentName;
                    }
                    break;

                case "ShippingLine": // Shipping Line
                    ViewData["CustomerType"] = "ShippingLine";
                    PayCustomerID = tables.TxnPaymentHdMulti.FirstOrDefault().ShippingLineID;
                    ViewData["ShippingLineID"] = PayCustomerID;
                    var tblCustomer = _operationcontext.RefShippingLines.Where(t => t.ShippingLineId == PayCustomerID);
                    if (tblCustomer != null)
                    {
                        CustomerName = tblCustomer.FirstOrDefault().Name;
                    }
                    break;

                case "Supplier": // Supplier 
                    ViewData["CustomerType"] = "Supplier";
                    PayCustomerID = tables.TxnPaymentHdMulti.FirstOrDefault().CustomerID;
                    ViewData["SupplierID"] = PayCustomerID;
                    var tblSupplier = _context.RefSuppliers.Where(t => t.SupplierId == PayCustomerID);
                    if (tblSupplier != null)
                    {
                        CustomerName = tblSupplier.FirstOrDefault().Name;
                    }
                    break;
            }
            ViewData["Customer"] = CustomerName;

            // Account(2) 1. Purches voucher 2.Credit Note Additional 
            // Account(2) 1. Purches voucher
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnPurchasVoucherAccounts = _context.TxnPurchasVoucherHDs.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.ShippingLine == PayCustomerID).OrderByDescending(p => p.PurchasVoucherNo);
            }
            else if (customerType == "Supplier")
            {
                txnPurchasVoucherAccounts = _context.TxnPurchasVoucherHDs.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Supplier == PayCustomerID).OrderByDescending(p => p.PurchasVoucherNo);
            }

            payVoucherData = txnPurchasVoucherAccounts.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.PurchasVoucherNo,
                DocType = "PurchasVoucher",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType
            });

            // Account(2) 2.Credit Note Additional 
            if (customerType == "Agent")  // Agent
            {
                txnCreditNoteAcc = _context.TxnCreditNoteAccHDs.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Agent == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }

            payCreditDataAcc = txnCreditNoteAcc.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.CreditNoteNo,
                DocType = "CreditNTAdd",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType
            });

            // Concatenate payCreditDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payCreditDataAcc);

            // Operation(4) 1 Payment Voucher Import 
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Customer == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.AgentID == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }

            payVoucherDataImport = txnPaymentVoucherimport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.PayVoucherNo,
                DocType = "PayVCImport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalPayVoucherAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });
            // Concatenate payVoucherDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payVoucherDataImport);

            // Operation(4) 2.Payment Voucher Export 
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Customer == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.AgentID == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);
            }

            payVoucherDataExport = txnPaymentVoucherExport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.PayVoucherNo,
                DocType = "PayVCExport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalPayVoucherAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate payVoucherDataExport into payVoucherData
            payVoucherData = payVoucherData.Concat(payVoucherDataExport);

            // Operation(4) 3.Credit Note Export Normal and TS 
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnCreditNoterExport = _operationcontext.TxnCreditNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Customer == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnCreditNoterExport = _operationcontext.TxnCreditNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.AgentID == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }

            payCreditDataExport = txnCreditNoterExport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.CreditNoteNo,
                DocType = "CreditNTExport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalCreditAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate payCreditDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payCreditDataExport);
            // Operation(4) 4.Credit Note Import
            if (customerType == "ShippingLine")// Shipping Line
            {
                txnCreditNoterImport = _operationcontext.TxnCreditNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.Customer == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnCreditNoterImport = _operationcontext.TxnCreditNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.Approved == true && p.AgentID == PayCustomerID).OrderByDescending(p => p.CreditNoteNo);
            }

            payCreditDataImport = txnCreditNoterImport.Select(voucher => new SelectedPaymentVocher
            {
                PayVoucherNo = voucher.CreditNoteNo,
                DocType = "CreditNTImport",
                Date = voucher.Date,
                TotalPayVoucherAmountLkr = voucher.TotalCreditAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });

            // Concatenate payCreditDataImport into payVoucherData
            payVoucherData = payVoucherData.Concat(payCreditDataImport);

            // End of filling data

            // ****** Removing docs in PaymentDtl from  paymentDocData
            var paymentDtlDocNos = tables.TxnPaymentDtMulti
                                    .Where(t => t.PaymentNo == PaymentNo)
                                    .Select(t => t.DocNo)
                                    .ToList();
            // Filter receiptDocData to exclude items with matching DocNo in receiptDtlDocNos
            payVoucherData = payVoucherData
                            .Where(doc => !paymentDtlDocNos.Contains(doc.PayVoucherNo))
                            .ToList();

            tables.PayVoucherMulti = payVoucherData;

            // Create paymentDtlTableData for EDIT
            PaymentDtlTableData = tables.TxnPaymentDtMulti.Select(voucher => new SelectedPaymentDtlTableForEdit
            {
                DocNo = voucher.DocNo,
                DocType = voucher.DocType,
                Date = DateTime.Now,
                TotalAmountLkr = (decimal)0.00,
                AmountPaid = (decimal)0.00,
                AmountToBePaid = (decimal)0.00,
                Amount = voucher.Amount,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType,
            });
            tables.PaymentDtlTableMulti = PaymentDtlTableData;

            //******** Generate ViewData
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
        public async Task<IActionResult> Edit(string id, string customerType, string dtlItemsList, [Bind("PaymentNo,Date,PaymentMethod,ExchangeRate,CustomerID,ChequeNo,ChequeDate,ChequeBankID,ChequeAmount,Remarks,PaymentTotalAmt,DebitAcc,CreditAcc,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,IsAcPayeeOnly, AgentID, ShippingLineID,OtherAmountDescr,OtherAmount,Narration")] TxnPaymentHDs txnPaymentHD)
        {
            if (id != txnPaymentHD.PaymentNo)
            {
                return NotFound();
            }

            var TotalAmount = txnPaymentHD.PaymentTotalAmt;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnPaymentHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);
            txnPaymentHD.LastUpdatedBy = "Admin";
            txnPaymentHD.LastUpdatedDateTime = DateTime.Now;
            txnPaymentHD.CustomerType = customerType;

            if (txnPaymentHD.PaymentMethod== "Cash")
            {
                txnPaymentHD.ChequeAmount = 0;
                txnPaymentHD.ChequeNo = "";
            }

            if (ModelState.IsValid)
            {
                // Adding TxnPaymentDtl records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    // Remove exisiting dtl rows 
                    var rowsToDelete = _context.TxnPaymentDtls.Where(t => t.PaymentNo == id);
                    if (rowsToDelete != null && rowsToDelete.Any())
                    {
                        _context.TxnPaymentDtls.RemoveRange(rowsToDelete);
                    }

                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedPaymentItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnPaymentDtl
                            {
                                PaymentNo = id, // Set PaymentNo to nextRefNo
                                DocNo = item.RefNo,
                                DocType = item.RefType,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,
                                JobNo = item.JobNo,
                                JobType = item.JobType,
                            };
                            _context.TxnPaymentDtls.Add(detailItem);

                        }
                    }
                }
                try
                {
                    _context.Update(txnPaymentHD);
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
        

            /// ***********

                var tables = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = _context.TxnPaymentHDs.Where(t => t.PaymentNo == id),
                TxnPaymentDtMulti = _context.TxnPaymentDtls.Where(t => t.PaymentNo == id),
                //PayVoucherMulti = payVoucherData
            };

            return View(tables);
        }


    }

}

   

   


