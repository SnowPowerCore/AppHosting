using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Navigation;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Processors;
using AppHosting.Xamarin.Forms.Extensions;
using AsyncAwaitBestPractices;
using RGPopup.Maui.Pages;
using RGPopup.Maui.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NavigationEventArgs = AppHosting.Xamarin.Forms.Abstractions.EventArgs.NavigationEventArgs;

namespace AppHosting.Xamarin.Forms.Services.Navigation;

public class LegacyTabbedNavigationService : INavigationService
{
    private readonly List<Guid> _processedItems = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly IAppVisualProcessor _appVisualProcessor;

    public event EventHandler<NavigationEventArgs> PageNavigating;
    public event EventHandler<NavigationEventArgs> PopupNavigating;
    public event EventHandler<NavigationEventArgs> ModalNavigating;

    public IReadOnlyList<Page> Pages
        => (Application.Current.Windows[0].Page as NavigationPage)?.Navigation.NavigationStack
        ?? (Application.Current.Windows[0].Page as TabbedPage)?.CurrentPage?.Navigation?.NavigationStack;

    public IReadOnlyList<PopupPage> PopupPages => PopupNavigation.Instance.PopupStack;

    public IReadOnlyList<Page> Modals
        => (Application.Current.Windows[0].Page as NavigationPage)?.Navigation.ModalStack
        ?? (Application.Current.Windows[0].Page as TabbedPage)?.CurrentPage?.Navigation?.ModalStack;

    public LegacyTabbedNavigationService(IServiceProvider serviceProvider,
                                         IAppVisualProcessor appVisualProcessor)
    {
        _serviceProvider = serviceProvider;
        _appVisualProcessor = appVisualProcessor;
    }

    public Task SwitchMainPageAsync<TPage>(TPage page)
    {
        if (page is Page xfPage)
        {
            ProcessPageAsync(xfPage).SafeFireAndForget();
            return CloseModalAsync()
                .ContinueWith(async t =>
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        Application.Current.Windows[0].Page = xfPage),
                            TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        return Task.CompletedTask;
    }

    public void DetermineAndSetMainPage<TPage>()
    {
        if (typeof(TPage).IsSubclassOf(typeof(Page)))
        {
            var page = (Page)_serviceProvider.GetService(typeof(TPage));
            ProcessPageAsync(page).SafeFireAndForget();
            Application.Current.Windows[0].Page = page;
        }
    }

    public Task OpenModalAsync<TPage>(TPage modal, bool animated = true)
    {
        if (modal is Page xfModal)
        {
            var navigatingArgs = new NavigationEventArgs
            {
                Cancel = false,
                NavigationPageType = typeof(TPage)
            };
            ModalNavigating?.Invoke(this, navigatingArgs);
            if (!navigatingArgs.Cancel)
            {
                ProcessPageAsync(xfModal).SafeFireAndForget();
                var navPage = Application.Current.Windows[0].Page as NavigationPage;
                var tabbedPage = Application.Current.Windows[0].Page as TabbedPage;
                if (navPage != null)
                    return MainThread.InvokeOnMainThreadAsync(() => navPage.Navigation.PushModalAsync(xfModal, animated));
                if (tabbedPage?.CurrentPage?.Navigation != null)
                    return MainThread.InvokeOnMainThreadAsync(() => tabbedPage.CurrentPage.Navigation.PushModalAsync(xfModal, animated));
                return Task.CompletedTask;
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        return Task.CompletedTask;
    }

    public Task CloseModalAsync(bool animated = true)
    {
        var navPage = Application.Current.Windows[0].Page as NavigationPage;
        var tabbedPage = Application.Current.Windows[0].Page as TabbedPage;
        if (navPage?.Navigation.ModalStack.Count > 0)
            return MainThread.InvokeOnMainThreadAsync(() => navPage.Navigation.PopModalAsync(animated));
        if (tabbedPage?.CurrentPage?.Navigation?.ModalStack.Count > 0)
            return MainThread.InvokeOnMainThreadAsync(() => tabbedPage.CurrentPage.Navigation.PopModalAsync(animated));
        return Task.CompletedTask;
    }

    public Task OpenPopupAsync(string routeWithParams, bool animated = true)
    {
        var popupPage = routeWithParams.GetElementFromRouting<PopupPage>();
        var navigatingArgs = new NavigationEventArgs
        {
            Cancel = false,
            NavigationPageType = popupPage.GetType()
        };
        PopupNavigating?.Invoke(this, navigatingArgs);
        if (!navigatingArgs.Cancel)
        {
            ProcessPageAsync(popupPage).SafeFireAndForget();
            return MainThread.InvokeOnMainThreadAsync(
                () => PopupNavigation.Instance.PushAsync(popupPage, animated));
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    public Task ClosePopupAsync(bool animated = true) =>
        PopupNavigation.Instance.PopupStack.Count > 0
            ? MainThread.InvokeOnMainThreadAsync(() => PopupNavigation.Instance.PopAsync(animated))
            : Task.CompletedTask;

    public Task NavigateToPageAsync(string routeWithParams, bool animated = true)
    {
        var tabbedPage = Application.Current.Windows[0].Page as TabbedPage;
        var navPage = Application.Current.Windows[0].Page as NavigationPage;
        var page = routeWithParams.GetElementFromRouting<Page>();
        var navigatingArgs = new NavigationEventArgs
        {
            Cancel = false,
            NavigationPageType = page.GetType()
        };
        PageNavigating?.Invoke(this, navigatingArgs);
        if (!navigatingArgs.Cancel)
        {
            ProcessPageAsync(page).SafeFireAndForget();
            if (tabbedPage?.CurrentPage?.Navigation != null)
                return MainThread.InvokeOnMainThreadAsync(() => tabbedPage.CurrentPage.Navigation.PushAsync(page, animated));
            if (navPage?.Navigation != null)
                return MainThread.InvokeOnMainThreadAsync(() => navPage.Navigation.PushAsync(page, animated));
        }
        return Task.CompletedTask;
    }

    public Task NavigateBackAsync(bool animated = true)
    {
        var tabbedPage = Application.Current.Windows[0].Page as TabbedPage;
        var navPage = Application.Current.Windows[0].Page as NavigationPage;
        var currentTab = tabbedPage?.CurrentPage;
        if (currentTab?.Navigation?.NavigationStack.Count > 1)
            return MainThread.InvokeOnMainThreadAsync(() => currentTab.Navigation.PopAsync(animated));
        if (navPage?.Navigation?.NavigationStack.Count > 1)
            return MainThread.InvokeOnMainThreadAsync(() => navPage.Navigation.PopAsync(animated));
        return Task.CompletedTask;
    }

    public Task NavigateToRootAsync(bool animated = true)
    {
        var tabbedPage = Application.Current.Windows[0].Page as TabbedPage;
        var navPage = Application.Current.Windows[0].Page as NavigationPage;
        if (tabbedPage?.CurrentPage?.Navigation != null)
            return MainThread.InvokeOnMainThreadAsync(() => tabbedPage.CurrentPage.Navigation.PopToRootAsync(animated));
        if (navPage?.Navigation != null)
            return MainThread.InvokeOnMainThreadAsync(() => navPage.Navigation.PopToRootAsync(animated));
        return Task.CompletedTask;
    }

    public Task SwitchItemAsync(int index) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            var tabbedPage = Application.Current.Windows[0].Page as TabbedPage;
            if (tabbedPage != null)
                tabbedPage.CurrentPage = tabbedPage.Children.ElementAtOrDefault(index);
        });

    private Task ProcessPageAsync(Page page) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (_processedItems.Contains(page.Id))
                return Task.CompletedTask;
            var elementTask = _appVisualProcessor.ElementProcessing?.Invoke(page);
            var pageTask = _appVisualProcessor.PageProcessing?.Invoke(page);
            _processedItems.Add(page.Id);
            return Task.WhenAll(elementTask, pageTask);
        });
}