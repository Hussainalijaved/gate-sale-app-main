using GateSale.Services.Interfaces;
using GateSale.Services;
using Microsoft.Extensions.Logging;

namespace GateSale
{
    public static class MauiProgram
    {
        // Base API URL - change this to your actual backend IP
        public static readonly string ApiBaseUrl = "http://YOUR_ALB_DNS_NAME/";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Register existing services
            builder.Services.AddSingleton<FavoriteService>();

            builder.Services.AddHttpClient("GateSaleAPI", client =>
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(120);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<AuthHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
#if ANDROID
                // Android-specific handler that bypasses SSL certificate validation for development
                var handler = new Xamarin.Android.Net.AndroidMessageHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                return handler;
#else
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
#endif
            });

            // Register AuthHeaderHandler
            builder.Services.AddTransient<AuthHeaderHandler>();

            // Also register IHttpClientFactory explicitly
            builder.Services.AddScoped<HttpClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                return factory.CreateClient("GateSaleAPI");
            });

            // Register new data services as singletons (for array-based storage)
            builder.Services.AddSingleton<IProductService, ProductService>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ISchoolService, SchoolService>();
            builder.Services.AddSingleton<ICategoryService, CategoryService>();
            builder.Services.AddSingleton<IDisputeService, DisputeService>();
            builder.Services.AddSingleton<IOrderService, OrderService>();
            builder.Services.AddSingleton<ILockerService, LockerService>();
            builder.Services.AddSingleton<IUserLockerService, UserLockerService>();
            builder.Services.AddSingleton<IPaymentService, PaymentService>();
            builder.Services.AddSingleton<IOrderTrackingService, OrderTrackingService>();
            builder.Services.AddSingleton<IPudoSimulationService, PudoSimulationService>();

            // Register product creation service as singleton (for session-based storage)
            builder.Services.AddSingleton<ProductCreationService>();

            // Register page transition service
            builder.Services.AddSingleton<PageTransitionService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

























