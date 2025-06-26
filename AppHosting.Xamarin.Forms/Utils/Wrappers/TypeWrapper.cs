using System;
using Microsoft.Maui.Controls;

namespace AppHosting.Xamarin.Forms.Utils.Wrappers;

public class TypeWrapper
{
    public Type Type { get; set; }

    public BindableObject Parent { get; set; }
}