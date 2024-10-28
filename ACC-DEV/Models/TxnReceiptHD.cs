using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using ACC_DEV.ModelsOperation;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[Table("Txn_ReceiptHD")]
public class TxnReceiptHD
{
    [Key]
    [StringLength(50)]
    public string ReceiptNo { get; set; }

    [Column(TypeName = "date")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? Date { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? ExchangeRate { get; set; }

    [StringLength(50)]
    public string? CustomerID { get; set; }

    [StringLength(50)]
    public string? AgentID { get; set; }

    [StringLength(100)]
    public string? ChequeNo { get; set; }

    [Column(TypeName = "date")]
    public DateTime? ChequeDate { get; set; }

    [StringLength(50)]
    public string? ChequeBankID { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? ChequeAmount { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? ReceiptTotalAmt { get; set; }

    [StringLength(50)]
    public string? DebitAcc { get; set; }

    [StringLength(50)]
    public string? CreditAcc { get; set; }

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

    [StringLength(250)]
    public string? OtherAmountDescr { get; set; }

    [Column(TypeName = "numeric(18, 2)")]
    public decimal? OtherAmount { get; set; }

    [StringLength(500)]
    public string? TotAmtWord { get; set; }

    [StringLength(50)]
    public string? CustomerType { get; set; }

    public bool Approved { get; set; }

    [StringLength(20)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedDateTime { get; set; }

    [StringLength(50)]
    public string? Narration { get; set; }

    [ForeignKey("AgentID")] // Assuming this is the correct foreign key property
    [InverseProperty("TxnReceiptHDs")]
    public virtual RefAgentAccOpt? RefAgentNavigation { get; set; }

    [ForeignKey("CustomerID")] // Assuming this is the correct foreign key property
    [InverseProperty("TxnReceiptHDs")]
    public virtual RefCustomerAccOpt? RefCustomerNavigation { get; set; }

    //[ForeignKey("ChequeBankID")]
    //[InverseProperty("TxnReceiptHDs")]
    //public virtual RefBankAcc? RefBankNavigation { get; set; }
}
