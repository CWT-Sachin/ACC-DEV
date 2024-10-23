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
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.AspNetCore.Authorization;

namespace ACC_DEV.Controllers
{
    //[Authorize]
    public class TxnCreditInvoiceHdsController : Controller
    {

        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnCreditInvoiceHdsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
        {
            _context = context;
            _operationcontext = operationcontext;
        }


        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var txnPaymentHDs = await _context.TxnPaymentHDs.OrderByDescending(p => p.PaymentNo).ToListAsync();

            var viewModel = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = txnPaymentHDs,
                // Initialize other properties of the view model if needed
            };

            return View(viewModel);
        }



        // GET: Print Invoice 
        public async Task<IActionResult> RepPrintInvoice(string InvoiceNo)
        {
            if (InvoiceNo == null || _context.TxnInvoiceHds == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == InvoiceNo);

            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            var strjobNo = txnInvoiceExportHd.JobNo;

            var containerNo = _operationcontext.TxnStuffingPlanHds
                .Where(s => s.JobNumber == strjobNo)
                .Select(s => s.ContainerNo)
                .FirstOrDefault();

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == InvoiceNo)
                    .Include(t => t.InvoiceHdAcc),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == InvoiceNo)
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
                    ContainerNo = containerNo // I Just Set the Container Number 
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
            return View(tables);

        }


        // GET: txnInvoiceExportHds/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TxnInvoiceHds == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == id);

            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            var jobNo = txnInvoiceExportHd.JobNo;

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == id),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == id),
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

        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == id);

            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            jobNo = txnInvoiceExportHd.JobNo; // Set the jobNo property

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == id),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == id),
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

            return View(tables);
        }


        //[HttpPost]
        //public async Task<IActionResult> Approve(string id, bool approved)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var txnInvoiceExportHd = await _context.TxnInvoiceHds
        //        .FirstOrDefaultAsync(m => m.InvoiceNo == id);

        //    if (txnInvoiceExportHd == null)
        //    {
        //        return NotFound();
        //    }



        //    var txnInvoiceExportDtls = await _context.TxnInvoiceDtls
        //         .Include(t => t.ChargeItemNavigation)
        //        .Where(m => m.InvoiceNo == id)
        //        .ToListAsync();

        //    // Next RefNumber for Txn_Transactions
        //    var nextAccTxnNo = "";
        //    var TableIDAccTxn = "Txn_Transactions";
        //    var refLastNumberAccTxn = await _context.RefLastNumbers.FindAsync(TableIDAccTxn);
        //    if (refLastNumberAccTxn != null)
        //    {
        //        var nextNumberAccTxn = refLastNumberAccTxn.LastNumber + 1;
        //        refLastNumberAccTxn.LastNumber = nextNumberAccTxn;
        //        nextAccTxnNo = "TXN" + DateTime.Now.Year.ToString() + nextNumberAccTxn.ToString().PadLeft(5, '0');

        //        // _context.RefLastNumbers.Remove(refLastNumber);
        //    }
        //    else
        //    {
        //        return NotFound();
        //        //return View(tables);
        //    }

        //    if (txnInvoiceExportDtls != null)
        //    {
        //        var SerialNo_AccTxn = 1;
        //        var AccTxnDescription = " Local Debtors INV: " + txnInvoiceExportHd.InvoiceNo;
        //        // First Acc transaction 
        //        TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
        //        NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
        //        NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
        //        NewRowAccTxnFirst.Date = txnInvoiceExportHd.Date; //  Jurnal Date 
        //        NewRowAccTxnFirst.Description = AccTxnDescription;
        //        NewRowAccTxnFirst.TxnAccCode = txnInvoiceExportHd.DebitAcc;
        //        NewRowAccTxnFirst.Dr = (decimal)txnInvoiceExportHd.TotalInvoiceAmountLkr;
        //        NewRowAccTxnFirst.Cr = (decimal)0;
        //        NewRowAccTxnFirst.RefNo = txnInvoiceExportHd.InvoiceNo; // Invoice No
        //        NewRowAccTxnFirst.Note = "";
        //        NewRowAccTxnFirst.Reconciled = false;
        //        NewRowAccTxnFirst.DocType = "Invoice";
        //        NewRowAccTxnFirst.IsMonthEndDone = false;
        //        NewRowAccTxnFirst.CreatedBy = "Admin";
        //        NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
        //        NewRowAccTxnFirst.Canceled = false;

        //        _context.TxnTransactions.Add(NewRowAccTxnFirst);

        //        foreach (var item in txnInvoiceExportDtls)
        //        {
        //            // Transaction table Insert 
        //            TxnTransactions NewRowAccTxn = new TxnTransactions();

        //            SerialNo_AccTxn = SerialNo_AccTxn + 1;
        //            NewRowAccTxn.TxnNo = nextAccTxnNo;
        //            NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
        //            NewRowAccTxn.Date = txnInvoiceExportHd.Date; //  Jurnal Date 
        //            NewRowAccTxn.Description = item.ChargeItemNavigation.Description +"INV: "+item.InvoiceNo;
        //            NewRowAccTxn.TxnAccCode = item.AccNo;
        //            NewRowAccTxn.Dr = (decimal)0;
        //            if (item.Currency == "USD")
        //            {
        //                var AmtLkr = item.Amount * txnInvoiceExportHd.ExchangeRate; // convert to LKR
        //                NewRowAccTxn.Cr = (decimal)AmtLkr;
        //            }
        //            else
        //            {
        //                NewRowAccTxn.Cr = (decimal)item.Amount;
        //            }
        //            NewRowAccTxn.RefNo = item.InvoiceNo; // Invoice No
        //            NewRowAccTxn.Note = "";
        //            NewRowAccTxn.Reconciled = false;
        //            NewRowAccTxn.DocType = "Invoice";
        //            NewRowAccTxn.IsMonthEndDone = false;
        //            NewRowAccTxn.CreatedBy = "Admin";
        //            NewRowAccTxn.CreatedDateTime = DateTime.Now;
        //            NewRowAccTxn.Canceled = false;

        //            _context.TxnTransactions.Add(NewRowAccTxn);
        //        }
        //    }


        //    /// Update the Approved property based on the form submission
        //    txnInvoiceExportHd.Approved = approved;
        //    txnInvoiceExportHd.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
        //    txnInvoiceExportHd.ApprovedDateTime = DateTime.Now;

        //    _context.Update(txnInvoiceExportHd);
        //    _context.RefLastNumbers.Update(refLastNumberAccTxn); // Transaction last number
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        //}



        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier)
        {
            var txnPaymentVoucherAddtional = _context.TxnPaymentVoucherHds.Where(t => t.PayVoucherNo == "xxx");

            var txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(t => t.PayVoucherNo == "xxx");
            var txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(t => t.PayVoucherNo == "xxx");

            var PayCustomerID = "";
            var CustomerName = "";
            ViewData["CustomerType"] = "";
            switch (customerType)
            {
                case "Overseas":
                    ViewData["CustomerType"] = "Overseas";
                    PayCustomerID = POAgent;
                    ViewData["AgentID"] = POAgent;
                    var tblCustomerAg = _operationcontext.RefAgents.Where(t => t.AgentId == PayCustomerID);
                    if (tblCustomerAg != null)
                    {
                        CustomerName = tblCustomerAg.FirstOrDefault().AgentName;
                    }
                    break;

                case "Local":
                    ViewData["CustomerType"] = "Local";
                    PayCustomerID = ShippingLine;
                    ViewData["ShippingLineID"] = ShippingLine;
                    var tblCustomer = _operationcontext.RefShippingLines.Where(t => t.ShippingLineId == PayCustomerID);
                    if (tblCustomer != null)
                    {
                        CustomerName = tblCustomer.FirstOrDefault().Name;
                    }
                    break;

                case "Supplier":
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

            IEnumerable<SelectedPaymentVocher> payVoucherData = null;

            ViewData["PaymentVoucherType"] = "Select payment Voucher Type";

            switch (searchType)
            {
                case "Export":
                    ViewData["PaymentVoucherType"] = "EXPORT";
                    txnPaymentVoucherExport = _operationcontext.TxnPaymentVoucherExportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);

                    payVoucherData = txnPaymentVoucherExport.Select(voucher => new SelectedPaymentVocher
                    {
                        PayVoucherNo = voucher.PayVoucherNo,
                        Date = voucher.Date,
                        TotalPayVoucherAmountLkr = voucher.TotalPayVoucherAmountLkr,
                        AmountPaid = voucher.AmountPaid,
                        AmountToBePaid = voucher.AmountToBePaid
                    });
                    break;
                case "Import":
                    ViewData["PaymentVoucherType"] = "IMPORT";

                    txnPaymentVoucherimport = _operationcontext.TxnPaymentVoucherImportHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);

                    payVoucherData = txnPaymentVoucherimport.Select(voucher => new SelectedPaymentVocher
                    {
                        PayVoucherNo = voucher.PayVoucherNo,
                        Date = voucher.Date,
                        TotalPayVoucherAmountLkr = voucher.TotalPayVoucherAmountLkr,
                        AmountPaid = voucher.AmountPaid,
                        AmountToBePaid = voucher.AmountToBePaid
                    });
                    break;
                case "Additional":
                    ViewData["PaymentVoucherType"] = "ADDITIONAL";

                    txnPaymentVoucherAddtional = _context.TxnPaymentVoucherHds.Where(p => p.AmountToBePaid != 0 && p.Customer == PayCustomerID).OrderByDescending(p => p.PayVoucherNo);

                    payVoucherData = txnPaymentVoucherAddtional.Select(voucher => new SelectedPaymentVocher
                    {
                        PayVoucherNo = voucher.PayVoucherNo,
                        Date = voucher.Date,
                        TotalPayVoucherAmountLkr = voucher.TotalPayVoucherAmountLkr,
                        AmountPaid = voucher.AmountPaid,
                        AmountToBePaid = voucher.AmountToBePaid
                    });
                    break;

                default:
                    ViewData["PaymentVoucherType"] = "Select payment Voucher Type";

                    // Assign an empty enumerable if searchType doesn't match any case
                    payVoucherData = Enumerable.Empty<SelectedPaymentVocher>();
                    break;
            }


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


        //I added a private function GetChargeItemAccNo that takes a ChargeId as a 
        //parameter and returns the corresponding AccNo by querying the RefChargeItems table in your database.
        //Then, inside your loop that iterates through DtailItemdataTable,
        //I call this function to get the AccNo for each ChargeItem and set it in the DetailItem.AccNo property.



        // POST: txnInvoiceExportHds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("InvoiceNo,Date,JobNo,Blno,Customer,ExchangeRate,TotalInvoiceAmountLkr,TotalInvoiceAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,CreditAcc,DebitAcc")] TxnInvoiceHd txnInvoiceHd)
        {

            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_InvoiceHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "INVEX" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                      txnInvoiceHd.InvoiceNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }
            txnInvoiceHd.Canceled = false;
            txnInvoiceHd.Approved = false;
            txnInvoiceHd.CreatedBy = "Admin";
            txnInvoiceHd.CreatedDateTime = DateTime.Now;
            txnInvoiceHd.DebitAcc = "ACC0023"; // LOCAL DEBTORS  2200-01
            txnInvoiceHd.CreditAcc = null;

            var TransDocMode = "Export"; 
            var TransDocType = "Revenue";
            var TransDocVender = "";

            ModelState.Remove("InvoiceNo");  // Auto genrated
            if (ModelState.IsValid)
            {
                // Adding CargoBreakDown records
                if (!string.IsNullOrWhiteSpace(dtlItemsList))
                {
                    var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnInvoiceDtl>>(dtlItemsList);
                    if (DtailItemdataTable != null)
                    {
                        foreach (var item in DtailItemdataTable)
                        {
                            TxnInvoiceDtl DetailItem = new TxnInvoiceDtl();
                            DetailItem.InvoiceNo = nextRefNo; // New Invoice Number
                            DetailItem.SerialNo = item.SerialNo;
                            DetailItem.ChargeItem = item.ChargeItem;
                            DetailItem.Unit = item.Unit;
                            DetailItem.Rate = item.Rate;
                            DetailItem.Currency = item.Currency;
                            DetailItem.Qty = item.Qty;
                            DetailItem.Amount = item.Amount;
                            DetailItem.CreatedDate = DateTime.Now;
                            //DetailItem.AccNo = GetChargeItemAccNo(item.ChargeItem, TransDocMode, TransDocType, TransDocVender);
                            _context.TxnInvoiceDtls.Add(DetailItem);
                        }
                    }
                }
                _context.Add(txnInvoiceHd);
                _context.RefLastNumbers.Update(refLastNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == "xxx"),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == "xxx"),

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
            ViewData["Customer"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

            return View(tables);
        }


        //private string GetChargeItemAccNo(string chargeItemNo, string mode, string Type, string Vender)
        //{
        //    var chargeItem = _context.RefChargeItems.FirstOrDefault(x => x.ChargeId == chargeItemNo);
        //    var AccNumber = "";

        //    if (mode == "Import")
        //    {
        //        if (Type == "Revenue")
        //        {
        //            AccNumber = chargeItem?.AccNo_Revenue_Imp;
        //        }
        //        else // Expenses
        //        {
        //            if (Vender == "Liner")
        //            {
        //                AccNumber = chargeItem?.AccNo_Expense_Imp_Liner;
        //            }
        //            else // Vender == "Agent"
        //            {
        //                AccNumber = chargeItem?.AccNo_Expense_Imp_Agent;
        //            }
        //        }
        //    }
        //    else // Export
        //    {
        //        if (Type == "Revenue")
        //        {
        //            AccNumber = chargeItem?.AccNo_Revenue_Exp;
        //        }
        //        else // Type == Expenses
        //        {
        //            if (Vender == "Liner")
        //            {
        //                AccNumber = chargeItem?.AccNo_Expense_Exp_Liner;
        //            }
        //            else // Vender == "Agent"
        //            {
        //                AccNumber = chargeItem?.AccNo_Expense_Exp_Agent;
        //            }
        //        }

        //    }

        //    return AccNumber;
        //}




        // GET: txnInvoiceExportHds/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds.FindAsync(id);
            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            var InvoiceNo = id;
            var JobNo = txnInvoiceExportHd.JobNo; // Assuming JobNo is an integer; convert to string if needed

            var tables = new InvoiceViewModel
            {
                InvoiceHdMulti = _context.TxnInvoiceHds.Where(t => t.InvoiceNo == InvoiceNo),
                InvoiceDtMulti = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == InvoiceNo),
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
                ExportJobDtlMulti = _operationcontext.TxnExportJobDtls.Where(t => t.JobNo == JobNo),
            };
            List<SelectListItem> Currency = new List<SelectListItem>
                {
                    new SelectListItem { Value = "LKR", Text = "LKR" },
                    new SelectListItem { Value = "USD", Text = "USD" }

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
// POST: txnInvoiceExportHds/Edit/5
// To protect from overposting attacks, enable the specific properties you want to bind to.
// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string dtlItemsList, string id, [Bind("InvoiceNo,Date,JobNo,Blno,Customer,ExchangeRate,TotalInvoiceAmountLkr,TotalInvoiceAmountUsd,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,CreditAcc,DebitAcc")] TxnInvoiceHd txnInvoiceHd)
        {
            if (id != txnInvoiceHd.InvoiceNo)
            {
                return NotFound();
            }
            txnInvoiceHd.LastUpdatedBy = "Admin";
            txnInvoiceHd.LastUpdatedDateTime = DateTime.Now;
            txnInvoiceHd.DebitAcc = "ACC0023"; // LOCAL DEBTORS  2200-01
            txnInvoiceHd.CreditAcc = null;

            var TransDocMode = "Export"; 
            var TransDocType = "Revenue";
            var TransDocVender = "";

            if (ModelState.IsValid)
            {
                try
                {
                    // Adding Invoice Items records
                    if (!string.IsNullOrWhiteSpace(dtlItemsList))
                    {
                        var rowsToDelete = _context.TxnInvoiceDtls.Where(t => t.InvoiceNo == id);
                        if (rowsToDelete != null || rowsToDelete.Any())
                        {
                            // Remove the rows from the database context
                            _context.TxnInvoiceDtls.RemoveRange(rowsToDelete);
                        }
                        var DtailItemdataTable = JsonConvert.DeserializeObject<List<TxnInvoiceDtl>>(dtlItemsList);
                        if (DtailItemdataTable != null)
                        {
                            foreach (var item in DtailItemdataTable)
                            {
                                TxnInvoiceDtl DetailItem = new TxnInvoiceDtl();
                                DetailItem.InvoiceNo = id; // New Invoice Number
                                DetailItem.SerialNo = item.SerialNo;
                                DetailItem.ChargeItem = item.ChargeItem;
                                DetailItem.Unit = item.Unit ?? "DefaultUnit"; // Set a default value if 'Unit' is null
                                DetailItem.Rate = item.Rate;
                                DetailItem.Currency = item.Currency;
                                DetailItem.Qty = item.Qty;
                                DetailItem.Amount = item.Amount;
                                DetailItem.CreatedDate = DateTime.Now;
                                //DetailItem.AccNo = GetChargeItemAccNo(item.ChargeItem, TransDocMode, TransDocType, TransDocVender);
                                _context.TxnInvoiceDtls.Add(DetailItem);
                            }
                        }
                    }

                    _context.Update(txnInvoiceHd);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!txnInvoiceExportHdExists(txnInvoiceHd.InvoiceNo))
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
            return View(txnInvoiceHd);
        }

        // GET: txnInvoiceExportHds/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TxnInvoiceHds == null)
            {
                return NotFound();
            }

            var txnInvoiceExportHd = await _context.TxnInvoiceHds
                .FirstOrDefaultAsync(m => m.InvoiceNo == id);
            if (txnInvoiceExportHd == null)
            {
                return NotFound();
            }

            return View(txnInvoiceExportHd);
        }

        // POST: txnInvoiceExportHds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TxnInvoiceHds == null)
            {
                return Problem("Entity set 'FtlcolombOperationContext.TxnInvoiceExportHds'  is null.");
            }
            var txnInvoiceExportHd = await _context.TxnInvoiceHds.FindAsync(id);
            if (txnInvoiceExportHd != null)
            {
                _context.TxnInvoiceHds.Remove(txnInvoiceExportHd);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool txnInvoiceExportHdExists(string id)
        {
          return (_context.TxnInvoiceHds?.Any(e => e.InvoiceNo == id)).GetValueOrDefault();
        }
    }
}
