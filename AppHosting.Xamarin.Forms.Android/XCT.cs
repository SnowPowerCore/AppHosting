using Android.Content;
using Android.OS;
using System;
using Microsoft.Maui.Controls;
using AView = Android.Views.View;

namespace AppHosting.Xamarin.Forms.Android
{
    internal static partial class XCT
    {
        private static Context context;
        private static int? sdkInt;

        /// <summary>
        /// Gets the <see cref="Context"/>.
        /// </summary>
        internal static Context Context
        {
            get
            {
                var window = Application.Current.Windows.Count > 0 ? Application.Current.Windows[0] : throw new NullReferenceException($"No windows available");
                var page = window.Page ?? throw new NullReferenceException($"Window.Page cannot be null");
                var handler = page.Handler ?? throw new NullReferenceException($"Page.Handler cannot be null");
                var platformView = handler.PlatformView as AView ?? throw new NullReferenceException($"PlatformView is not an Android.Views.View");

                if (platformView.Context is not null)
                    context = platformView.Context;

                return platformView.Context ?? context ?? throw new NullReferenceException($"{nameof(Context)} cannot be null");
            }
        }

        internal static int SdkInt => sdkInt ??= (int)Build.VERSION.SdkInt;
    }
}