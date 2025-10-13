namespace eCommerce.API.Extensions
{
    public static class CorsExtensions
    {
        private const string PolicyName = "AllowFrontend";

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:3002",
                            "http://localhost:3001"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }

        public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
        {
            app.UseCors(PolicyName);
            return app;
        }
    }
}