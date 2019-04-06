using FluentValidation.Results;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Helpers;
using myPace.Core.Validation;
using myPace.Services;
using Newtonsoft.Json;
using Prism.Commands;
using Windows.UI.Xaml;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using myPace.Interfaces;
using System.Threading.Tasks;
using static myPace.Shared.Helpers.FileHelpers;
using System.Collections.ObjectModel;
using System.Linq;
using myPace.Shared.Helpers;
using myPace.Shared.Dtos;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;

namespace myPace.ViewModels
{
    public class AddCoordinatorViewModel: ViewModelBase
    {
        #region Private Members
        private Coordinator _coordinator;
        private CoordinatorValidator _coordValidator;
        private string _connectedUserJson;
        private UserAuthDto _connectedUser;
        private ObservableCollection<SchoolDto> _schools;
        private BaseValidationHelpers _validationHelpers;
        private ImageSource _profileImage;
        private FileData _file;
        private string _imagePath;
        private bool isNew = true;
        #endregion
        public AddCoordinatorViewModel(IHttpClientService httpClientService, INavigationService navigationService):base(navigationService,httpClientService)
        {
            _coordinator = new Coordinator();
            _coordinator.DOB = DateTimeOffset.Now;
            _validationHelpers = new BaseValidationHelpers();
            _errorVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(ErrorVisibility));
        }
        #region Properties
        public ObservableCollection<GenderEnum> GenderList => new ObservableCollection<GenderEnum>(GetGenderEnums().ToList());
        public ObservableCollection<SchoolDto> Schools
        {
            get { return _schools; }
            set { SetProperty(ref _schools, value); }
        }
        public string ImagePath
        {
            get { return _imagePath; }
            set { SetProperty(ref _imagePath, value); }
        }
        public ImageSource ProfileImageSource
        {
            get { return _profileImage; }
            set { _profileImage = value; OnPropertyChanged(nameof(ProfileImageSource)); }
        }
        public Coordinator Coordinator
        {
            get { return _coordinator; }
            set
            {
                _coordinator = value; OnPropertyChanged(nameof(Coordinator));
            }
        }

        #endregion
                #region Commands
        public DelegateCommand RegisterCoordCommand => new DelegateCommand(OnRegisterCoordinator);        
        public DelegateCommand UploadProfileCommand => new DelegateCommand(async () =>
        {
            _file = await CrossFilePicker.Current.PickFile();
            if (_file == null)
                return; // user canceled file picking
            _imagePath = _file.FileName;
            _profileImage = new BitmapImage();//Remove actual displayed image
            RaisePropertyChanged(nameof(ImagePath));
            RaisePropertyChanged(nameof(ProfileImageSource));
        });
        #endregion
        #region Methodes
        public async Task LoadAsync(UserAuthDto connectedUser)
        {
            _connectedUserJson = JsonConvert.SerializeObject(connectedUser);
            _connectedUser = connectedUser;
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
            _schools = await GetSchoolDtos(_httpClientService);
            RaisePropertyChanged(nameof(Schools));
        }
        private async void OnRegisterCoordinator()
        {
            DisableButtons();
            if (!_schools.Any(s=> s.IsChecked == true))
            {
                var mes = new MessageDialog("Kindly select a school");
                await mes.ShowAsync();
                EnableButtons();
                return;
            }
            _coordValidator = new CoordinatorValidator();
            _coordinator?.Email?.Trim();
            _coordinator?.MobilePhone?.Trim();
            _coordinator.UploadDate = DateTime.Now;
            _coordinator.Status = Core.Helper.StatusEnum.Active;
            foreach(var school in _schools)
            {
                if(school.IsChecked == true)
                {
                    _coordinator?.Schools.Add(school.School);
                }
            }
           
            ValidationResult valResult = await _coordValidator.ValidateAsync(_coordinator);
            var dialog = new MessageDialog("");
            dialog.Commands.Add(new UICommand("Ok")
            {
                Id = 0
            });
            dialog.CancelCommandIndex = 0;
            if (valResult.IsValid)
            {
                try
                {
                    var saveCoordinator = new HttpResponseMessage();
                    if (isNew)
                    {
                        saveCoordinator = await _httpClientService.PostBasicAsync(_coordinator, "coordinator/register");
                    }
                    else
                    {
                        saveCoordinator = await _httpClientService.PutBasicAsync(_coordinator, $"coordinator/update/{_coordinator.Id}");
                    }
                    if(saveCoordinator.IsSuccessStatusCode)
                    {
                        var entity = JsonConvert.DeserializeObject<Coordinator>(await saveCoordinator.Content.ReadAsStringAsync());
                        if (_file != null)
                        {
                            var imageUri = await UplaodFileAsync(_httpClientService, entity, "coordinator", _file);
                            _profileImage = imageUri != null ? new BitmapImage(new Uri(imageUri)) : null;
                            RaisePropertyChanged(nameof(ProfileImageSource));
                        }
                        dialog.Title = "Success";
                        dialog.Content = $"{_coordinator.FirstName} {_coordinator.LastName} has been registered.";
                        _coordinator = new Coordinator();
                        RaisePropertyChanged(nameof(Coordinator));
                        _errorVisibility = Visibility.Collapsed;
                        RaisePropertyChanged(nameof(ErrorVisibility));
                    }
                    else
                    {
                        dialog.Title = "An error occured";                       
                        dialog.Content = await saveCoordinator.Content.ReadAsStringAsync();
                    }
                }
                catch (AppException ex)
                {
                    dialog.Title = "Error";
                    dialog.Content = ex.Message;
                }
                catch (Exception ex)
                {
                    dialog.Title = "Error";
                    dialog.Content = ex.Message;
                }
                await dialog.ShowAsync();
                EnableButtons();
                return;
            }
            _validationHelpers.RaiseValidationErrors(valResult);
            _validationErrors = _validationHelpers.ValidationErrors;
            _errorVisibility = _validationHelpers.ErrorVisibility;
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(ErrorVisibility));
            EnableButtons();
        }
        
        #endregion
    }
}
