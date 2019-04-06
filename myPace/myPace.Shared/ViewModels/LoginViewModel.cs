using FluentValidation.Results;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Helpers;
using myPace.Core.Interfaces;
using myPace.Core.Validation;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Views;
using myPace.Views.Coordinators;
using myPace.Views.Volunteers;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myPace.ViewModels
{
    public class LoginViewModel:ViewModelBase
    {
        #region Private variables
        private string _email;
        private string _password;
        private UserValidator _userValidator;
        #endregion
        public LoginViewModel(IHttpClientService httpClientService, INavigationService navigationService) : base(navigationService,httpClientService)
        {
            _errorVisibility = Visibility.Collapsed;
            _progressStackVisibility = Visibility.Collapsed;
            OnPropertyChanged(nameof(ErrorVisibility));
        }
        #region Properties
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }
       
        #endregion
        #region Commands
        public DelegateCommand GoToHomePage => new DelegateCommand(OnUserAuthentication);
        public DelegateCommand GoToRegisterPage => new DelegateCommand(() =>
        {
            ProggressBarVisible(true);
            _navigationService.Navigate(typeof(RegisterVolPage));
        });
        #endregion
        #region Methodes
        private async void OnUserAuthentication()
        {
            //Disable buttons
            DisableButtons();
            var dialog = new MessageDialog("");
            dialog.Commands.Add(new UICommand("Ok")
            {
                Id = 0
            });
            dialog.CancelCommandIndex = 0;
            var loginUser = new UserDto
            {
                Email = _email?.Trim()?.ToLower(),
                Password = _password?.Trim()
            };
            _userValidator = new UserValidator();
            ValidationResult valResult = await _userValidator.ValidateAsync(loginUser);
            
            if (valResult.IsValid)
            {
                _errorVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(ValidationErrors));
                OnPropertyChanged(nameof(ErrorVisibility));
                try
                {
                    ProggressBarVisible(true);
                    var user = await _httpClientService.AuthenticateUser(loginUser);
                    if (user != null)
                    {
                        dialog.Title = "Success";
                        dialog.Content = $"Welcome {user.FirstName} {user.LastName} to PACE!";
                        _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, user.Token);
                        await dialog.ShowAsync();
                        if (user.Role == "Admin" || user.Role == "Coord")
                        {
                            await SignInUser<Coordinator>(user,"coordinator",new HomeCoordinatorPage());
                        }
                        else
                        {
                            await SignInUser<Volunteer>(user, "volunteer", new HomeVolunteerPage());
                        }
                        _errorVisibility = Visibility.Collapsed;
                        OnPropertyChanged(nameof(ErrorVisibility));
                        EnableButtons();
                        return;
                    }
                }
                catch (AppException ex)
                {
                    dialog.Title = "Error";
                    dialog.Content = ex.Message;
                    await dialog.ShowAsync();
                    EnableButtons();
                    ProggressBarVisible(false);
                    return;
                }
                catch (Exception ex)
                {
                    dialog.Title = "Error";
                    dialog.Content = ex.Message;
                    await dialog.ShowAsync();
                    EnableButtons();
                    ProggressBarVisible(false);
                    return;
                }
            }
            _validationErrors = valResult.ToString();
            _errorVisibility = Visibility.Visible;
            EnableButtons();
            OnPropertyChanged(nameof(ValidationErrors));
            OnPropertyChanged(nameof(ErrorVisibility));
            dialog.Content = _validationErrors;
            await dialog.ShowAsync();
        }

        private async Task SignInUser<T>(UserAuthDto user,string controllerForGettingDetailsOfUser, Page pageToNavigateTo) where T:BasePersonEntity
        {
            var entity = await _httpClientService.GetTAsync<T>($"{controllerForGettingDetailsOfUser}/getbyemail/{user.Email}");
            user.UserId = entity.Id;
            _connectedUserAuth = user;
            var navigationDto = new BaseEntityDto<T>(entity)
            {
                Age = (DateTime.Now - entity.DOB.DateTime).Days / 365,
                ConnectedUser = _connectedUserAuth
            };
            var navigationDtoJson = JsonConvert.SerializeObject(navigationDto, Formatting.Indented);
            ProggressBarVisible(true);
            if (pageToNavigateTo is HomeCoordinatorPage)
                _navigationService.Navigate(typeof(HomeCoordinatorPage), navigationDtoJson);
            else
                _navigationService.Navigate(typeof(HomeVolunteerPage), navigationDtoJson);
        }


        #endregion
    }
}
