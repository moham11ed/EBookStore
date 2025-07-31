using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    public class UserOrderController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepo;

        public UserOrderController(IUserOrderRepository userOrderRepo)
        {
            _userOrderRepo = userOrderRepo;
        }
        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderRepo.UserOrders();
            return View(orders);
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _userOrderRepo.GetOrderById(orderId);
            if (order == null || order.IsDeleted)
            {
                return NotFound();
            }

            order.OrderStatusId = 5; 
            order.IsDeleted = true;

            await _userOrderRepo.SaveChanges(); 

            return RedirectToAction(nameof(UserOrders));
        }

    }
}
