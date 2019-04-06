using System;

namespace myPace.Interfaces
{
    public interface INavigationService
    {
        void Navigate(Type sourcePage);
        void Navigate(Type sourcePage, object parameter);
        void GoBack();
        void GoForward();
        bool CanGoForward { get; }
        bool CanGoBack { get; }
    }
}
