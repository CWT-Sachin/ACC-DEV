using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACC_DEV.Models;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models
{
    [Table("Txn_CreditNoteAccHD")]
    public class TxnCreditNoteAccHD
    {
        [Key]
        [StringLength(50)]
        public string CreditNoteNo { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal ExchangeRate { get; set; }

        [Required]
        [StringLength(50)]
        public string Agent { get; set; }

        [Required]
        [StringLength(50)]
        public string DocType { get; set; }

        [Required]
        [StringLength(50)]
        public string JobNo { get; set; }

        [Required]
        [StringLength(100)]
        public string BLNo { get; set; }

        [Required]
        [StringLength(50)]
        public string MainAcc { get; set; }

        [StringLength(500)]
        public string Narration { get; set; }

        [Required]
        public decimal MainAccAmount { get; set; }

        [Required]
        public decimal TotalInvoiceAmountLKR { get; set; }

        [Required]
        public decimal TotalInvoiceAmountUSD { get; set; }

        [StringLength(450)]
        public string Remarks { get; set; }

        [Required]
        [StringLength(20)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }

        [StringLength(20)]
        public string LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedDateTime { get; set; }

        [Required]
        public bool Canceled { get; set; }

        [StringLength(20)]
        public string CanceledBy { get; set; }

        public DateTime? CanceledDateTime { get; set; }

        [StringLength(350)]
        public string CanceledReason { get; set; }

        [Required]
        public bool Approved { get; set; }

        [StringLength(20)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDateTime { get; set; }

        public decimal? AmountPaid { get; set; }

        public decimal? AmountToBePaid { get; set; }
    }
}
