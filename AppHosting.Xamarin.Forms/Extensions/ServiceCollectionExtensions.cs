using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Factory;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Navigation;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Processors;
using AppHosting.Xamarin.Forms.Internal.Factory;
using AppHosting.Xamarin.Forms.Middleware;
using AppHosting.Xamarin.Forms.Services.Navigation;
using AppHosting.Xamarin.Forms.Services.Processors;
using Microsoft.Extensions.DependencyInjection;

namespace AppHosting.Xamarin.Forms.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVisualProcessingCore(this IServiceCollection services)
    {
        services.AddSingleton<BindingContextMiddleware>();
        services.AddSingleton<ChildrenBindingContextMiddleware>();
        services.AddSingleton<PageAppearingMiddleware>();
        services.AddSingleton<PageDisappearingMiddleware>();
        services.AddSingleton<ProcessElementMiddleware>();
        services.AddSingleton<CommandMiddleware>();
        services.AddSingleton<AsyncCommandMiddleware>();
        services.AddSingleton<AttachedCommandMiddleware>();
        services.AddSingleton<AttachedAsyncCommandMiddleware>();
        services.AddSingleton<AttachedLongPressCommandMiddleware>();
        services.AddSingleton<AttachedAsyncLongPressCommandMiddleware>();
        return services;
    }

    /// <summary>
    /// Assigns app visual builder.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection UseAppVisualProcessor(this IServiceCollection services) =>
        services.AddSingleton<IPageBuilderFactory, PageBuilderFactory>()
            .AddSingleton<IElementBuilderFactory, ElementBuilderFactory>()
            .AddSingleton<IAppVisualProcessor, AppVisualProcessor>();

    /// <summary>
    /// Tells the application to use shell navigation as a main navigation service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection UseShellNavigation(this IServiceCollection services) =>
        services.AddSingleton<INavigationService, ShellNavigationService>();

    /// <summary>
    /// Tells the application to use legacy tabbed navigation as a main navigation service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection UseLegacyTabbedNavigation(this IServiceCollection services) =>
        services.AddSingleton<INavigationService, LegacyTabbedNavigationService>();
}