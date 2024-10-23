using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.ModelsOperation;
using PowerAcc.Models;

namespace ACC_DEV.Models;

[Table("Txn_PurchasVoucherHD")]
public partial class TxnPurchasVoucherHD
{
    [Key]
    [StringLength(50)]
    public string PurchasVoucherNo { get; set; } = null!;

    [Column(TypeName = "date")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? Date { get; set; }

    [Column(TypeName = "numeric(18, 3)")]
    public decimal? ExchangeRate { get; set; }

    [StringLength(50)]
    public string CreditorType { get; set; } = null!;

    [StringLength(50)]
    public string? Supplier { get; set; }

    [StringLength(50)]
    public string? ShippingLine { get; set; }

    [StringLength(50)]
    public string? DocType { get; set; }

    [StringLength(20)]
    public string? JobNo { get; set; }


    [Column("BLNo")]
    [StringLength(100)]
    public string? Blno { get; set; }


    [StringLength(50)]
    public string? MainAcc { get; set; }

    [StringLength(500)]
    public string? Narration { get; set; }

    [Column("MainAccAmount", TypeName = "numeric(18, 2)")]
    public decimal? MainAccAmount { get; set; }

    [Column("TotalAmountLKR", TypeName = "numeric(18, 2)")]
    public decimal? TotalAmountLKR { get; set; }

    [StringLength(450)]
    public string? Remarks { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

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
    public string? CanceledReson { get; set; }

    public bool? Approved { get; set; }

    [StringLength(20)]
    public string? ApprovedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApprovedDateTime { get; set; }

    [Column("AmountPaid", TypeName = "numeric(18, 2)")]
    public decimal? AmountPaid { get; set; }

    [Column("AmountToBePaid", TypeName = "numeric(18, 2)")]
    public decimal? AmountToBePaid { get; set; }

    [StringLength(500)]
    public string? TotAmtWord { get; set; }

    [StringLength(50)]
    public string? JobType { get; set; }

    [ForeignKey("Supplier")] // Assuming this is the correct foreign key property
    [InverseProperty("TxnPurchasVoucherHD")]
    public virtual RefSupplier? SupplierNavigation { get; set; }

    [ForeignKey("ShippingLine")] // Assuming this is the correct foreign key property
    [InverseProperty("TxnPurchasVoucherHD")]
    public virtual RefShippingLineAccOpt? ShippingLineNavigation { get; set; }

}
