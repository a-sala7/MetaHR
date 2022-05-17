using DataAccess.DbInitializer;

namespace MetaHR_API
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app, string env)
        {
            using var scopedServices = app.ApplicationServices.CreateScope();
            var serviceProvider = scopedServices.ServiceProvider;
            var dbInitializer = serviceProvider.GetRequiredService<IDbInitializer>();

            if(env.ToLower() != "development")
            {
                await dbInitializer.Migrate();
            }
            await dbInitializer.InitializeRoles();
            await dbInitializer.SeedAdminUser();

            return app;
        }
    }
}
