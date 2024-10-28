using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
//using ACC_DEV.ModelsOperation;
using ACC_DEV.Models;

namespace ACC_DEV.Models;

[Table("Ref_ChargeItem")]
public partial class RefChargeItem
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



    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnCreditNoteDtl> TxnCreditNoteDtls { get; } = new List<TxnCreditNoteDtl>();

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnDebitNoteDtl> TxnDebitNoteDtls { get; } = new List<TxnDebitNoteDtl>();

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnInvoiceDtl> TxnInvoiceDtls { get; } = new List<TxnInvoiceDtl>();

    //[InverseProperty("ChargeItemNavigation")]
    //public virtual ICollection<TxnInvoiceImportDtl> TxnInvoiceImportDtls { get; } = new List<TxnInvoiceImportDtl>();

    [InverseProperty("ChargeItemNavigation")]
    public virtual ICollection<TxnPaymentVoucherDtl> TxnPaymentVoucherDtls { get; } = new List<TxnPaymentVoucherDtl>();

    ///  Charge Items
    [ForeignKey("AccNo_Revenue_Imp")]
    [InverseProperty("AccNoRevenueImp")]
    public virtual RefChartOfAcc AccNoRevenueImpNavigation { get; set; }

    [ForeignKey("AccNo_Revenue_Exp")]
    [InverseProperty("AccNoRevenueExp")]
    public virtual RefChartOfAcc AccNoRevenueExpNavigation { get; set; }

    [ForeignKey("AccNo_Expense_Imp_Liner")]
    [InverseProperty("AccNoExpenseImpLiner")]
    public virtual RefChartOfAcc AccNoExpenseImpLinerNavigation { get; set; }

    [ForeignKey("AccNo_Expense_Imp_Agent")]
    [InverseProperty("AccNoExpenseImpAgent")]
    public virtual RefChartOfAcc AccNoExpenseImpAgentNavigation { get; set; }

    [ForeignKey("AccNo_Expense_Exp_Liner")]
    [InverseProperty("AccNoExpenseExpLiner")]
    public virtual RefChartOfAcc AccNoExpenseExpLinerNavigation { get; set; }

    [ForeignKey("AccNo_Expense_Exp_Agent")]
    [InverseProperty("AccNoExpenseExpAgent")]
    public virtual RefChartOfAcc AccNoExpenseExpAgentNavigation { get; set; }
}
