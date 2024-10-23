using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[Table("Txn_JournalHD")]
public class TxnJournalHd
{
    [Key]
    [StringLength(50)]
    public string? JournalNo { get; set; }

    [Column(TypeName = "date")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? Date { get; set; }

    [StringLength(100)]
    public string? RefNo { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public bool IsAdjustmentEntry { get; set; }

    public bool IsJobRelated { get; set; }

    [StringLength(100)]
    public string? JobType { get; set; }

    [StringLength(50)]
    public string? JobNo { get; set; }

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

    [StringLength(500)]
    public string? CanceledReason { get; set; }
}
