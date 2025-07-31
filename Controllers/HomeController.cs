using System.Diagnostics;
using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Models.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _logger = logger;
            _homeRepository = homeRepository;
        }

        public async Task<IActionResult> Index(string sterm ="", int genreId = 0)
        {
           
            IEnumerable<Book> books = await _homeRepository.GetBooks(sterm,genreId);


            IEnumerable<Genre> geners = await _homeRepository.Genres();
            BookDisplayModel BookModel = new BookDisplayModel
            {
                Books = books,
                Genres = geners,
                STerm = sterm,
                GenreId= genreId,

        
            };


            return View(BookModel);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
