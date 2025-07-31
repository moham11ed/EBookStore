namespace BookShoppingCartMvcUI.Models.DTOS
{
    public class StockDisplayModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public string? BookName { get; set; }
    }
}
