using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models;

[PrimaryKey("AgentId")]
[Table("Ref_AgentAccOpt")]
public partial class RefAgentAccOpt
{
    [Key]
    [Column("AgentID")]
    [StringLength(20)]
    public string AgentId { get; set; } = null!;

    [Key]
    [Column("PortID")]
    [StringLength(20)]
    public string PortId { get; set; } = null!;

    [StringLength(300)]
    public string AgentName { get; set; } = null!;

    [StringLength(250)]
    public string? Address1 { get; set; }

    [StringLength(250)]
    public string? Address2 { get; set; }

    [StringLength(250)]
    public string? Address3 { get; set; }

    [StringLength(250)]
    public string? City { get; set; }

    [StringLength(200)]
    public string? County { get; set; }

    [StringLength(15)]
    public string? TelNo { get; set; }

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(350)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDateTime { get; set; }

    [StringLength(20)]
    public string? LastUpdatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedDateTime { get; set; }

    [ForeignKey("PortId")]
    [InverseProperty("RefAgents")]
    public virtual RefPortAccOpt Port { get; set; } = null!;

    [InverseProperty("AgentNavigation")] // from Accounts 
    public virtual ICollection<TxnDebitNoteAccHD> TxnDebitNoteAccHD { get; } = new List<TxnDebitNoteAccHD>();

    [InverseProperty("AgentNavigation")] // from Accounts 
    public virtual ICollection<TxnCreditNoteAccHD> TxnCreditNoteAccHD { get; } = new List<TxnCreditNoteAccHD>();

    [InverseProperty("RefAgentNavigation")] // from Accounts 
    public virtual ICollection<TxnPaymentHDs> TxnPaymentHDs { get; } = new List<TxnPaymentHDs>();

    [InverseProperty("RefAgentNavigation")] // from Accounts 
    public virtual ICollection<TxnReceiptHD> TxnReceiptHDs { get; } = new List<TxnReceiptHD>();


}
