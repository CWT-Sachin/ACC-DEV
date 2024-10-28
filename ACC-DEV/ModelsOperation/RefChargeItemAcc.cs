using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
//using ACC_DEV.ModelsOperation;
using ACC_DEV.Models;

namespace ACC_DEV.Models;

[Table("Ref_ChargeItemAcc")]
public partial class RefChargeItemAcc
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

    [StringLength(50)]
    public string? AccNo_Revenue_Exp { get; set; }

    [StringLength(50)]
    public string? AccNo_Revenue_Imp { get; set; }

    [StringLength(50)]
    public string? AccNo_Expense_Imp_Liner { get; set; }

    [StringLength(50)]
    public string? AccNo_Expense_Imp_Agent { get; set; }

    [StringLength(50)]
    public string? AccNo_Expense_Exp_Liner { get; set; }
    [StringLength(50)]
    public string? AccNo_Expense_Exp_Agent { get; set; }



}
