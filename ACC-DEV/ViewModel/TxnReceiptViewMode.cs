using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;
using ACC_DEV.DataOperation;
using ACC_DEV.Data;

namespace ACC_DEV.ViewModel
{

    public class TxnReceiptViewMode
    {

        public IEnumerable<TxnReceiptHD> TxnReceiptHdMulti { get; set; }
        public IEnumerable<TxnReceiptDtl> TxnReceiptDtMulti { get; set; }

        public IEnumerable< TxnInvoiceHd> InvoiceHdMulti { get; set; }
        public IEnumerable<TxnInvoiceDtl> InvoiceDtMulti { get; set; }

        public IEnumerable<TxnImportJobHd> ImportJobHdMulti { get; set; }
        public IEnumerable<TxnImportJobDtl> ImportJobDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }

        public IEnumerable<TxnDebitNoteHd> DebitNoteHdMulti { get; set; }
        public IEnumerable<TxnDebitNoteDtl> DebitNoteDtlMulti { get; set; }

    }

    public class SelectedReceiptItem
    {
        public string? RefNo { get; set; }
        public string? RefType { get; set; }
        public decimal Amount { get; set; }
    }

}
