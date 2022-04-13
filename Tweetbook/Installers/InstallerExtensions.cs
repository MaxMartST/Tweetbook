using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Tweetbook.Installers
{
    public static class InstallerExtensions
    {
        public static void InstallServicesInAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            // Получает коллекцию открытых типов, определенных в этой сборке
            // и видимых за ее пределами.

            var installers = typeof(Startup).Assembly.ExportedTypes
                .Where(x => 
                    typeof(IInstaller).IsAssignableFrom(x) // ищем подходящий экземпляр указанного типа IInstaller
                    && !x.IsInterface // не интерфейс
                    && !x.IsAbstract) // не абстрактный класс
                .Select(Activator.CreateInstance) // создание экземпляра
                .Cast<IInstaller>() // приводим в качестве интерфейса
                .ToList(); // выводим экземпляры в виде списка

            // запускаем установку каждого экземпляра
            installers
                .ForEach(installer => 
                    installer.InstallServices(services, configuration));
        }
    }
}
