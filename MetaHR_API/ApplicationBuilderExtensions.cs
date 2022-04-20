using DataAccess.DbInitializer;

namespace MetaHR_API
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app)
        {
            using var scopedServices = app.ApplicationServices.CreateScope();
            var serviceProvider = scopedServices.ServiceProvider;
            var dbInitializer = serviceProvider.GetRequiredService<IDbInitializer>();

            await dbInitializer.InitializeRoles();
            await dbInitializer.SeedAdminUserr();

            return app;
        }
    }
}
