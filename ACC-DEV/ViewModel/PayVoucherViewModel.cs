using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;

namespace ACC_DEV.ViewModel
{
    public class PayVoucherViewModel
    {
        public IEnumerable<TxnPaymentVoucherHd> PayVoucherHdMulti { get; set; }
        public IEnumerable<TxnPaymentVoucherDtl> PayVoucherDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }

    }
}
