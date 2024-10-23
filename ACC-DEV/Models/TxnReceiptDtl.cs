using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[Table("Txn_ReceiptDtl")]
public class TxnReceiptDtl
{
    [Key]
    [StringLength(50)]
    public string ReceiptNo { get; set; } = null!;

    [Key]
    [StringLength(50)]
    public string? RefNo { get; set; }

    [StringLength(50)]
    public string? RefType { get; set; } = null!;

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Amount { get; set; }

    [Required]
    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }
}