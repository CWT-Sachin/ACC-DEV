using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;

//using ACC_DEV.DataOperation;
using ACC_DEV.Data;

namespace ACC_DEV.ViewModel
{

    public class TxnPettyCashViewModel
    {


        
        public IEnumerable<TxnPettyCashHD> TxnPettyCashHDMulti { get; set; }
        public IEnumerable<TxnPettyCashDtl> TxnPettyCashDtlMulti { get; set; }

      }

    public class SelectedPettyCashItem
    {
        //public string? SerialNo { get; set; }

        public string? SerialNo { get; set; }
        public string? LineAccNo { get; set; }
        public string? Description { get; set; }

        public decimal? Amount { get; set; } // Change the type to decimal?
    }

}
