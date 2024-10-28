﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACC_DEV.ModelsOperation;
using ACC_DEV.DataOperation;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.ModelsOperation;

[Table("Txn_PaymentVoucherHD_Import")]
public partial class TxnPaymentVoucherImportHd
{
    [Key]
    [StringLength(20)]
    public string PayVoucherNo { get; set; } = null!;

    [Column(TypeName = "date")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? Date { get; set; }

    [StringLength(20)]
    public string? JobNo { get; set; }

    [Column("Customer")]
    [StringLength(20)]
    public string? Customer { get; set; }

    [Column(TypeName = "numeric(18, 3)")]
    public decimal? ExchangeRate { get; set; }

    [StringLength(20)]
    public string? PayCurrency { get; set; }

    [Column("TotalPayVoucherAmountLKR", TypeName = "numeric(18, 3)")]
    public decimal? TotalPayVoucherAmountLkr { get; set; }

    [Column("TotalPayVoucherAmountUSD", TypeName = "numeric(18, 3)")]
    public decimal? TotalPayVoucherAmountUsd { get; set; }

    [StringLength(450)]
    public string? Remarks { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    public bool? Canceled { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(20)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }

    public bool? Approved { get; set; }

    [StringLength(20)]
    public string? ApprovedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApprovedDateTime { get; set; }


    [StringLength(50)]
    public string? DebitAcc { get; set; }

    [StringLength(50)]
    public string? CreditAcc { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    [StringLength(20)]
    public string? AgentID { get; set; }

    [Column("AmountPaid", TypeName = "numeric(18, 3)")]
    public decimal? AmountPaid { get; set; }

    [Column("AmountToBePaid", TypeName = "numeric(18, 3)")]
    public decimal? AmountToBePaid { get; set; }

    //[ForeignKey("AgentID")]
    //[InverseProperty("TxnPaymentVoucherImportHd")]
    //public virtual RefAgent? AgentPaymentVoucherImportNavigation { get; set; }

    //[ForeignKey("Customer")] // Shippingline
    //[InverseProperty("TxnPaymentVoucherImportHd")]
    //public virtual RefShippingLine? ShippingLinePaymentVoucherImpNavigation { get; set; }

}
