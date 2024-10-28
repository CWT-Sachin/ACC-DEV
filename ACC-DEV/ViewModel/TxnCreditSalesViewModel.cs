using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;

namespace ACC_DEV.ViewModel
{
    public class TxnCreditSalesViewModel
    {
        public IEnumerable<TxnCreditSalesHD> TxnCreditSalesHDMulti { get; set; }
        public IEnumerable<TxnCreditSalesDtl> TxnCreditSalesDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }

    }

    public class SelectedCreditSalesItem
    {
        //public string? SerialNo { get; set; }

        public string? SerialNo { get; set; }
        public string? LineAccNo { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; } // Change the type to decimal?
    }
}
