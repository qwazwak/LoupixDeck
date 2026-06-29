using Microsoft.Extensions.DependencyInjection;

namespace QCommon;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommand<TCommand>() where TCommand : PluginCommandBase => services.AddSingleton<PluginCommandBase, TCommand>();
        public IServiceCollection AddMenuContributor<TContributor>() where TContributor : MenuContributorBase => services.AddSingleton<MenuContributorBase, TContributor>();
    }
}