﻿using Android.Content;
using Android.OS;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;

namespace AppHosting.Xamarin.Forms.Android
{
    internal static partial class XCT
    {
        private static Context? context;
        private static int? sdkInt;

        /// <summary>
        /// Gets the <see cref="Context"/>.
        /// </summary>
        internal static Context Context
        {
            get
            {
                var page = Application.Current.MainPage ?? throw new NullReferenceException($"{nameof(Application.MainPage)} cannot be null");
                var renderer = page.GetRenderer();

                if (renderer?.View.Context is not null)
                    context = renderer.View.Context;

                return renderer?.View.Context ?? context ?? throw new NullReferenceException($"{nameof(Context)} cannot be null");
            }
        }

        internal static int SdkInt => sdkInt ??= (int)Build.VERSION.SdkInt;
    }
}