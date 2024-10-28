using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[PrimaryKey("ReceiptNo", "DocNo")]
[Table("Txn_ReceiptDtl")]
public class TxnReceiptDtl
{
    [Key]
    [StringLength(50)]
    public string ReceiptNo { get; set; } = null!;

    [Key]
    [StringLength(50)]
    public string? DocNo { get; set; }

    [StringLength(50)]
    public string? DocType { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(50)]
    public string? JobNo { get; set; }

    [StringLength(50)]
    public string? JobType { get; set; }
}