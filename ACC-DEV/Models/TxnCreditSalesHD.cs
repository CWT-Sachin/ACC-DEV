using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACC_DEV.Models
{
    [Table("Txn_CreditSalesHD")]
    public partial class TxnCreditSalesHD
    {
        [Key]
        [StringLength(50)]
        public string CreditSalesNo { get; set; }

        public DateTime? Date { get; set; }

        [Column(TypeName = "numeric(18, 3)")]
        public decimal? ExchangeRate { get; set; }

        [StringLength(50)]
        public string Customer { get; set; }

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

        [Column(TypeName = "numeric(18, 2)")]
        public decimal? MainAccAmount { get; set; }

        [Column(TypeName = "numeric(18, 3)")]
        public decimal? TotalInvoiceAmountLKR { get; set; }

        [StringLength(450)]
        public string Remarks { get; set; }

        [StringLength(20)]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDateTime { get; set; }

        [StringLength(20)]
        public string LastUpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastUpdatedDateTime { get; set; }

        public bool? Canceled { get; set; }

        [StringLength(20)]
        public string CanceledBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CanceledDateTime { get; set; }

        [StringLength(350)]
        public string CanceledReason { get; set; }

        public bool? Approved { get; set; }

        [StringLength(20)]
        public string ApprovedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ApprovedDateTime { get; set; }

        [Column(TypeName = "numeric(18, 3)")]
        public decimal? AmountPaid { get; set; }

        [Column(TypeName = "numeric(18, 3)")]
        public decimal? AmountToBePaid { get; set; }
    }
}
