using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.ModelsOperation;

[PrimaryKey("JobNo", "RefNo", "Id")]
[Table("Txn_ImportJob_Cnt")]
public partial class TxnImportJobCnt
{
    [Key]
    [StringLength(20)]
    public string JobNo { get; set; } = null!;

    [Key]
    [StringLength(20)]
    public string RefNo { get; set; } = null!;

    [Key]
    [StringLength(20)]
    public string Id { get; set; } = null!;

    
    [StringLength(20)]
    public string ContainerNo { get; set; } 

    [StringLength(20)]
    public string? Size { get; set; }

    [StringLength(20)]
    public string? SealNo { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? Weight { get; set; }

    [Column("CBM", TypeName = "numeric(18, 4)")]
    public decimal? Cbm { get; set; }

    [Column(TypeName = "numeric(18, 0)")]
    public decimal? NoOfPackages { get; set; }

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
    public string? CanceledReason { get; set; }

    [ForeignKey("Size")]
    [InverseProperty("TxnImportJobCnts")]
    public virtual RefContainerSize? SizeNavigation { get; set; }
}
