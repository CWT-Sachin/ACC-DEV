using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ACC_DEV.ViewModel
{

    public class TxnCreditNoteAccViewModel
    {

        public IEnumerable<TxnCreditNoteAccHD> TxnCreditNoteAccHDMulti { get; set; }
        public IEnumerable<TxnCreditNoteAccDtl> TxnCreditNoteAccDtlMulti { get; set; }


    }

    public class SelectedCreditNoteAccItem
    {
        //public string? SerialNo { get; set; }

        public string? SerialNo { get; set; }
        public string? LineAccNo { get; set; }
        public string? ChargeItem { get; set; }

        public string? Description { get; set; }
        public decimal? Amount { get; set; } // Change the type to decimal?
    }
}
