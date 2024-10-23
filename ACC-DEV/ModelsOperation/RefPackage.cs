using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.ModelsOperation;

[Table("Ref_Package")]
public partial class RefPackage
{
    [Key]
    [Column("PackageID")]
    [StringLength(20)]
    public string PackageId { get; set; } = null!;

    [StringLength(300)]
    public string Description { get; set; } = null!;

    [StringLength(80)]
    public string? CustomCode { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(1)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }

    [InverseProperty("PackageNavigation")]
    public virtual ICollection<TxnBookingExp> TxnBookingExps { get; } = new List<TxnBookingExp>();

    [InverseProperty("PackageTypeImportJobNavigation")]
    public virtual ICollection<TxnImportJobDtl> TxnImportJobDtls { get; } = new List<TxnImportJobDtl>();

}
