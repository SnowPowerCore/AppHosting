using AppHosting.Xamarin.Forms.Abstractions.Interfaces.Builders;

namespace AppHosting.Xamarin.Forms.Abstractions.Interfaces;

public interface IPageElementConfigure
{
    void ConfigurePage(IPageBuilder pageBuilder);

    void ConfigureElement(IElementBuilder elementBuilder);
}