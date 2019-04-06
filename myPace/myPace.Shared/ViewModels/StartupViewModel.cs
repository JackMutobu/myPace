using FluentValidation.Results;
using myPace.Core.Dtos;
using myPace.Core.Helpers;
using myPace.Core.Interfaces;
using myPace.Core.Validation;
using myPace.Interfaces;
using myPace.Views.Coordinators;
using myPace.Views.Volunteers;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myPace.ViewModels
{
    public class StartupViewModel : ViewModelBase
    {
        public LoginViewModel _loginViewModel;
        public StartupViewModel(INavigationService navigationService, IHttpClientService httpClientService) : base(navigationService,httpClientService)
        {
            _loginViewModel = new LoginViewModel(httpClientService, navigationService);
        }
        public LoginViewModel LoginViewModel
        {
            get { return _loginViewModel; }
            set
            {
                _loginViewModel = value;
                OnPropertyChanged();
            }
        }
    }
}
