using ACC_DEV.Models;
using ACC_DEV.ModelsOperation;
using System.Collections.Generic;

using ACC_DEV.DataOperation;
using ACC_DEV.Data;

namespace ACC_DEV.ViewModel
{

    public class InvoiceViewModel
    {


        
        public IEnumerable< TxnInvoiceHd> InvoiceHdMulti { get; set; }
        public IEnumerable<TxnInvoiceDtl> InvoiceDtMulti { get; set; }

        public IEnumerable<TxnImportJobHd> ImportJobHdMulti { get; set; }
        public IEnumerable<TxnImportJobDtl> ImportJobDtlMulti { get; set; }

        public IEnumerable<TxnExportJobHD> ExportJobHdMulti { get; set; }
        public IEnumerable<TxnExportJobDtl> ExportJobDtlMulti { get; set; }
        public string ContainerNo { get; set; }

    }

}
