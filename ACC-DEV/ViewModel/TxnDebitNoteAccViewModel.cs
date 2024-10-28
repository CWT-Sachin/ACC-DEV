using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACC_DEV.ViewModel
{

    public class TxnDebitNoteAccViewModel
    {

        public IEnumerable<TxnDebitNoteAccHD> TxnDebitNoteAccHDMulti { get; set; }
        public IEnumerable<TxnDebitNoteAccDtl> TxnDebitNoteAccDtlMulti { get; set; }


        public IEnumerable<TxnDebitNoteHd> DebitNoteHdMulti { get; set; }
        public IEnumerable<TxnDebitNoteDtl> DebitNoteDtlMulti { get; set; }
    }

    public class SelectedDebitNoteAccItem
    {
        //public string? SerialNo { get; set; }

        public string? SerialNo { get; set; }
        public string? LineAccNo { get; set; }
        public string? ChargeItem { get; set; }

        public string? Description { get; set; }
        public decimal? Amount { get; set; } // Change the type to decimal?
    }
}
