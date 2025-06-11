namespace AppHosting.Xamarin.Forms.Abstractions.Interfaces.Services.Navigation
{
    public interface INavigationService : IPageNavigation, IModalNavigation, ITabbedNavigation // Removed IPopupNavigation for migration
    {
    }
}