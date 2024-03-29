﻿using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Navigation;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Processors;
using AppHosting.Xamarin.Forms.Extensions;
using AsyncAwaitBestPractices;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using NavigationEventArgs = AppHosting.Xamarin.Forms.Abstractions.EventArgs.NavigationEventArgs;

namespace AppHosting.Xamarin.Forms.Services.Navigation
{
    public class LegacyTabbedNavigationService : INavigationService
    {
        private readonly List<Guid> _processedItems = new();

        private readonly IServiceProvider _serviceProvider;
        private readonly IAppVisualProcessor _appVisualProcessor;

        public event EventHandler<NavigationEventArgs> PageNavigating;
        public event EventHandler<NavigationEventArgs> PopupNavigating;
        public event EventHandler<NavigationEventArgs> ModalNavigating;

        public IReadOnlyList<Page> Pages => Application.Current.MainPage.Navigation.NavigationStack;

        public IReadOnlyList<PopupPage> PopupPages => PopupNavigation.Instance.PopupStack;

        public IReadOnlyList<Page> Modals => Application.Current.MainPage.Navigation.ModalStack;

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
                    .ContinueWith(t =>
                        Device.InvokeOnMainThreadAsync(() =>
                            Application.Current.MainPage = xfPage),
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
                Application.Current.MainPage = page;
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
                    return Device.InvokeOnMainThreadAsync(
                        () => Application.Current.MainPage.Navigation.PushModalAsync(
                            xfModal, animated));
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }

        public Task CloseModalAsync(bool animated = true) =>
            Application.Current.MainPage.Navigation.ModalStack.Count > 0
                ? Device.InvokeOnMainThreadAsync(
                    () => Application.Current.MainPage.Navigation.PopModalAsync(animated))
                : Task.CompletedTask;

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
                return Device.InvokeOnMainThreadAsync(
                    () => PopupNavigation.Instance.PushAsync(popupPage, animated));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task ClosePopupAsync(bool animated = true) =>
            PopupNavigation.Instance.PopupStack.Count > 0
                ? Device.InvokeOnMainThreadAsync(() => PopupNavigation.Instance.PopAsync(animated))
                : Task.CompletedTask;

        public Task NavigateToPageAsync(string routeWithParams, bool animated = true)
        {
            var tabbedPage = (TabbedPage)Application.Current.MainPage;
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
                return Device.InvokeOnMainThreadAsync(
                    () => tabbedPage.CurrentPage.Navigation.PushAsync(page, animated));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task NavigateBackAsync(bool animated = true)
        {
            var tabbedPage = (TabbedPage)Application.Current.MainPage;
            var currentTab = tabbedPage.CurrentPage;
            if (currentTab.Navigation.NavigationStack.Count > 1)
                return Device.InvokeOnMainThreadAsync(
                    () => currentTab.Navigation.PopAsync(animated));
            return Task.CompletedTask;
        }

        public Task NavigateToRootAsync(bool animated = true)
        {
            var tabbedPage = (TabbedPage)Application.Current.MainPage;
            return Device.InvokeOnMainThreadAsync(
                () => tabbedPage.CurrentPage.Navigation.PopToRootAsync(animated));
        }

        public Task SwitchItemAsync(int index) =>
            Device.InvokeOnMainThreadAsync(() =>
            {
                var tabbedPage = (TabbedPage)Application.Current.MainPage;
                tabbedPage.CurrentPage = tabbedPage.Children.ElementAtOrDefault(index);
            });

        private Task ProcessPageAsync(Page page) =>
            Device.InvokeOnMainThreadAsync(() =>
            {
                if (_processedItems.Contains(page.Id))
                    return Task.CompletedTask;
                var elementTask = _appVisualProcessor.ElementProcessing?.Invoke(page);
                var pageTask = _appVisualProcessor.PageProcessing?.Invoke(page);
                _processedItems.Add(page.Id);
                return Task.WhenAll(elementTask, pageTask);
            });
    }
}