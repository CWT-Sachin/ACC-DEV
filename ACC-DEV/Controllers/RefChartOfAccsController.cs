using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.Models;
using ACC_DEV.ViewModel;
using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ACC_DEV.Data;

namespace ACC_DEV.Views
{
    public class RefChartOfAccsController : Controller
    {
        private readonly FtlcolomboAccountsContext _context;
      

        public RefChartOfAccsController(FtlcolomboAccountsContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string searchString, string searchType, int pg = 1)
        {
            var refChartOfAccs = _context.RefChartOfAccs.OrderByDescending(p => p.AccNo);

            if (!String.IsNullOrEmpty(searchString))
            {
                if (searchType == "AccountCode")
                {
                    refChartOfAccs = refChartOfAccs.Where(refChartOfAccs => refChartOfAccs.AccCode.Contains(searchString)).OrderByDescending(p => p.AccNo);
                }
                else if (searchType == "AccountName")
                {
                    refChartOfAccs = refChartOfAccs.Where(refChartOfAccs => refChartOfAccs.AccName.Contains(searchString)).OrderByDescending(p => p.AccNo);
                }

                if (!refChartOfAccs.Any())
                {
                    ViewData["ChartOfAccsFound"] = "ChartOfAccs Number: " + searchString + " not found";
                }
                else
                {
                    ViewData["ChartOfAccsFound"] = "";
                }
            }

            const int pageSize = 7;
            if (pg < 1)
                pg = 1;
            int recsCount = refChartOfAccs.Count();
            var pager = new Pager(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = new ChartOfAllAccViewModel
            {
                refChartOfAccsMulti = refChartOfAccs.Skip(recSkip).Take(pager.PageSize).ToList(),
                ChartOfAccViewModelMulti = GetCOATreeviewData()
            };

            ViewBag.Pager = pager;
            return View(data);
        }






















        private List<ChartOfAccViewModel> GetCOATreeviewData()
        {
            List<RefChartOfAcc> chartOfAccounts = _context.RefChartOfAccs.Include(x => x.Children).ToList();
            Dictionary<string, ChartOfAccViewModel> nodesMap = new Dictionary<string, ChartOfAccViewModel>();
            List<ChartOfAccViewModel> dtoList = new List<ChartOfAccViewModel>();

            foreach (var node in chartOfAccounts)
            {
                ChartOfAccViewModel dto = new ChartOfAccViewModel
                {
                    AccNo = node.AccNo,
                    ParentNo = node.ParentNo ?? string.Empty, // Handle NULL by using an empty string
                    //AccName = node.AccName,
                    AccName = node.AccName + "- " + node.AccCode,
                    Children = new List<ChartOfAccViewModel>()
                };

                nodesMap[node.AccNo] = dto;

                if (node.ParentNo == node.AccNo || string.IsNullOrEmpty(node.ParentNo))
                {
                    dtoList.Add(dto);
                }
            }

            foreach (var node in chartOfAccounts)
            {
                if (node.ParentNo != node.AccNo && nodesMap.ContainsKey(node.ParentNo ?? string.Empty))
                {
                    nodesMap[node.ParentNo ?? string.Empty].Children.Add(nodesMap[node.AccNo]);
                }
            }

            return dtoList;
        }




        [HttpGet]
        public async Task<IActionResult> GetParentInfo(string accNo)
        {
            accNo = accNo.Trim();

            try
            {
                // Attempt to retrieve the data with a direct SQL query
                var parentInfo = await _context.RefChartOfAccs
                    .FromSqlRaw($"SELECT * FROM Ref_ChartOfAcc WHERE AccNo = '{accNo}'")
                    .FirstOrDefaultAsync();

                if (parentInfo != null && !string.IsNullOrEmpty(parentInfo.ParentNo?.Trim()))
                {
                    var result = new { accNo = accNo, ParentNo = parentInfo.ParentNo.Trim(), Description = parentInfo.Description?.Trim() };
                    return Json(result);
                }

                // Handle the case when the node is not found or ParentNo is null/empty
                return Json(new { accNo = accNo, ParentNo = "N/A", Description = "N/A" });
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine($"Error in GetParentInfo: {ex.Message}");
                return Json(new { accNo = accNo, ParentNo = "N/A", Description = "N/A" });
            }
        }



        public async Task<IActionResult> Create(string searchString, int pg = 1)
        {
            ViewData["RefAccountsTypes"] = new SelectList(_context.Set<RefAccountType>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.AccTypeName), "AccTypeID", "AccTypeName", "AccTypeID");

            var chartOfAccViewModelData = GetCOATreeviewData();

            var viewModel = new ChartOfAllAccViewModel
            {
                refChartOfAccsMulti = new List<RefChartOfAcc>(), // Initialize with an empty list
                ChartOfAccViewModelMulti = chartOfAccViewModelData
            };

            return View(viewModel);

        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccNo,ParentNo,AccName,AccCode,AccType,Description,Note,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,IsInactive,InactivatedBy,InactivatedDateTime,InactivatedReason")] RefChartOfAcc refChartOfAccs, string selectedNode)
        {
            var TableID = "Ref_ChartOfAcc";  // Table ID in the Ref_Last
            var refLastNumber = await _context.RefLastNumbers.FindAsync(TableID);

            if (refLastNumber != null)
            {
                var nextNumber = refLastNumber.LastNumber + 1;
                refLastNumber.LastNumber = nextNumber;
                var IDNumber = "ACC" + nextNumber.ToString().PadLeft(4, '0');
                refChartOfAccs.AccNo = IDNumber;
            }
            else
            {
                return View(new ChartOfAllAccViewModel());
            }

            refChartOfAccs.CreatedBy = "Admin";
            refChartOfAccs.CreatedDateTime = DateTime.Now;

            ModelState.Remove("AccNo");

            try
            {
                // Update ParentNo with the selected node
                refChartOfAccs.ParentNo = selectedNode;

                _context.Add(refChartOfAccs);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log or debug the exception
                Console.WriteLine(ex.Message);
                throw; // Rethrow the exception to see details in the browser
            }


            // Populate the refChartOfAccsMulti property of ChartOfAllAccViewModel
            var chartOfAllAccViewModel = new ChartOfAllAccViewModel
            {
                refChartOfAccsMulti = _context.RefChartOfAccs.ToList(), // Adjust this line based on your data retrieval logic
            };

            ViewData["RefAccountsTypes"] = new SelectList(_context.Set<RefAccountType>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.AccTypeName), "AccTypeID", "AccTypeName", "AccTypeID");
            return View(chartOfAllAccViewModel);
        }
        // GET: TxnInvoiceHds/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.RefChartOfAccs == null)
            {
                return NotFound();
            }

            var refChartOfAccs = await _context.RefChartOfAccs.FindAsync(id);
            if (refChartOfAccs == null)
            {
                return NotFound();
            }
            var AccNo = id;

            ViewData["RefAccountsTypes"] = new SelectList(_context.Set<RefAccountType>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.AccTypeName), "AccTypeID", "AccTypeName", "AccTypeID");
            return View(refChartOfAccs);
        }
        // POST: TxnInvoiceHds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("AccNo,ParentNo,AccName,AccCode,AccType,Description,Note,CreatedBy,CreatedDateTime,LastUpdatedBy,LastUpdatedDateTime,IsInactive,InactivatedBy,InactivatedDateTime,InactivatedReason")] RefChartOfAcc refChartOfAccs)
        {
            if (id != refChartOfAccs.AccNo)
            {
                return NotFound();
            }
            refChartOfAccs.LastUpdatedBy = "Admin";
            refChartOfAccs.LastUpdatedDateTime = DateTime.Now;

            if (ModelState.IsValid)
            {
                try
                {
               
                    _context.Update(refChartOfAccs);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!refChartOfAccsExists(refChartOfAccs.AccNo))
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
            ViewData["RefAccountsTypes"] = new SelectList(_context.Set<RefAccountType>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.AccTypeName), "AccTypeID", "AccTypeName", "AccTypeID");
            return View(refChartOfAccs);
        }





        // GET: TxnPaymentVoucherHds/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.RefChartOfAccs == null)
            {
                return NotFound();
            }

            var refChartOfAccs = await _context.RefChartOfAccs
                .FirstOrDefaultAsync(m => m.AccNo == id);
            if (refChartOfAccs == null)
            {
                return NotFound();
            }
            ViewData["RefAccountsTypes"] = new SelectList(_context.Set<RefAccountType>().Where(a => a.IsActive.Equals(true)).OrderBy(p => p.AccTypeName), "AccTypeID", "AccTypeName", "AccTypeID");
            return View(refChartOfAccs);
        }

        private bool refChartOfAccsExists(string id)
        {
          return (_context.RefChartOfAccs?.Any(e => e.AccNo == id)).GetValueOrDefault();
        }
    }
}
