using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;

namespace ACC_DEV.ViewModel
{
    public class CreditNoteViewModel
    {
        public IEnumerable<TxnCreditNoteHd> CreditNoteHdMulti { get; set; }
        public IEnumerable<TxnCreditNoteDtl> CreditNoteDtlMulti { get; set; }

        public IEnumerable<TxnImportJobHd> ImportJobHdMulti { get; set; }
        public IEnumerable<TxnImportJobDtl> ImportJobDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }

    }
}
