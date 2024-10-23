using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACC_DEV.Models;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models
{
    [Table("Txn_DebitNoteAccHD")]
    public class TxnDebitNoteAccHD
    {
        [Key]
        [StringLength(50)]
        public string DebitNoteNo { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public decimal ExchangeRate { get; set; }

        [StringLength(50)]
        public string Agent { get; set; }

        [StringLength(50)]
        public string DocType { get; set; }

        [StringLength(50)]
        public string JobNo { get; set; }

        [StringLength(100)]
        public string BLNo { get; set; }

        [StringLength(50)]
        public string MainAcc { get; set; }

        [StringLength(500)]
        public string Narration { get; set; }

        public decimal MainAccAmount { get; set; }

        public decimal TotalInvoiceAmountLKR { get; set; }

        public decimal TotalInvoiceAmountUSD { get; set; }

        [StringLength(450)]
        public string Remarks { get; set; }

        [StringLength(20)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDateTime { get; set; }

        [StringLength(20)]
        public string LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedDateTime { get; set; }

        public bool Canceled { get; set; }

        [StringLength(20)]
        public string CanceledBy { get; set; }

        public DateTime? CanceledDateTime { get; set; }

        [StringLength(350)]
        public string CanceledReason { get; set; }

        public bool Approved { get; set; }

        [StringLength(20)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDateTime { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal AmountToBePaid { get; set; }
    }
}
