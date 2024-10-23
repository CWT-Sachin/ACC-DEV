using ACC_DEV.Models;

namespace ACC_DEV.ViewModel
{
    public class AccountsViewModel
    {

        public IEnumerable<TxnJournalDtl> TxnJournalDtlMulti { get; set; }
        public IEnumerable<TxnJournalHd> TxnJournalHdMulti { get; set; }

    }

}
