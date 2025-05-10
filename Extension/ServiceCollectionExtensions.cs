using NetCore.AutoRegisterDi;
using System.Reflection;

namespace DemoFYP.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static WebApplicationBuilder AutoRegisterServices(this WebApplicationBuilder builder, Assembly[] assemblies)
        {
            (from c in builder.Services.RegisterAssemblyPublicNonGenericClasses(assemblies)
             where c.Name.EndsWith("Service") || c.Name.EndsWith("Repository")
             select c).AsPublicImplementedInterfaces();

            return builder;
        }
    }

}
