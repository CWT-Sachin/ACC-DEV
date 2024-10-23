using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACC_DEV.Models
{
    [Table("Txn_PettyCashHD")]
    public partial class TxnPettyCashHD
    {
        [Key]
        [StringLength(50)]
        public string PettyCashNo { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [StringLength(50)]
        public string Supplier { get; set; }

        [StringLength(50)]
        public string DocType { get; set; }

        [StringLength(50)]
        public string? VoucherNo { get; set; }

        [StringLength(50)]
        public string? VoucherType { get; set; }

        [StringLength(50)]
        public string MainAcc { get; set; }

        [StringLength(500)]
        public string? Narration { get; set; }

        [Column(TypeName = "numeric(18, 2)")]
        public decimal MainAccAmount { get; set; }

        [Column("TotalAmountLKR", TypeName = "numeric(18, 2)")]
        public decimal TotalAmountLKR { get; set; }

        [StringLength(450)]
        public string? Remarks { get; set; }

        [Column(TypeName = "numeric(18, 3)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "numeric(18, 3)")]
        public decimal AmountToBePaid { get; set; }

        [StringLength(500)]
        public string TotAmtWord { get; set; }

        [StringLength(20)]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CreatedDateTime { get; set; }

        [StringLength(20)]
        public string? LastUpdatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? LastUpdatedDateTime { get; set; }

        public bool? Canceled { get; set; }

        [StringLength(20)]
        public string? CanceledBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? CanceledDateTime { get; set; }

        [StringLength(350)]
        public string? CanceledReason { get; set; }

        public bool? Approved { get; set; }

        [StringLength(20)]
        public string? ApprovedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ApprovedDateTime { get; set; }

        [StringLength(250)]
        public string? CreditorName { get; set; }

        [ForeignKey("Supplier")] // Assuming this is the correct foreign key property
        [InverseProperty("TxnPettyCashHD")]
        public virtual RefSupplier? SupplierPettyNavigation { get; set; }
    }
}
