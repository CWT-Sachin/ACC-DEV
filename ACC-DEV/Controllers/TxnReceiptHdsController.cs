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
using System.Data;
using Microsoft.Extensions.Logging;

namespace ACC_DEV.Controllers
{
    public class TxnReceiptHDsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnReceiptHDsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }
        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnReceiptHDs = await _context.TxnReceiptHDs
                .Include(t => t.RefAgentNavigation)
                .Include(t => t.RefCustomerNavigation)
                .OrderByDescending(p => p.ReceiptNo).ToListAsync();

            var viewModel = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = txnReceiptHDs,
                // Initialize other properties of the view model if needed
            };

            if (!String.IsNullOrEmpty(searchString))
            {
                viewModel.TxnReceiptHdMulti = viewModel.TxnReceiptHdMulti.Where(t => t.ReceiptNo.Contains(searchString)).OrderByDescending(t => t.ReceiptNo).ToList();
            }


            const int pageSize = 20;
            if (pg < 1)
                pg = 1;
            int recsCount = viewModel.TxnReceiptHdMulti.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = viewModel.TxnReceiptHdMulti.Skip(recSkip).Take(pager.PageSize).ToList();

            viewModel.TxnReceiptHdMulti = data;
            this.ViewBag.Pager = pager;

            return View(viewModel);
        }

        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier, string CustomerID)
        {
            var PayCustomerID = "";
            var CustomerName = "";
            ViewData["CustomerType"] = "";
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

                case "Customer": // Customer 
                    ViewData["CustomerType"] = "Customer";
                    PayCustomerID = CustomerID;
                    ViewData["CustomerID"] = CustomerID;
                    var tblSupplier = _context.RefCustomerAccOpt.Where(t => t.CustomerId == PayCustomerID);
                    if (tblSupplier != null)
                    {
                        CustomerName = tblSupplier.FirstOrDefault().Name;
                    }
                    break;
            }


            ViewData["Customer"] = CustomerName;

            // Apply search functionality based on searchType

            // Account 1 Debite Note 2 Credit Sales/Invoice
            var txnDebitNoteAccHDs = _context.TxnDebitNoteAccHDs.Where(p => p.DebitNoteNo == "xxx");
            var txnCreditSalesHDs = _context.TxnCreditSalesHDs.Where(p => p.CreditSalesNo == "xxx");

            // Operation 1.Debit Note Import 2.Debit Note Export 3.Invoice Import 4.Invoice Export
            var txnDebitNoteImportHds = _operationcontext.TxnDebitNoteImportHds.Where(t => t.DebitNoteNo == "xxx");
            var txnDebitNoteExportHds = _operationcontext.TxnDebitNoteExportHds.Where(t => t.DebitNoteNo == "xxx");

            var txnInvoiceImportHds = _operationcontext.TxnInvoiceImportHds.Where(t => t.InvoiceNo == "xxx");
            var txnInvoiceExportHds = _operationcontext.TxnInvoiceExportHds.Where(t => t.InvoiceNo == "xxx");


            IEnumerable<SelectedReceiptDoc> receiptDocData = null;// payVoucherData = null;
            IEnumerable<SelectedReceiptDoc> receiptCreditSalesAcc = null;

            IEnumerable<SelectedReceiptDoc> receiptDebitNoteExport = null;
            IEnumerable<SelectedReceiptDoc> receiptDebitNoteImport = null;
            IEnumerable<SelectedReceiptDoc> receiptInvoiceImport = null;
            IEnumerable<SelectedReceiptDoc> receiptInvoiceExport = null;

            




            // Account 1 Debite Note 
            txnDebitNoteAccHDs = _context.TxnDebitNoteAccHDs.Where(p => p.AmountToBePaid != 0 && p.Agent == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);


            receiptDocData = txnDebitNoteAccHDs.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.DebitNoteNo,
                DocType = "DebitNTAdd",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType=voucher.JobType
            });

            // 2. Accounts- Credit Sales/Invoice
            txnCreditSalesHDs = _context.TxnCreditSalesHDs.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.CreditSalesNo);

            receiptCreditSalesAcc = txnCreditSalesHDs.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.CreditSalesNo,
                DocType = "CreditSales",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType
            });
            // Concatenate receiptCreditSalesAcc into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptCreditSalesAcc);

            // Operation 1.Debit Note Import
            if (customerType == "Customer")// Customer
            {
                txnDebitNoteImportHds = _operationcontext.TxnDebitNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnDebitNoteImportHds = _operationcontext.TxnDebitNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.AgentID == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }

            receiptDebitNoteImport = txnDebitNoteImportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.DebitNoteNo,
                DocType = "DebitNTImport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalDebitAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });

            // Concatenate receiptDebitNoteImport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptDebitNoteImport);


            // Operation 2.Debit Note Export
            if (customerType == "Customer")// Shipping Line
            {
                txnDebitNoteExportHds = _operationcontext.TxnDebitNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnDebitNoteExportHds = _operationcontext.TxnDebitNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.AgentID == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }

            receiptDebitNoteExport = txnDebitNoteExportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.DebitNoteNo,
                DocType = "DebitNTExport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalDebitAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate receiptDebitNoteExport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptDebitNoteExport);


            // Operation 3.Invoice import

            txnInvoiceImportHds = _operationcontext.TxnInvoiceImportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.InvoiceNo);


            receiptInvoiceImport = txnInvoiceImportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.InvoiceNo,
                DocType = "InvoiceImport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalInvoiceAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });

            // Concatenate receiptInvoiceImport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptInvoiceImport);


            // Operation 4.Invoice Export

            txnInvoiceExportHds = _operationcontext.TxnInvoiceExportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.InvoiceNo);


            receiptInvoiceExport = txnInvoiceExportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.InvoiceNo,
                DocType = "InvoiceExport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalInvoiceAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate receiptInvoiceImport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptInvoiceExport);

            ViewData["PaymentVoucherType"] = "Select payment Voucher Type";

            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == "xxx"),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == "xxx"),
                ReceiptDocMulti = receiptDocData
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
        public async Task<IActionResult> Create(string dtlItemsList, string customerType, string paymentVouchType, [Bind("ReceiptNo,Date,PaymentMethod,ExchangeRate,CustomerID,AgentID,ChequeNo,ChequeDate,ChequeBankID,ChequeAmount,Remarks,ReceiptTotalAmt,DebitAcc,CreditAcc,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,OtherAmountDescr,OtherAmount,Narration")] TxnReceiptHD txnReceiptHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_ReceiptHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "RCP" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnReceiptHD.ReceiptNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }
            txnReceiptHD.Approved = false;
            txnReceiptHD.Canceled = false;
            txnReceiptHD.CreatedBy = "Admin";
            txnReceiptHD.CreatedDateTime = DateTime.Now;
            txnReceiptHD.CustomerType = customerType;



            var TotalAmount = txnReceiptHD.ReceiptTotalAmt;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnReceiptHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            ModelState.Remove("ReceiptNo");  // Auto generated
            if (ModelState.IsValid)
            {
                // Adding TxnReceiptDtl records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedRecieptPaymentItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnReceiptDtl
                            {
                                ReceiptNo = nextRefNo, // Set PaymentNo to nextRefNo
                                DocNo = item.DocNo,
                                DocType = item.DocType,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,
                                JobNo=item.JobNo, 
                                JobType = item.JobType,
                            };
                            _context.TxnReceiptDtls.Add(detailItem);

                        }
                    }
                }
                try
                {
                    _context.Add(txnReceiptHD);
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


            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == "xxx"),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == "xxx"),
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

        public async Task<IActionResult> RepPrintReceipt(string ReceiptNo)
        {
            if (ReceiptNo == null || _context.TxnReceiptHDs == null)
            {
                return NotFound();
            }

            var txnReceiptHd = await _context.TxnReceiptHDs
                .FirstOrDefaultAsync(m => m.ReceiptNo == ReceiptNo);

            if (txnReceiptHd == null)
            {
                return NotFound();
            }





            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == ReceiptNo),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == ReceiptNo),

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

        public async Task<IActionResult> RepPrintChequePaymentVoucher(string ReceiptNo)
        {
            if (ReceiptNo == null || _context.TxnReceiptHDs == null)
            {
                return NotFound();
            }

            var txnReceiptHd = await _context.TxnReceiptHDs
                .FirstOrDefaultAsync(m => m.ReceiptNo == ReceiptNo);

            if (txnReceiptHd == null)
            {
                return NotFound();
            }





            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == ReceiptNo),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == ReceiptNo),

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

        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnReceiptHDs = await _context.TxnReceiptHDs
                .FirstOrDefaultAsync(m => m.ReceiptNo == id);


            if (txnReceiptHDs == null)
            {
                return NotFound();
            }

            //var jobNo = txnPaymentHDs.JobNo; // Set the jobNo property

            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == id),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == id),
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

            var txnReceiptHDs = await _context.TxnReceiptHDs
                .FirstOrDefaultAsync(m => m.ReceiptNo == id);

            if (txnReceiptHDs == null)
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

            var txnReceiptDtls = await _context.TxnReceiptDtls
                    .Where(m => m.ReceiptNo == id)
                    .ToListAsync();

            if (txnReceiptHDs != null)
            {
                var SerialNo_AccTxn = 1;
                var AccTxnDescription = txnReceiptHDs.Narration;
                // First Acc transaction 
                TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
                NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
                NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
                NewRowAccTxnFirst.Date = txnReceiptHDs.Date; //  Jurnal Date 
                NewRowAccTxnFirst.Description = AccTxnDescription;
                NewRowAccTxnFirst.TxnAccCode = txnReceiptHDs.DebitAcc;
                NewRowAccTxnFirst.Dr =  (decimal)txnReceiptHDs.ReceiptTotalAmt;  //  Debit Local creditor 
                NewRowAccTxnFirst.Cr = (decimal)0;
                NewRowAccTxnFirst.RefNo = txnReceiptHDs.ReceiptNo; // Invoice No
                NewRowAccTxnFirst.Note = "";
                NewRowAccTxnFirst.Reconciled = false;
                NewRowAccTxnFirst.DocType = "Receipt";
                NewRowAccTxnFirst.IsMonthEndDone = false;
                NewRowAccTxnFirst.CreatedBy = "Admin";
                NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
                NewRowAccTxnFirst.Canceled = false;

                NewRowAccTxnFirst.JobNo = "";// txnDebitNoteAccHDs.JobNo;
                NewRowAccTxnFirst.JobType = "";// txnDebitNoteAccHDs.JobType;

                _context.TxnTransactions.Add(NewRowAccTxnFirst);

                // Other amount has a value
                if (txnReceiptHDs.OtherAmount > 0)
                {
                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    AccTxnDescription = txnReceiptHDs.Narration;
                    // Other Amount Acc transaction 
                    TxnTransactions NewRowAccTxnother = new TxnTransactions();
                    NewRowAccTxnother.TxnNo = nextAccTxnNo;
                    NewRowAccTxnother.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxnother.Date = txnReceiptHDs.Date; //  Jurnal Date 
                    NewRowAccTxnother.Description = AccTxnDescription + "OtherAmt: " + txnReceiptHDs.OtherAmountDescr;
                    NewRowAccTxnother.TxnAccCode = txnReceiptHDs.CreditAcc;
                    NewRowAccTxnother.Dr = (decimal)0;
                    NewRowAccTxnother.Cr = (decimal)txnReceiptHDs.OtherAmount;  //  Credit the bank account 
                    NewRowAccTxnother.RefNo = txnReceiptHDs.ReceiptNo; // Invoice No
                    NewRowAccTxnother.Note = "";
                    NewRowAccTxnother.Reconciled = false;
                    NewRowAccTxnother.DocType = "Receipt";
                    NewRowAccTxnother.IsMonthEndDone = false;
                    NewRowAccTxnother.CreatedBy = "Admin";
                    NewRowAccTxnother.CreatedDateTime = DateTime.Now;
                    NewRowAccTxnother.Canceled = false;

                    NewRowAccTxnother.JobNo = "";// txnDebitNoteAccHDs.JobNo;
                    NewRowAccTxnother.JobType = "";// txnDebitNoteAccHDs.JobType;

                    _context.TxnTransactions.Add(NewRowAccTxnother);
                }


                foreach (var item in txnReceiptDtls)
                {
                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    AccTxnDescription = txnReceiptHDs.Narration;
                    // First Acc transaction 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnReceiptHDs.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = AccTxnDescription + " DocNo: " + item.DocNo;
                    NewRowAccTxn.TxnAccCode = txnReceiptHDs.CreditAcc;
                    NewRowAccTxn.Dr = (decimal)0;
                    NewRowAccTxn.Cr = (decimal)item.Amount;  //  Credit the bank account 
                    NewRowAccTxn.RefNo = txnReceiptHDs.ReceiptNo; // Invoice No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "Receipt";
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
                txnReceiptHDs.Approved = approved;
                txnReceiptHDs.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
                txnReceiptHDs.ApprovedDateTime = DateTime.Now;

                // DocType Table initializing 
                // Accounts 2
                var txnDebitNoteAccHDs = await _context.TxnDebitNoteAccHDs
               .FirstOrDefaultAsync(m => m.DebitNoteNo == "xxx");
                var txnCreditSalesHDs = await _context.TxnCreditSalesHDs
                .FirstOrDefaultAsync(m => m.CreditSalesNo == "xxx");

                //Operation 4
                var txnDebitNoteImportHds = await _operationcontext.TxnDebitNoteImportHds
                .FirstOrDefaultAsync(m => m.DebitNoteNo == "xxx");
                var txnDebitNoteExportHds = await _operationcontext.TxnDebitNoteExportHds
                .FirstOrDefaultAsync(m => m.DebitNoteNo == "xxx");

                var txnInvoiceImportHds = await _operationcontext.TxnInvoiceImportHds
               .FirstOrDefaultAsync(m => m.InvoiceNo == "xxx");
                var txnInvoiceExportHds = await _operationcontext.TxnInvoiceExportHds
               .FirstOrDefaultAsync(m => m.InvoiceNo == "xxx");

                // Update Amount paid 
                foreach (var item in txnReceiptDtls)
                {
                    var strDocType = item.DocType;
                    var strDocNo = item.DocNo;
                    var amount = item.Amount;

                    var totAmt = (decimal)0;
                    var PaidAmt = (decimal)0;
                    var totalPaidAmt = (decimal)0;
                    var AmtTobePaid = (decimal)0;

                    switch (strDocType)
                    {
                        case "DebitNTAdd":  //Accounts 1/2
                            txnDebitNoteAccHDs = await _context.TxnDebitNoteAccHDs.FirstOrDefaultAsync(t => t.DebitNoteNo == strDocNo);
                            totAmt = (decimal)txnDebitNoteAccHDs.TotalAmountLKR;
                            PaidAmt = (decimal)txnDebitNoteAccHDs.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnDebitNoteAccHDs.AmountPaid = totalPaidAmt;
                            txnDebitNoteAccHDs.AmountToBePaid = AmtTobePaid;
                            _context.Update(txnDebitNoteAccHDs);
                            break;
                        case "CreditSales":  //Accounts 2/2
                            txnCreditSalesHDs = await _context.TxnCreditSalesHDs.FirstOrDefaultAsync(m => m.CreditSalesNo == strDocNo);
                            totAmt = (decimal)txnCreditSalesHDs.TotalAmountLKR;
                            PaidAmt = (decimal)txnCreditSalesHDs.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnCreditSalesHDs.AmountPaid = totalPaidAmt;
                            txnCreditSalesHDs.AmountToBePaid = AmtTobePaid;
                            _context.Update(txnCreditSalesHDs);
                            break;
                        case "DebitNTImport":  //Operation 1/4
                            txnDebitNoteImportHds = await _operationcontext.TxnDebitNoteImportHds.FirstOrDefaultAsync(m => m.DebitNoteNo == strDocNo);
                            totAmt = (decimal)txnDebitNoteImportHds.TotalDebitAmountLkr;
                            PaidAmt = (decimal)txnDebitNoteImportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnDebitNoteImportHds.AmountPaid = totalPaidAmt;
                            txnDebitNoteImportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(txnDebitNoteImportHds);
                            break;
                        case "DebitNTExport":  // Operation 2/4
                            txnDebitNoteExportHds = await _operationcontext.TxnDebitNoteExportHds.FirstOrDefaultAsync(m => m.DebitNoteNo == strDocNo);
                            totAmt = (decimal)txnDebitNoteExportHds.TotalDebitAmountLkr;
                            PaidAmt = (decimal)txnDebitNoteExportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnDebitNoteExportHds.AmountPaid = totalPaidAmt;
                            txnDebitNoteExportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(txnDebitNoteExportHds);
                            break;
                        case "InvoiceImport":  // Operation 3/4
                            txnInvoiceImportHds = await _operationcontext.TxnInvoiceImportHds.FirstOrDefaultAsync(m => m.InvoiceNo == strDocNo);
                            totAmt = (decimal)txnInvoiceImportHds.TotalInvoiceAmountLkr;
                            PaidAmt = (decimal)txnInvoiceImportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnInvoiceImportHds.AmountPaid = totalPaidAmt;
                            txnInvoiceImportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(txnInvoiceImportHds);

                            break;
                        case "InvoiceExport":  // Operation 4/4
                            txnInvoiceExportHds = await _operationcontext.TxnInvoiceExportHds.FirstOrDefaultAsync(m => m.InvoiceNo == strDocNo);
                            totAmt = (decimal)txnInvoiceExportHds.TotalInvoiceAmountLkr;
                            PaidAmt = (decimal)txnInvoiceExportHds.AmountPaid;
                            totalPaidAmt = (decimal)PaidAmt + (decimal)amount;
                            AmtTobePaid = (decimal)totAmt - (decimal)totalPaidAmt;
                            txnInvoiceExportHds.AmountPaid = totalPaidAmt;
                            txnInvoiceExportHds.AmountToBePaid = AmtTobePaid;
                            _operationcontext.Update(txnInvoiceExportHds);
                            break;
                    }

                }

                _context.Update(txnReceiptHDs);

                await _context.SaveChangesAsync();
                await _operationcontext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }

        // Details 
        public async Task<IActionResult> Details(string id)
        {
            IEnumerable<SelectedReceiptDoc> receiptDocData = null;

            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == id),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == id),
                ReceiptDocMulti = receiptDocData
            };
            
            var PayCustomerID = "";
            var CustomerName = "";
            var customerType = tables.TxnReceiptHdMulti.FirstOrDefault().CustomerType;
            ViewData["CustomerType"] = customerType;

            switch (customerType)
            {
                case "Agent":  // Agent
                    ViewData["CustomerType"] = "Agent";
                    PayCustomerID = tables.TxnReceiptHdMulti.FirstOrDefault().AgentID; 
                    ViewData["AgentID"] = PayCustomerID;
                    var tblCustomerAg = _operationcontext.RefAgents.Where(t => t.AgentId == PayCustomerID);
                    if (tblCustomerAg != null)
                    {
                        CustomerName = tblCustomerAg.FirstOrDefault().AgentName;
                    }
                    break;

                case "ShippingLine": // Shipping Line
                    //ViewData["CustomerType"] = "ShippingLine";
                    //PayCustomerID = ShippingLine;
                    //ViewData["ShippingLineID"] = ShippingLine;
                    //var tblCustomer = _operationcontext.RefShippingLines.Where(t => t.ShippingLineId == PayCustomerID);
                    //if (tblCustomer != null)
                    //{
                    //    CustomerName = tblCustomer.FirstOrDefault().Name;
                    //}
                    break;

                case "Customer": // Customer 
                    ViewData["CustomerType"] = "Customer";
                    PayCustomerID = tables.TxnReceiptHdMulti.FirstOrDefault().CustomerID;
                    ViewData["CustomerID"] = PayCustomerID;
                    var tblSupplier = _context.RefCustomerAccOpt.Where(t => t.CustomerId == PayCustomerID);
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
            bool isChequeNumberExists = _context.TxnReceiptHDs.Any(x => x.ChequeNo == chequeNo);
            return Json(isChequeNumberExists);
        }
        // GET: Receipt/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var ReceiptNo = "";

            if (string.IsNullOrWhiteSpace(id))
            {
                return View("Error");
            }
            else
            {
                ReceiptNo = id;
            }

            // Account 1 Debite Note 2 Credit Sales/Invoice
            var txnDebitNoteAccHDs = _context.TxnDebitNoteAccHDs.Where(p => p.DebitNoteNo == "xxx");
            var txnCreditSalesHDs = _context.TxnCreditSalesHDs.Where(p => p.CreditSalesNo == "xxx");
            // Operation 1.Debit Note Import 2.Debit Note Export 3.Invoice Import 4.Invoice Export
            var txnDebitNoteImportHds = _operationcontext.TxnDebitNoteImportHds.Where(t => t.DebitNoteNo == "xxx");
            var txnDebitNoteExportHds = _operationcontext.TxnDebitNoteExportHds.Where(t => t.DebitNoteNo == "xxx");

            var txnInvoiceImportHds = _operationcontext.TxnInvoiceImportHds.Where(t => t.InvoiceNo == "xxx");
            var txnInvoiceExportHds = _operationcontext.TxnInvoiceExportHds.Where(t => t.InvoiceNo == "xxx");
            IEnumerable<SelectedReceiptDoc> receiptDocData = null;// payVoucherData = null;
            IEnumerable<SelectedReceiptDoc> receiptCreditSalesAcc = null;

            IEnumerable<SelectedReceiptDoc> receiptDebitNoteExport = null;
            IEnumerable<SelectedReceiptDoc> receiptDebitNoteImport = null;
            IEnumerable<SelectedReceiptDoc> receiptInvoiceImport = null;
            IEnumerable<SelectedReceiptDoc> receiptInvoiceExport = null;

            IEnumerable<SelectedReceiptDtlTableForEdit> receiptDtlTableData = null;


            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == ReceiptNo),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == ReceiptNo),
                ReceiptDocMulti = receiptDocData
            };


            var PayCustomerID = tables.TxnReceiptHdMulti.FirstOrDefault().CustomerID;
            var CustomerName = "";
            var customerType = tables.TxnReceiptHdMulti.FirstOrDefault().CustomerType;
            ViewData["CustomerType"] = "";
            switch (customerType)
            {
                case "Agent":  // Agent
                    ViewData["CustomerType"] = "Agent";
                    PayCustomerID = tables.TxnReceiptHdMulti.FirstOrDefault().AgentID;
                    ViewData["AgentID"] = PayCustomerID;
                    var tblCustomerAg = _operationcontext.RefAgents.Where(t => t.AgentId == PayCustomerID);
                    if (tblCustomerAg != null)
                    {
                        CustomerName = tblCustomerAg.FirstOrDefault().AgentName;
                    }
                    break;



                case "Customer": // Customer 
                    ViewData["CustomerType"] = "Customer";
                    PayCustomerID = tables.TxnReceiptHdMulti.FirstOrDefault().CustomerID;
                    ViewData["CustomerID"] = PayCustomerID;
                    var tblSupplier = _context.RefCustomerAccOpt.Where(t => t.CustomerId == PayCustomerID);
                    if (tblSupplier != null)
                    {
                        CustomerName = tblSupplier.FirstOrDefault().Name;
                    }
                    break;
            }

            ViewData["Customer"] = CustomerName;

            // Account 1 Debite Note 
            txnDebitNoteAccHDs = _context.TxnDebitNoteAccHDs.Where(p => p.AmountToBePaid != 0 && p.Agent == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);


            receiptDocData = txnDebitNoteAccHDs.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.DebitNoteNo,
                DocType = "DebitNTAdd",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType
            });

            // 2. Accounts- Credit Sales/Invoice
            txnCreditSalesHDs = _context.TxnCreditSalesHDs.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.CreditSalesNo);

            receiptCreditSalesAcc = txnCreditSalesHDs.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.CreditSalesNo,
                DocType = "CreditSales",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalAmountLKR,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = voucher.JobType
            });
            // Concatenate receiptCreditSalesAcc into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptCreditSalesAcc);

            // Operation 1.Debit Note Import
            if (customerType == "Customer")// Shipping Line
            {
                txnDebitNoteImportHds = _operationcontext.TxnDebitNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnDebitNoteImportHds = _operationcontext.TxnDebitNoteImportHds.Where(p => p.AmountToBePaid != 0 && p.AgentID == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }

            receiptDebitNoteImport = txnDebitNoteImportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.DebitNoteNo,
                DocType = "DebitNTImport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalDebitAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });

            // Concatenate receiptDebitNoteImport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptDebitNoteImport);


            // Operation 2.Debit Note Export
            if (customerType == "Customer")// Shipping Line
            {
                txnDebitNoteExportHds = _operationcontext.TxnDebitNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }
            else if (customerType == "Agent")  // Agent
            {
                txnDebitNoteExportHds = _operationcontext.TxnDebitNoteExportHds.Where(p => p.AmountToBePaid != 0 && p.AgentID == PayCustomerID).OrderByDescending(p => p.DebitNoteNo);
            }

            receiptDebitNoteExport = txnDebitNoteExportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.DebitNoteNo,
                DocType = "DebitNTExport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalDebitAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate receiptDebitNoteExport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptDebitNoteExport);


            // Operation 3.Invoice import

            txnInvoiceImportHds = _operationcontext.TxnInvoiceImportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.InvoiceNo);


            receiptInvoiceImport = txnInvoiceImportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.InvoiceNo,
                DocType = "InvoiceImport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalInvoiceAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Import"
            });

            // Concatenate receiptInvoiceImport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptInvoiceImport);


            // Operation 4.Invoice Export

            txnInvoiceExportHds = _operationcontext.TxnInvoiceExportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.InvoiceNo);


            receiptInvoiceExport = txnInvoiceExportHds.Select(voucher => new SelectedReceiptDoc
            {
                DocNo = voucher.InvoiceNo,
                DocType = "InvoiceExport",
                Date = voucher.Date,
                TotalAmountLkr = voucher.TotalInvoiceAmountLkr,
                AmountPaid = voucher.AmountPaid,
                AmountToBePaid = voucher.AmountToBePaid,
                JobNo = voucher.JobNo,
                JobType = "Export"
            });

            // Concatenate receiptInvoiceImport into receiptDocData
            receiptDocData = receiptDocData.Concat(receiptInvoiceExport);

            // ****** Removing docs in ReceiptDtl from  receiptDocData
            var receiptDtlDocNos = tables.TxnReceiptDtMulti
                                    .Where(t => t.ReceiptNo == ReceiptNo)
                                    .Select(t => t.DocNo)
                                    .ToList();
            // Filter receiptDocData to exclude items with matching DocNo in receiptDtlDocNos
            receiptDocData = receiptDocData
                .Where(doc => !receiptDtlDocNos.Contains(doc.DocNo))
                .ToList();

            tables.ReceiptDocMulti = receiptDocData;

            // Create receiptDtlTableData
            receiptDtlTableData = tables.TxnReceiptDtMulti.Select(voucher => new SelectedReceiptDtlTableForEdit
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
            tables.ReceiptDtlTableMulti = receiptDtlTableData;

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
        public async Task<IActionResult> Edit(string id, string customerType, string dtlItemsList, [Bind("ReceiptNo, Date, PaymentMethod, ExchangeRate, CustomerID, AgentID, ChequeNo, ChequeDate, ChequeBankID, ChequeAmount, Remarks, ReceiptTotalAmt, DebitAcc, CreditAcc, CreatedBy, CreatedDateTime, LastUpdatedBy, LastUpdatedDateTime, Canceled, CanceledBy, CanceledDateTime, CanceledReason, OtherAmountDescr, OtherAmount, Narration")] TxnReceiptHD txnReceiptHD)
        {
            if (id != txnReceiptHD.ReceiptNo)
            {
                return NotFound();
            }
            var TotalAmount = txnReceiptHD.ReceiptTotalAmt;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnReceiptHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            txnReceiptHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);
            txnReceiptHD.LastUpdatedBy = "Admin";
            txnReceiptHD.LastUpdatedDateTime = DateTime.Now;
            txnReceiptHD.CustomerType = customerType;

            if (txnReceiptHD.PaymentMethod == "Cash") // for thr cheque and Bank transfer, used the same fields
            {
                //txnReceiptHD.ChequeAmount = 0;  ChequeAmount field used to keep   Cash Amount 
                txnReceiptHD.ChequeNo = "";
            }

            if (ModelState.IsValid)
            {
                // Adding TxnReceiptDtl records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    // Remove exisiting dtl rows 
                    var rowsToDelete = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == id);
                    if (rowsToDelete != null && rowsToDelete.Any())
                    {
                        _context.TxnReceiptDtls.RemoveRange(rowsToDelete);
                    }


                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedRecieptPaymentItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {
                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnReceiptDtl
                            {
                                ReceiptNo = id, // Set PaymentNo to nextRefNo
                                DocNo = item.DocNo,
                                DocType = item.DocType,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,
                                JobNo = item.JobNo,
                                JobType = item.JobType,
                            };
                            _context.TxnReceiptDtls.Add(detailItem);

                        }
                    }
                }
                try
                {
                    _context.Update(txnReceiptHD);
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


            // ************************
            var tables = new TxnReceiptViewModel
            {
                TxnReceiptHdMulti = _context.TxnReceiptHDs.Where(t => t.ReceiptNo == id),
                TxnReceiptDtMulti = _context.TxnReceiptDtls.Where(t => t.ReceiptNo == id),
                //ReceiptDocMulti = receiptDocData
            };
            return View(tables);
        }
    }
}
