using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[PrimaryKey("JournalNo", "SerialNo")]
[Table("Txn_JournalDtl")]
public class TxnJournalDtl
{
    [Key]
    [StringLength(50)]
    public string JournalNo { get; set; } = null!;

    [Key]
    [Column(TypeName = "numeric(6, 0)")]
    public decimal SerialNo { get; set; }

    [StringLength(50)]
    public string? AccNo { get; set; }

    [StringLength(50)]
    public string? Description { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Debit { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Credit { get; set; }
}
