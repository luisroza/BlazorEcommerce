using Stripe;
using Stripe.Checkout;

namespace BlazorEcommerce.Server.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IOrderService _orderService;
        
        const string secret = "whsec_ba5fb99b39473bc176497f180c73fedf909ed8e5a7797341fb73df1c1a585c3b";

        public PaymentService(ICartService cartService, IAuthService authService, IOrderService orderService)
        {
            StripeConfiguration.ApiKey = "sk_test_51KnsawCpfhEkHK7eSQM0zWSSuoVgKgD8XFLZyqSEwdPM4c93VdcBQMX1dymYEPmGPqTKJieiWyK32XTbJZQDXzYh00Y1E4BHPW";

            _cartService = cartService;
            _authService = authService;
            _orderService = orderService;
        }

        public async Task<Session> CreateCheckoutSession()
        {
            var products = (await _cartService.GetDbCartProducts()).Data;
            var lineItems = new List<SessionLineItemOptions>();

            if (products != null)
                products.ForEach(product => lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = product.Price * 100,
                        Currency = "cad",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = product.Title,
                            Images = new List<string> {product.ImageUrl}
                        }
                    },
                    Quantity = product.Quantity
                }));

            var options = new SessionCreateOptions
            {
                CustomerEmail = _authService.GetUserEmail(),
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "US", "CA" }
                },
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = "https://localhost:7231/order-success",
                CancelUrl = "https://localhost:7231/orders"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session;
        }

        public async Task<ServiceResponse<bool>> FulfillOrder(HttpRequest request)
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    request.Headers["Stripe-Signature"],
                    secret
                );

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    var user = await _authService.GetUserByEmail(session.CustomerEmail);
                    await _orderService.PlaceOrder(user.UserId);
                }

                return new ServiceResponse<bool> {Data = true};
            }
            catch (StripeException e)
            {
                return new ServiceResponse<bool> {Data = false, Success = false, Message = e.Message};
            }
        }
    }
}
