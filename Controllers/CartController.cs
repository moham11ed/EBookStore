using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository cartRepo;
        

        public CartController(ICartRepository cartrepo)
        {
            this.cartRepo = cartrepo;
        }

        public async Task<IActionResult> addItem(int bookId, int qty, int redirect = 0)
        {
            var cartCount = await cartRepo.AddItem(bookId, qty);

            if (redirect == 0)
                return Ok(cartCount);

            return RedirectToAction("GetUserCart");
        }
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request)
        {
            var cartCount = await cartRepo.AddItem(request.BookId, request.Qty);
            return Ok(cartCount);
        }

        public async Task< IActionResult> removeItem (int bookId)
        {
            var cartCount = await cartRepo.RemoveItem(bookId);

            return RedirectToAction("GetUserCart");

        }
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await cartRepo.GetUserCart();

        return View(cart); }



        public async  Task<IActionResult> GEtTotalItemInCart()
        {

            int cartItem = await  cartRepo.GetCartItemCount();
            return Ok(cartItem);
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool isCheckedOut = await cartRepo.Checkout(model);

            if (!isCheckedOut)
                return RedirectToAction(nameof(OrderFailure));

            return RedirectToAction(nameof(OrderSuccess));
        }

     

        public IActionResult OrderSuccess()
        {
            return View("OrderSuccess");
        }

        public IActionResult OrderFailure()
        {
            return View("OrderFailure");
        }

    }
}

