using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User is not logged in");

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var cart = await GetCart(userId);
                if (cart == null)
                {
                    cart = new ShoppingCart { UserId = userId };
                    await db.ShoppingCarts.AddAsync(cart);
                    await db.SaveChangesAsync();
                }

                var cartItem = await db.CartDetails.FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty
                    };
                    await db.CartDetails.AddAsync(cartItem);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // ممكن تعمل logging هنا
                throw;
            }

            return await GetCartItemCount(userId);
        }

        public async Task<int> RemoveItem(int bookId)
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged in");

            var cart = await GetCart(userId);
            if (cart == null)
                throw new Exception("Cart is empty");

            var cartItem = await db.CartDetails.FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
            if (cartItem == null)
                throw new Exception("Item not found in cart");

            if (cartItem.Quantity == 1)
                db.CartDetails.Remove(cartItem);
            else
                cartItem.Quantity -= 1;

            await db.SaveChangesAsync();
            return await GetCartItemCount(userId);
        }
        public async Task<ShoppingCart> GetUserCart()
        {
            string userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Invalid userId");

            var shoppingCart = await db.ShoppingCarts
                .Include(a => a.CartDetails)
                    .ThenInclude(a => a.Book)
                        .ThenInclude(b => b.Stock)
                .Include(a => a.CartDetails)
                    .ThenInclude(a => a.Book)
                        .ThenInclude(b => b.Genre)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            return shoppingCart;
        }


        public async Task<ShoppingCart> GetCart(string userId)
        {
            return await db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in db.ShoppingCarts
                              join cartDetail in db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              where cart.UserId == userId 
                              select new { cartDetail.Id }
                        ).ToListAsync();
            return data.Count;
        }

        private string GetUserId()
        {
            var principal = httpContextAccessor.HttpContext?.User;
            return userManager.GetUserId(principal);
        }
    
         public async Task<bool> Checkout(CheckoutModel model)
        {
            using var transaction = db.Database.BeginTransaction();
            try
            {

                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new InvalidOperationException("Invalid cart");
                var cartDetail = db.CartDetails
                                    .Where(a => a.ShoppingCartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new InvalidOperationException("Cart is empty");
                var pendingRecord = db.orderStatuses.FirstOrDefault(s => s.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new InvalidOperationException("Order status does not have Pending status");
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false,
                    OrderStatusId = pendingRecord.Id
                };
                db.Orders.Add(order);
                db.SaveChanges();
                foreach (var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    db.OrderDetails.Add(orderDetail);


                    var stock = await db.Stocks.FirstOrDefaultAsync(a => a.BookId == item.BookId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null");
                    }

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} items(s) are available in the stock");
                    }
                    stock.Quantity -= item.Quantity;
                }

                db.CartDetails.RemoveRange(cartDetail);
                db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
       
       
    }
}
