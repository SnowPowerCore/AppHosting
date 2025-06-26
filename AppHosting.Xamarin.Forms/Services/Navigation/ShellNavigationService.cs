﻿using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Navigation;
using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Processors;
using AppHosting.Xamarin.Forms.Controls;
using AppHosting.Xamarin.Forms.Extensions;
using AsyncAwaitBestPractices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using RGPopup.Maui.Pages;
using RGPopup.Maui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NavigationEventArgs = AppHosting.Xamarin.Forms.Abstractions.EventArgs.NavigationEventArgs;

namespace AppHosting.Xamarin.Forms.Services.Navigation
{
    public class ShellNavigationService : INavigationService
    {
        private readonly List<Guid> _processedItems = new();

        private readonly IServiceProvider _serviceProvider;
        private readonly IAppVisualProcessor _appVisualProcessor;

        public event EventHandler<NavigationEventArgs> PageNavigating;
        public event EventHandler<NavigationEventArgs> PopupNavigating;
        public event EventHandler<NavigationEventArgs> ModalNavigating;

        public IReadOnlyList<Page> Pages => Shell.Current.Navigation.NavigationStack;

        public IReadOnlyList<PopupPage> PopupPages => PopupNavigation.Instance.PopupStack;

        public IReadOnlyList<Page> Modals => Shell.Current.Navigation.ModalStack;

        public ShellNavigationService(IServiceProvider serviceProvider,
                                      IAppVisualProcessor appVisualProcessor)
        {
            _serviceProvider = serviceProvider;
            _appVisualProcessor = appVisualProcessor;
        }

        public Task SwitchMainPageAsync<TPage>(TPage page)
        {
            if (page is Shell shell)
            {
                Shell.Current.FlyoutIsPresented = false;
                ProcessPageAsync(shell).SafeFireAndForget();
                return CloseModalAsync()
                    .ContinueWith(t =>
                        MainThread.InvokeOnMainThreadAsync(() =>
                            Application.Current.MainPage = shell),
                                TaskContinuationOptions.OnlyOnRanToCompletion);
            }

            return Task.CompletedTask;
        }

        public void DetermineAndSetMainPage<TPage>()
        {
            if (typeof(TPage).IsSubclassOf(typeof(Shell)))
            {
                var shell = (HostedShell)_serviceProvider.GetService(typeof(TPage));
                ProcessPageAsync(shell).SafeFireAndForget();
                Application.Current.MainPage = shell;
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
                    return MainThread.InvokeOnMainThreadAsync(
                        () => Shell.Current.Navigation.PushModalAsync(xfModal, animated));
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }

        public Task CloseModalAsync(bool animated = true) =>
            Shell.Current.Navigation.ModalStack.Count > 0
                ? MainThread.InvokeOnMainThreadAsync(
                    () => Shell.Current.Navigation.PopModalAsync(animated))
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
                ? MainThread.InvokeOnMainThreadAsync(
                    () => PopupNavigation.Instance.PopAsync(animated))
                : Task.CompletedTask;

        public Task NavigateToPageAsync(string routeWithParams, bool animated = true)
        {
            Shell.Current.FlyoutIsPresented = false;
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
                return MainThread.InvokeOnMainThreadAsync(
                    () => Shell.Current.Navigation.PushAsync(page, animated));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task NavigateBackAsync(bool animated = true) =>
            Shell.Current.Navigation.NavigationStack.Count > 1
                ? MainThread.InvokeOnMainThreadAsync(
                    () => Shell.Current.Navigation.PopAsync(animated))
                : Task.CompletedTask;

        public Task NavigateToRootAsync(bool animated = true)
        {
            Shell.Current.FlyoutIsPresented = false;
            return MainThread.InvokeOnMainThreadAsync(
                () => Shell.Current.Navigation.PopToRootAsync(animated));
        }

        public Task SwitchItemAsync(int index) =>
            MainThread.InvokeOnMainThreadAsync(
                () => Shell.Current.CurrentItem = Shell.Current.Items.ElementAtOrDefault(index));

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
}