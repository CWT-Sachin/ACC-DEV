using ACC_DEV.Models;
//using ACC_DEV.ModelsOperation;
using System.Collections.Generic;
//using ACC_DEV.DataOperation;
using ACC_DEV.Data;
using System.ComponentModel.DataAnnotations;

namespace ACC_DEV.ViewModel
{

    public class TxnReceiptViewModel
    {

        public IEnumerable<TxnReceiptHD> TxnReceiptHdMulti { get; set; }
        public IEnumerable<TxnReceiptDtl> TxnReceiptDtMulti { get; set; }
        public IEnumerable<SelectedReceiptDoc> ReceiptDocMulti { get; set; }
        public IEnumerable<SelectedReceiptDtlTableForEdit> ReceiptDtlTableMulti { get; set; }





    }

    public class SelectedReceiptDoc
    {
        public string DocNo { get; set; }
        public string DocType { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }

        public decimal? TotalAmountLkr { get; set; }

        public decimal? AmountPaid { get; set; }

        public decimal? AmountToBePaid { get; set; }
        public string JobNo { get; set; }
        public string JobType { get; set; }


    }
    public class SelectedReceiptDtlTableForEdit
    {
        public string DocNo { get; set; }
        public string DocType { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }

        public decimal? TotalAmountLkr { get; set; }

        public decimal? AmountPaid { get; set; }
       
        public decimal? AmountToBePaid { get; set; }
        public decimal? Amount { get; set; }

        public string JobNo { get; set; }
        public string JobType { get; set; }


    }

    public class SelectedRecieptPaymentItem
    {
        public string? DocNo { get; set; }
        public string? DocType { get; set; }
        public decimal? Amount { get; set; } // Change the type to decimal?
        public string JobNo { get; set; }
        public string JobType { get; set; }
    }



}
