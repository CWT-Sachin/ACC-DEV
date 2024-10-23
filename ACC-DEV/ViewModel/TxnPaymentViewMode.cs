using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACC_DEV.ViewModel
{

    public class TxnPaymentViewMode
    {

        public IEnumerable<TxnPaymentHDs> TxnPaymentHdMulti { get; set; }
        public IEnumerable<TxnPaymentDtl> TxnPaymentDtMulti { get; set; }

        public IEnumerable<TxnImportJobHd> ImportJobHdMulti { get; set; }
        public IEnumerable<TxnImportJobDtl> ImportJobDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }

        public IEnumerable<SelectedPaymentVocher> PayVoucherMulti { get; set; }

    }



    public class SelectedPaymentVocher
    {
        public string PayVoucherNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }

        public decimal? TotalPayVoucherAmountLkr { get; set; }

        public decimal? AmountPaid { get; set; }

        public decimal? AmountToBePaid { get; set; }
    }


    public class SelectedPaymentItem
    {
        public string? RefNo { get; set; }
        public string? RefType { get; set; }
        public decimal? Amount { get; set; } // Change the type to decimal?
    }

}
