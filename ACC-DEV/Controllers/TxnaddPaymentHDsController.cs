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


namespace ACC_DEV.Views
{
    public class TxnaddPaymentHDsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
        private readonly FtlcolombOperationContext _operationcontext;

        public string jobNo { get; private set; }

        public TxnaddPaymentHDsController(FtlcolomboAccountsContext context, FtlcolombOperationContext operationcontext)
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
            var txnPaymentHDs = await _context.TxnPaymentHDs.OrderByDescending(p => p.PaymentNo).ToListAsync();

            var viewModel = new TxnPaymentViewMode
            {
                TxnPaymentHdMulti = txnPaymentHDs,
                // Initialize other properties of the view model if needed
            };

            return View(viewModel);
        }


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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, string customerType, [Bind("PaymentNo,Date,PaymentNoType,ExchangeRate,CustomerID,ChequeNo,ChequeDate,ChequeBankID,ChequeAmount,Remarks,PaymentTotalAmt,DebitAcc,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason, AgentID, ShippingLineID")] TxnPaymentHDs txnPaymentHD)
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
            txnPaymentHD.Canceled = false;
            txnPaymentHD.CreatedBy = "Admin";
            txnPaymentHD.CreatedDateTime = DateTime.Now;
            txnPaymentHD.CustomerType= customerType;


            var TotalAmount = txnPaymentHD.PaymentTotalAmt;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnPaymentHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            ModelState.Remove("PaymentNo");  // Auto generated
            if (ModelState.IsValid)
            {
                // Create Var for Additional Payment Voucher
                // swith 
                var voucherNo = "xxx";
                var PayVoutuerTable = await _context.TxnPaymentVoucherHds.FindAsync(voucherNo);

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
                                RefNo = item.RefNo,
                                RefType = item.RefType,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,

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


