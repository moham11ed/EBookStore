﻿using System.ComponentModel.DataAnnotations;

namespace BookShoppingCartMvcUI.Models
{
    
    public class CartDetail
    {
        public int Id { get; set; }
        [Required]
        public int ShoppingCartId { get; set; }
        [Required]
        public int BookId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public double UnitPrice { get; set; }   
        public Book Book { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
    }
}
