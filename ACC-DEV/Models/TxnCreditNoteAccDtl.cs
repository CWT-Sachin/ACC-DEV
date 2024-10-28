using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ACC_DEV.Models;
using Microsoft.EntityFrameworkCore;

namespace ACC_DEV.Models
{
    [PrimaryKey("CreditNoteNo", "SerialNo")]
    [Table("Txn_CreditNoteAccDtl")]
    public class TxnCreditNoteAccDtl
    {
        [Key]
        [StringLength(50)]
        public string CreditNoteNo { get; set; }

        [Key]
        [StringLength(50)]
        public string SerialNo { get; set; }

        [StringLength(50)]
        public string LineAccNo { get; set; }

        [StringLength(50)]
        public string ChargeItem { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}
