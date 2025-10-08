using eCommerce.Application.Interfaces;
using eCommerce.Application.Services;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Repositories;

namespace eCommerce.API.Extensions;

    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Product
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            // Auth & User
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<UserValidator>();

            // Cart
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartService, CartService>();

            // Comment
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentService, CommentService>();

            // Wishlist
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<IWishlistService, WishlistService>();

            // Category
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();

            // User Address
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();
            services.AddScoped<IUserAddressService, UserAddressService>();

            // Order
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();
            
            //Audit Log
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            
            //Offer
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<IOfferService, OfferService>();
            
            // Slider and Cart Contents
            services.AddScoped<ISliderCartService, SliderCartService>();
            
            // Payment
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            
        }
    }
