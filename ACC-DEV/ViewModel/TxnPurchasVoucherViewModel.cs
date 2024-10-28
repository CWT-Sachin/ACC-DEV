using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;

//using ACC_DEV.DataOperation;
using ACC_DEV.Data;

namespace ACC_DEV.ViewModel
{

    public class TxnPurchasVoucherViewModel
    {


        
        public IEnumerable<TxnPurchasVoucherHD> TxnPurchasVoucherHDMulti { get; set; }
        public IEnumerable<TxnPurchasVoucherDtl> TxnPurchasVoucherDtlMulti { get; set; }

        public IEnumerable<TxnImportJobHd> ImportJobHdMulti { get; set; }
        public IEnumerable<TxnImportJobDtl> ImportJobDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }
        public string ContainerNo { get; set; }

    }

    public class SelectedPurchasItem
    {
        //public string? SerialNo { get; set; }

        public decimal? SerialNo { get; set; }
        public string? LineAccNo { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; } // Change the type to decimal?
    }

}
