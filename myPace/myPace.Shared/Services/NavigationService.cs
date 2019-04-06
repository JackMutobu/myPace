using myPace.Interfaces;
using myPace.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myPace.Services
{
    public class NavigationService:INavigationService
    {
        private Frame _currentFrame;

        public NavigationService()
        {
            var frame = (Frame)Window.Current.Content;
            _currentFrame = frame;
        }
        public void Navigate(Type sourcePage)
        {
            var frame = (Frame)Window.Current.Content;
            frame.Navigate(sourcePage);
        }

        public void Navigate(Type sourcePage, object parameter)
        {
            var frame = (Frame)Window.Current.Content;
            frame.Navigate(sourcePage, parameter);
        }

        public void GoBack()
        {
            var frame = (Frame)Window.Current.Content;
            frame.GoBack();
        }
        public void GoForward()
        {
            var frame = (Frame)Window.Current.Content;
            frame.GoForward();
        }
        public bool CanGoForward
        {
            get { return _currentFrame.CanGoForward; }
        }
        public bool CanGoBack
        {
           get { return _currentFrame.CanGoBack; }
        }
    }
}
