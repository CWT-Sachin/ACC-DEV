using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ACC_DEV.Models;
using ACC_DEV.ViewModel;
using Newtonsoft.Json;
using ACC_DEV.Data;
using ACC_DEV.DataOperation;
using PowerAcc.Models;



namespace ACC_DEV.Data;

public partial class FtlcolomboAccountsContext : DbContext
{
    public FtlcolomboAccountsContext()
    {
    }

    public FtlcolomboAccountsContext(DbContextOptions<FtlcolomboAccountsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RefChartOfAcc> RefChartOfAccs { get; set; }
    public virtual DbSet<RefChargeItem> RefChargeItems { get; set; }

    public virtual DbSet<TxnJournalHd> TxnJournalHD { get; set; }

    public virtual DbSet<TxnJournalDtl> TxnJournalsDTL { get; set; }

    public virtual DbSet<RefAccountType> AccountTypes { get; set; }

    public virtual DbSet<RefSupplier> RefSuppliers { get; set; }

    public virtual DbSet<RefLastNumber> RefLastNumbers { get; set; }

    public virtual DbSet<TxnPettyCashHD> TxnPettyCashHDs { get; set; }

    public virtual DbSet<TxnPettyCashDtl> TxnPettyCashDtls { get; set; }

    //public virtual DbSet<RefLastNumber> RefLastNumbers { get; set; }

    public virtual DbSet<TxnInvoiceDtl> TxnInvoiceDtls { get; set; }

    public virtual DbSet<TxnInvoiceHd> TxnInvoiceHds { get; set; }

    //public virtual DbSet<TxnInvoiceImportDtl> TxnInvoiceImportDtls { get; set; }

    //public virtual DbSet<TxnInvoiceImportHd> TxnInvoiceImportHds { get; set; }

    public virtual DbSet<TxnPaymentVoucherDtl> TxnPaymentVoucherDtls { get; set; }

    public virtual DbSet<TxnPaymentVoucherHd> TxnPaymentVoucherHds { get; set; }

    public virtual DbSet<TxnCreditNoteDtl> TxnCreditNoteDtls { get; set; }

    public virtual DbSet<TxnCreditNoteHd> TxnCreditNoteHds { get; set; }

    public virtual DbSet<TxnDebitNoteDtl> TxnDebitNoteDtls { get; set; }

    public virtual DbSet<TxnDebitNoteHd> TxnDebitNoteHds { get; set; }

    public virtual DbSet<Ref_BankAcc> RefBanks { get; set; }

    public virtual DbSet<TxnReceiptDtl> TxnReceiptDtls { get; set; }

    public virtual DbSet<TxnReceiptHD> TxnReceiptHDs { get; set; }

    public virtual DbSet<TxnPaymentDtl> TxnPaymentDtls { get; set; }

    public virtual DbSet<TxnPaymentHDs> TxnPaymentHDs { get; set; }
    public virtual DbSet<TxnTransactions> TxnTransactions { get; set; }

    public virtual DbSet<TxnPurchasVoucherHD> TxnPurchasVoucherHDs { get; set; }
    public virtual DbSet<TxnPurchasVoucherDtl> TxnPurchasVoucherDtls { get; set; }

    public virtual DbSet<RefShippingLineAccOpt> RefShippingLineAccOpts { get; set; }




    public virtual DbSet<TxnDebitNoteAccDtl> TxnDebitNoteAccDtls { get; set; }

    public virtual DbSet<TxnDebitNoteAccHD> TxnDebitNoteAccHDs { get; set; }



    public virtual DbSet<TxnCreditNoteAccDtl> TxnCreditNoteAccDtls { get; set; }

    public virtual DbSet<TxnCreditNoteAccHD> TxnCreditNoteAccHDs { get; set; }













    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=constring");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TxnPaymentDtl>()
.HasKey(e => new { e.PaymentNo, e.RefNo });

        modelBuilder.Entity<TxnInvoiceHd>(entity =>
        {
            entity.HasOne(d => d.InvoiceHdAcc).WithMany(p => p.TxnInvoiceHds).HasConstraintName("FK_Txn_InvoiceHD_Export_Ref_Customer");
        });
        modelBuilder.Entity<TxnReceiptHD>(entity =>
        {
            entity.HasOne(d => d.RefBankNavigation).WithMany(p => p.TxnReceiptHDs).HasConstraintName("FK_Txn_ReceiptHD_Ref_Bank");
        });
        modelBuilder.Entity<TxnPaymentHDs>(entity =>
        {
            entity.HasOne(d => d.RefBankPayNavigation).WithMany(p => p.TxnPaymentHDs).HasConstraintName("FK_Txn_PaymentHD_Ref_Bank");
        });

        modelBuilder.Entity<TxnCreditNoteAccDtl>()
            .HasKey(e => new { e.CreditNoteNo, e.SerialNo });

        modelBuilder.Entity<TxnDebitNoteAccDtl>()
           .HasKey(e => new { e.DebitNoteNo, e.SerialNo });

        modelBuilder.Entity<TxnReceiptDtl>()
        .HasKey(e => new { e.ReceiptNo, e.RefNo });


        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
