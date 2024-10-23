 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PowerAcc.Models;
using ACC_DEV.ViewModel;
using Newtonsoft.Json;
using ACC_DEV.Data;
using ACC_DEV.DataOperation;
using ACC_DEV.CommonMethods;
using ACC_DEV.Models;


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

            //var viewModel = new TxnPaymentViewMode
            //{
            //    TxnPaymentHdMulti = txnPaymentHDs,
            //    // Initialize other properties of the view model if needed
            //};

            //return View(viewModel);
            return View(txnPurchasVoucherHDs);
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



            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpts.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
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
                //var voucherNo = "xxx";
                //var PayVoutuerTable = await _context.TxnPaymentVoucherHds.FindAsync(voucherNo);

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

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpts.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
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

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpts.OrderBy(c => c.Name), "ShippingLineId", "Name", txnPurchasVoucherHD.ShippingLine);
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", txnPurchasVoucherHD.Supplier);
            ViewData["CustomerList"] = new SelectList(_operationcontext.RefCustomers.OrderBy(c => c.Name), "CustomerId", "Name", txnPurchasVoucherHD.Supplier);
            ViewData["ChartofAccounts"] = new SelectList(_context.RefChartOfAccs.OrderBy(c => c.AccName), "AccNo", "AccName", txnPurchasVoucherHD.MainAcc);
            ViewData["RefBankList"] = new SelectList(_context.Set<Ref_BankAcc>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.Description), "ID", "Description");
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

        // GET: Print Invoice 
        public async Task<IActionResult> RepPrintPurchaseVoucher(string id)
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

            return View(tables);

        }

        private bool TxnPurchasVoucherHdExists(string id)
        {
            return _context.TxnPurchasVoucherHDs.Any(e => e.PurchasVoucherNo == id);
        }





    }
}


