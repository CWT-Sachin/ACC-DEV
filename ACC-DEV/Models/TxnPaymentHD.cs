﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACC_DEV.Models;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models
{
    [Table("Txn_PaymentHD")]
    public partial class TxnPaymentHDs
    {
            [Key]
            [StringLength(50)]
            public string? PaymentNo { get; set; }

            public DateTime? Date { get; set; }

            [StringLength(50)]
            public string? PaymentNoType { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? ExchangeRate { get; set; }

            [StringLength(50)]
            public string? CustomerID { get; set; }

            [StringLength(100)]
            public string? ChequeNo { get; set; }

            public DateTime? ChequeDate { get; set; }

            [StringLength(50)]
            public string? ChequeBankID { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? ChequeAmount { get; set; }

            [StringLength(500)]
            public string? Remarks { get; set; }

            [Column(TypeName = "numeric(18, 2)")]
            public decimal? PaymentTotalAmt { get; set; }

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

            public bool IsAcPayeeOnly { get; set; }

            [StringLength(500)]
            public string? TotAmtWord { get; set; }

            [StringLength(50)]
            public string? AgentID { get; set; }

            [StringLength(50)]
            public string? ShippingLineID { get; set; }

            [StringLength(50)]
            public string? PaymentVouchType { get; set; }

            [StringLength(50)]
            public string? CustomerType { get; set; }


            [ForeignKey("ChequeBankID")]
            [InverseProperty("TxnPaymentHDs")]
            public virtual Ref_BankAcc? RefBankPayNavigation { get; set; }
        }
}