using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;

namespace ACC_DEV.ViewModel
{
    public class DebitNoteViewModel
    {
        public IEnumerable<TxnDebitNoteHd> DebitNoteHdMulti { get; set; }
        public IEnumerable<TxnDebitNoteDtl> DebitNoteDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }

    }
}
