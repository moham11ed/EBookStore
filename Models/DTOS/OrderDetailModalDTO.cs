namespace BookShoppingCartMvcUI.Models.DTOS
{
    public class OrderDetailModalDTO
    {
        public string DivId { get; set; }
        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}
