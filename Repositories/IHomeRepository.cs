namespace BookShoppingCartMvcUI
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Book>> GetBooks(string sTrem = "A", int genreId = 0);
        Task<IEnumerable<Genre>> Genres();
    }
}