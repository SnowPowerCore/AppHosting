using AppHosting.Xamarin.Forms.Attributes;
using System;
using Microsoft.Maui.Controls;

namespace AppHosting.Xamarin.Forms.Extensions;

public static class AttributeExtensions
{
    public static BindingContextAttribute[] GetElementBindingContextAttributes(this Element xfElement)
    {
        var bindingContextAttrs = (BindingContextAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(BindingContextAttribute));

        return bindingContextAttrs is default(BindingContextAttribute[])
            ? []
            : bindingContextAttrs;
    }

    public static CommandAttribute[] GetElementCommandAttributes(this Element xfElement)
    {
        var commandAttrs = (CommandAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(CommandAttribute),
            true);

        return commandAttrs is default(CommandAttribute[])
            ? []
            : commandAttrs;
    }

    public static AttachedCommandAttribute[] GetElementAttachedCommandAttributes(this Element xfElement)
    {
        var commandAttrs = (AttachedCommandAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(AttachedCommandAttribute),
            true);

        return commandAttrs is default(AttachedCommandAttribute[])
            ? []
            : commandAttrs;
    }

    public static AttachedLongPressCommandAttribute[] GetElementAttachedLongPressCommandAttributes(this Element xfElement)
    {
        var commandAttrs = (AttachedLongPressCommandAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(AttachedLongPressCommandAttribute),
            true);

        return commandAttrs is default(AttachedLongPressCommandAttribute[])
            ? []
            : commandAttrs;
    }

    public static AsyncCommandAttribute[] GetElementAsyncCommandAttributes(this Element xfElement)
    {
        var commandAttrs = (AsyncCommandAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(AsyncCommandAttribute),
            true);

        return commandAttrs is default(AsyncCommandAttribute[])
            ? []
            : commandAttrs;
    }

    public static AttachedAsyncCommandAttribute[] GetElementAttachedAsyncCommandAttributes(this Element xfElement)
    {
        var commandAttrs = (AttachedAsyncCommandAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(AttachedAsyncCommandAttribute),
            true);

        return commandAttrs is default(AttachedAsyncCommandAttribute[])
            ? []
            : commandAttrs;
    }

    public static AttachedAsyncLongPressCommandAttribute[] GetElementAttachedAsyncLongPressCommandAttributes(this Element xfElement)
    {
        var commandAttrs = (AttachedAsyncLongPressCommandAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(AttachedAsyncLongPressCommandAttribute),
            true);

        return commandAttrs is default(AttachedAsyncLongPressCommandAttribute[])
            ? []
            : commandAttrs;
    }

    public static ProcessElementAttribute[] GetProcessElementsAttributes(this Element xfElement)
    {
        var bindingContextAttrs = (ProcessElementAttribute[])Attribute.GetCustomAttributes(
            xfElement.GetType(),
            typeof(ProcessElementAttribute));

        return bindingContextAttrs is default(ProcessElementAttribute[])
            ? []
            : bindingContextAttrs;
    }

    public static PageAppearingAttribute GetElementPageAppearingAttribute(this Page xfPage)
    {
        var pageAppearingAttr = (PageAppearingAttribute)Attribute.GetCustomAttribute(
            xfPage.GetType(),
            typeof(PageAppearingAttribute));

        return pageAppearingAttr is default(PageAppearingAttribute)
            ? default
            : pageAppearingAttr;
    }

    public static PageDisappearingAttribute GetElementPageDisappearingAttribute(this Page xfPage)
    {
        var pageAppearingAttr = (PageDisappearingAttribute)Attribute.GetCustomAttribute(
            xfPage.GetType(),
            typeof(PageDisappearingAttribute));

        return pageAppearingAttr is default(PageDisappearingAttribute)
            ? default
            : pageAppearingAttr;
    }
}