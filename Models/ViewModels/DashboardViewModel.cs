namespace RestoApp.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string totalSale { get; set; }
        public string totalSaleDetail { get; set; }
        public string totalSalePer { get; set; }
        public bool isSaleIncreased { get; set; }

        public string totalItemSale { get; set; }
        public string totalItemSaleDetail { get; set; }
        public string totalItemSalePer { get; set; }
        public bool isItemSaleIncreased { get; set; }

        public string totalNetProfit { get; set; }
        public string totalNetDetail { get; set; }
        public string totalNetPer { get; set; }
        public bool isNetProfitIncreased { get; set; }

        public string totalCustomer { get; set; }
        public string totalCustomerDetail { get; set; }
        public string totalCustomerPer { get; set; }
        public bool isCustomerIncreased { get; set; }
    }
}
