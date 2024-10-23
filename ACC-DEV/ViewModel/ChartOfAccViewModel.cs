using ACC_DEV.Models;

namespace ACC_DEV.ViewModel
{
    public class ChartOfAccViewModel
    {
        public string AccNo { get; set; }
        public string ParentNo { get; set; }
        public string Description { get; set; }  // Add this property
        public string AccName { get; set; }
        public List<ChartOfAccViewModel> Children { get; set; }
    }


    public class ChartOfAllAccViewModel
    {
        public IEnumerable<RefChartOfAcc> refChartOfAccsMulti { get; set; }
        public IEnumerable<ChartOfAccViewModel> ChartOfAccViewModelMulti { get; set; }

    }




}
