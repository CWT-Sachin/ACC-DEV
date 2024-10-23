﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[PrimaryKey("DebitNoteNo", "SerialNo")]
[Table("Txn_DebitNoteDtl")]
public partial class TxnDebitNoteDtl
{
    [Key]
    [StringLength(20)]
    public string DebitNoteNo { get; set; } = null!;

    [Key]
    [Column(TypeName = "numeric(6, 0)")]
    public decimal SerialNo { get; set; }

    [StringLength(100)]
    public string? BlContainerNo { get; set; }

    [StringLength(20)]
    public string? ChargeItem { get; set; }

    [StringLength(50)]
    public string? AccNo { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; }

    [Column(TypeName = "numeric(18, 3)")]
    public decimal? Rate { get; set; }

    [StringLength(20)]
    public string? Currency { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Qty { get; set; }

    [Column(TypeName = "numeric(18, 3)")]
    public decimal? Amount { get; set; }

    [ForeignKey("ChargeItem")]
    [InverseProperty("TxnDebitNoteDtls")]
    public virtual RefChargeItem? ChargeItemNavigation { get; set; }
}