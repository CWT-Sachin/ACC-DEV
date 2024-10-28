﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.ModelsOperation;



namespace ACC_DEV.Models;

[Table("Txn_DebitNoteHD_Export")]
public partial class TxnDebitNoteExportHd
{
    [Key]
    [StringLength(20)]
    public string DebitNoteNo { get; set; } = null!;

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

    [Column("TotalDebitAmountLKR", TypeName = "numeric(18, 3)")]
    public decimal? TotalDebitAmountLkr { get; set; }

    [Column("TotalDebitAmountUSD", TypeName = "numeric(18, 3)")]
    public decimal? TotalDebitAmountUsd { get; set; }

    [StringLength(450)]
    public string? Remarks { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    public bool? Canceled { get; set; }

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

    [StringLength(20)]
    public string? NoteType { get; set; }



    [ForeignKey("AgentID")]
    [InverseProperty("TxnDebitNoteExportHd")]
    public virtual RefAgent? AgentDebitNoteExportNavigation { get; set; }

    [ForeignKey("Customer")] // Shippingline
    [InverseProperty("TxnDebitNoteExportHd")]
    public virtual RefShippingLine? ShippingLineDbitNoteExpNavigation { get; set; }
}
