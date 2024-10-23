using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.ModelsOperation;
using ACC_DEV.Models;

namespace ACC_DEV.Models;

[Table("Ref_ChargeItem")]
public partial class RefChargeItem
{
    [Key]
    [Column("ChargeID")]
    [StringLength(20)]
    public string ChargeId { get; set; } = null!;

    [StringLength(300)]
    public string Description { get; set; } = null!;

    public bool? IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(1)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }

    [StringLength(50)]
    public string? AccNo { get; set; }

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnCreditNoteDtl> TxnCreditNoteDtls { get; } = new List<TxnCreditNoteDtl>();

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnDebitNoteDtl> TxnDebitNoteDtls { get; } = new List<TxnDebitNoteDtl>();

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnInvoiceDtl> TxnInvoiceDtls { get; } = new List<TxnInvoiceDtl>();

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnInvoiceImportDtl> TxnInvoiceImportDtls { get; } = new List<TxnInvoiceImportDtl>();

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnPaymentVoucherDtl> TxnPaymentVoucherDtls { get; } = new List<TxnPaymentVoucherDtl>();
}
