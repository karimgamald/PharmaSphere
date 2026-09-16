using System.Collections.Generic;
using System.Linq;

namespace PharmaSphere.Web.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal DeliveryFee { get; set; } = 15.00m;
        public decimal SubTotal => CartItems.Sum(item => item.TotalPrice);
        public decimal GrandTotal => SubTotal + DeliveryFee;

        public CheckoutViewModel CheckoutInfo { get; set; } = new();
    }
}