using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACC_DEV.ViewModel
{

    public class DebitNoteAccViewModel
    {

        public IEnumerable<TxnDebitNoteAccHD> TxnDebitNoteAccHDMulti { get; set; }
        public IEnumerable<TxnDebitNoteAccDtl> TxnDebitNoteAccDtlMulti { get; set; }


        public IEnumerable<TxnDebitNoteHd> DebitNoteHdMulti { get; set; }
        public IEnumerable<TxnDebitNoteDtl> DebitNoteDtlMulti { get; set; }
    }

}
