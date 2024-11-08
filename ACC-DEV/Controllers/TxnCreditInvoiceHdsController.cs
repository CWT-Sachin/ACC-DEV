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
using ACC_DEV.CommonMethods;
using Microsoft.EntityFrameworkCore.Metadata;


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
            //var txnCreditSalesHDs = await _context.TxnCreditSalesHDs.OrderByDescending(p => p.CreditSalesNo).ToListAsync();
            var txnCreditSalesHDs =  _context.TxnCreditSalesHDs.Include(t =>t.CustomerNavigation)
                .OrderByDescending(p => p.CreditSalesNo).ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                txnCreditSalesHDs = txnCreditSalesHDs.Where(t => t.CreditSalesNo.Contains(searchString)).OrderByDescending(t => t.CreditSalesNo).ToList();
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = txnCreditSalesHDs.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = txnCreditSalesHDs.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;

            return View(data);
        }



        // GET: Print Invoice 
        public async Task<IActionResult> RepPrintCreditInvoice(string CreditSalesNo)
        {
            if (CreditSalesNo == null || _context.TxnCreditSalesHDs == null)
            {
                return NotFound();
            }

            var txnCreditSalesHd = await _context.TxnCreditSalesHDs
                .FirstOrDefaultAsync(m => m.CreditSalesNo == CreditSalesNo);

            if (txnCreditSalesHd == null)
            {
                return NotFound();
            }

            var strjobNo = txnCreditSalesHd.JobNo;

            var containerNo = _operationcontext.TxnStuffingPlanHds
                .Where(s => s.JobNumber == strjobNo)
                .Select(s => s.ContainerNo)
                .FirstOrDefault();

            var tables = new TxnCreditSalesViewModel
            {
                TxnCreditSalesHDMulti = _context.TxnCreditSalesHDs.Where(t => t.CreditSalesNo == CreditSalesNo)
                    .Include(t => t.CustomerNavigation),
                TxnCreditSalesDtlMulti = _context.TxnCreditSalesDtls.Where(t => t.CreditSalesNo == CreditSalesNo),
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


    
        

        public async Task<IActionResult> Approve(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var txnCreditSalesHD =  await _context.TxnCreditSalesHDs
                .FirstOrDefaultAsync(m => m.CreditSalesNo == id);


            if (txnCreditSalesHD == null)
            {
                return NotFound();
            }

            jobNo = txnCreditSalesHD.JobNo; // Set the jobNo property

            var tables = new TxnCreditSalesViewModel
            {
                TxnCreditSalesHDMulti = _context.TxnCreditSalesHDs.Where(t => t.CreditSalesNo == id),
                TxnCreditSalesDtlMulti = _context.TxnCreditSalesDtls.Where(t => t.CreditSalesNo == id),
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

            var txnCreditSalesHD = await _context.TxnCreditSalesHDs
                .FirstOrDefaultAsync(m => m.CreditSalesNo == id);

            if (txnCreditSalesHD == null)
            {
                return NotFound();
            }

            // Inserting Transaction Data to Acccounts 
            var txnCreditSalesDtl = await _context.TxnCreditSalesDtls
                                .Where(m => m.CreditSalesNo == id)
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

            if (txnCreditSalesHD != null)
            {
                var SerialNo_AccTxn = 1;
                var AccTxnDescription = txnCreditSalesHD.Narration;
                // First Acc transaction 
                TxnTransactions NewRowAccTxnFirst = new TxnTransactions();
                NewRowAccTxnFirst.TxnNo = nextAccTxnNo;
                NewRowAccTxnFirst.TxnSNo = SerialNo_AccTxn;
                NewRowAccTxnFirst.Date = txnCreditSalesHD.Date; //  Jurnal Date 
                NewRowAccTxnFirst.Description = AccTxnDescription;
                NewRowAccTxnFirst.TxnAccCode = txnCreditSalesHD.MainAcc;
                NewRowAccTxnFirst.Dr = (decimal)txnCreditSalesHD.TotalAmountLKR;
                NewRowAccTxnFirst.Cr = (decimal)0;
                NewRowAccTxnFirst.RefNo = txnCreditSalesHD.CreditSalesNo; // Invoice No
                NewRowAccTxnFirst.Note = "";
                NewRowAccTxnFirst.Reconciled = false;
                NewRowAccTxnFirst.DocType = "CreditSales";
                NewRowAccTxnFirst.IsMonthEndDone = false;
                NewRowAccTxnFirst.CreatedBy = "Admin";
                NewRowAccTxnFirst.CreatedDateTime = DateTime.Now;
                NewRowAccTxnFirst.Canceled = false;

                NewRowAccTxnFirst.JobNo = txnCreditSalesHD.JobNo;
                NewRowAccTxnFirst.JobType = txnCreditSalesHD.JobType;

                _context.TxnTransactions.Add(NewRowAccTxnFirst);

                foreach (var item in txnCreditSalesDtl)
                {
                    // Transaction table Insert 
                    TxnTransactions NewRowAccTxn = new TxnTransactions();

                    SerialNo_AccTxn = SerialNo_AccTxn + 1;
                    NewRowAccTxn.TxnNo = nextAccTxnNo;
                    NewRowAccTxn.TxnSNo = SerialNo_AccTxn;
                    NewRowAccTxn.Date = txnCreditSalesHD.Date; //  Jurnal Date 
                    NewRowAccTxn.Description = item.Description;
                    NewRowAccTxn.TxnAccCode = item.LineAccNo;
                    NewRowAccTxn.Dr = (decimal)0;
                    NewRowAccTxn.Cr = (decimal)item.Amount;
                    NewRowAccTxn.RefNo = txnCreditSalesHD.CreditSalesNo; // Invoice No
                    NewRowAccTxn.Note = "";
                    NewRowAccTxn.Reconciled = false;
                    NewRowAccTxn.DocType = "CreditSales";
                    NewRowAccTxn.IsMonthEndDone = false;
                    NewRowAccTxn.CreatedBy = "Admin";
                    NewRowAccTxn.CreatedDateTime = DateTime.Now;
                    NewRowAccTxn.Canceled = false;
                    NewRowAccTxn.JobNo = txnCreditSalesHD.JobNo;
                    NewRowAccTxn.JobType = txnCreditSalesHD.JobType;

                    _context.TxnTransactions.Add(NewRowAccTxn);
                }
            }
            // END Inserting Transaction Data to Acccounts 

            /// Update the Approved property based on the form submission
            txnCreditSalesHD.Approved = approved;
            txnCreditSalesHD.ApprovedBy = "CurrentUserName"; // Replace with the actual user name
            txnCreditSalesHD.ApprovedDateTime = DateTime.Now;

            _context.Update(txnCreditSalesHD);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to the appropriate action
        }



        // GET: txnInvoiceExportHds/Create
        public IActionResult Create(string searchType, string customerType, string ShippingLine, string POAgent, string Supplier)
        {
            var txnPurchasVoucherHD = _context.TxnCreditSalesHDs.Where(t => t.CreditSalesNo == "xxx");


            var tables = new TxnCreditSalesViewModel
            {
                TxnCreditSalesHDMulti = _context.TxnCreditSalesHDs.Where(t => t.CreditSalesNo == "xxx"),
                TxnCreditSalesDtlMulti = _context.TxnCreditSalesDtls.Where(t => t.CreditSalesNo == "xxx"),
            };


            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["CustomerList"] = new SelectList(_context.RefCustomerAccOpt.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

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


        //I added a private function GetChargeItemAccNo that takes a ChargeId as a 
        //parameter and returns the corresponding AccNo by querying the RefChargeItems table in your database.
        //Then, inside your loop that iterates through DtailItemdataTable,
        //I call this function to get the AccNo for each ChargeItem and set it in the DetailItem.AccNo property.



        // POST: txnInvoiceExportHds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dtlItemsList, [Bind("CreditSalesNo,Date,ExchangeRate,Customer,DocType,JobNo,BLNo,MainAcc,Narration,MainAccAmount,TotalAmountLKR,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,AmountPaid,AmountToBePaid,TotAmtWord,JobType")] TxnCreditSalesHD txnCreditSalesHD)
        {
            // Next RefNumber for txnImportJobDtl
            var nextRefNo = "";
            var TableID = "Txn_CreditSalesHD";
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);
            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                nextRefNo = "CS" + DateTime.Now.Year.ToString() + nextNumber.ToString().PadLeft(5, '0');
                txnCreditSalesHD.CreditSalesNo = nextRefNo;

                // _context.RefLastNumbers.Remove(refLastNumber);
            }
            else
            {
                return NotFound();
                //return View(tables);
            }

            txnCreditSalesHD.AmountPaid = 0;
            txnCreditSalesHD.AmountToBePaid = txnCreditSalesHD.TotalAmountLKR;
            txnCreditSalesHD.Approved = false;

            txnCreditSalesHD.Canceled = false;
            txnCreditSalesHD.CreatedBy = "Admin";
            txnCreditSalesHD.CreatedDateTime = DateTime.Now;


            var TotalAmount = txnCreditSalesHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnCreditSalesHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            ModelState.Remove("CreditSalesNo");  // Auto generated
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
                    var detailItemList = JsonConvert.DeserializeObject<List<SelectedCreditSalesItem>>(dtlItemsList);
                    if (detailItemList != null)
                    {

                        foreach (var item in detailItemList)
                        {
                            var detailItem = new TxnCreditSalesDtl
                            {
                                CreditSalesNo = nextRefNo, // Set PaymentNo to nextRefNo
                                SerialNo = item.SerialNo,// (decimal)item.SerialNo,
                                LineAccNo = item.LineAccNo,
                                Description = item.Description,
                                Amount = item.Amount,
                                CreatedDateTime = DateTime.Now,
                            };
                            _context.TxnCreditSalesDtls.Add(detailItem);
                        }
                    }
                }
                try
                {

                    _context.Add(txnCreditSalesHD);
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

            var tables = new TxnCreditSalesViewModel
            {
                TxnCreditSalesHDMulti = _context.TxnCreditSalesHDs.Where(t => t.CreditSalesNo == "xxx"),
                TxnCreditSalesDtlMulti = _context.TxnCreditSalesDtls.Where(t => t.CreditSalesNo == "xxx"),
            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["CustomerList"] = new SelectList(_context.RefCustomerAccOpt.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

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


        private string GetChargeItemAccNo(string chargeItemNo, string mode, string Type, string Vender)
        {
            var chargeItem = _context.RefChargeItems.FirstOrDefault(x => x.ChargeId == chargeItemNo);
            var AccNumber = "";

            if (mode == "Import")
            {
                if (Type == "Revenue")
                {
                    AccNumber = chargeItem?.AccNo_Revenue_Imp;
                }
                else // Expenses
                {
                    if (Vender == "Liner")
                    {
                        AccNumber = chargeItem?.AccNo_Expense_Imp_Liner;
                    }
                    else // Vender == "Agent"
                    {
                        AccNumber = chargeItem?.AccNo_Expense_Imp_Agent;
                    }
                }
            }
            else // Export
            {
                if (Type == "Revenue")
                {
                    AccNumber = chargeItem?.AccNo_Revenue_Exp;
                }
                else // Type == Expenses
                {
                    if (Vender == "Liner")
                    {
                        AccNumber = chargeItem?.AccNo_Expense_Exp_Liner;
                    }
                    else // Vender == "Agent"
                    {
                        AccNumber = chargeItem?.AccNo_Expense_Exp_Agent;
                    }
                }

            }

            return AccNumber;
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

            var tables = new TxnCreditSalesViewModel
            {
                TxnCreditSalesHDMulti = _context.TxnCreditSalesHDs.Where(t => t.CreditSalesNo == VoucherNo),
                TxnCreditSalesDtlMulti = _context.TxnCreditSalesDtls.Where(t => t.CreditSalesNo == VoucherNo),
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


        // GET: Credit sales/Edit/5
        public async Task<IActionResult> Edit(string id)
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

            var tables = new TxnCreditSalesViewModel
            {
                TxnCreditSalesHDMulti = _context.TxnCreditSalesHDs.Where(t => t.CreditSalesNo == VoucherNo),
                TxnCreditSalesDtlMulti = _context.TxnCreditSalesDtls.Where(t => t.CreditSalesNo == VoucherNo),
            };

            ViewData["ShippingLine"] = new SelectList(_context.RefShippingLineAccOpt.OrderBy(c => c.Name), "ShippingLineId", "Name", "ShippingLineId");
            ViewData["Suppliers"] = new SelectList(_context.RefSuppliers.OrderBy(c => c.Name), "SupplierId", "Name", "SupplierId");

            ViewData["CustomerList"] = new SelectList(_context.RefCustomerAccOpt.OrderBy(c => c.Name), "CustomerId", "Name", "CustomerId");

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
        public async Task<IActionResult> Edit(string id, string dtlItemsList, [Bind("CreditSalesNo,Date,ExchangeRate,Customer,DocType,JobNo,BLNo,MainAcc,Narration,MainAccAmount,TotalAmountLKR,Remarks,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,Canceled,CanceledBy,CanceledDateTime,CanceledReason,Approved,ApprovedBy,ApprovedDateTime,AmountPaid,AmountToBePaid,TotAmtWord,JobType")] TxnCreditSalesHD txnCreditSalesHD)
        {
            if (id != txnCreditSalesHD.CreditSalesNo)
            {
                return NotFound();
            }

            txnCreditSalesHD.LastUpdatedBy = "Admin";
            txnCreditSalesHD.LastUpdatedDateTime = DateTime.Now;

            txnCreditSalesHD.AmountPaid = 0;
            txnCreditSalesHD.AmountToBePaid = txnCreditSalesHD.TotalAmountLKR;

            var TotalAmount = txnCreditSalesHD.TotalAmountLKR;
            var CommonMethodClass = new CommonMethodClass(); // to calll the convert to word 

            txnCreditSalesHD.TotAmtWord = CommonMethodClass.ConvertToWords((decimal)TotalAmount);

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(dtlItemsList))
                    {
                        var rowsToDelete = _context.TxnCreditSalesDtls.Where(t => t.CreditSalesNo == id);
                        if (rowsToDelete != null && rowsToDelete.Any())
                        {
                            _context.TxnCreditSalesDtls.RemoveRange(rowsToDelete);
                        }

                        var detailItemList = JsonConvert.DeserializeObject<List<SelectedCreditSalesItem>>(dtlItemsList);
                        if (detailItemList != null)
                        {
                            foreach (var item in detailItemList)
                            {
                                var detailItem = new TxnCreditSalesDtl
                                {
                                    CreditSalesNo = id, // Set PaymentNo to nextRefNo
                                    SerialNo = item.SerialNo,// (decimal)item.SerialNo,
                                    LineAccNo = item.LineAccNo,
                                    Description = item.Description,
                                    Amount = item.Amount,
                                    CreatedDateTime = DateTime.Now,
                                };
                                _context.TxnCreditSalesDtls.Add(detailItem);
                            }
                        }
                    }

                    _context.Update(txnCreditSalesHD);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TxnCreditSalesHDExists(txnCreditSalesHD.CreditSalesNo))
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

            return View(txnCreditSalesHD);
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

        private bool TxnCreditSalesHDExists(string id)
        {
            return _context.TxnCreditSalesHDs.Any(e => e.CreditSalesNo == id);
        }
    }
}
