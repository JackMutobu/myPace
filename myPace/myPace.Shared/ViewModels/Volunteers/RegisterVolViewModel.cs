using System;
using myPace.Core.Entities;
using myPace.Core.Helpers;
using myPace.Core.Validation;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.ViewModels;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static myPace.Shared.Helpers.FileHelpers;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using myPace.Shared.Views;

namespace myPace.Shared.ViewModels.Volunteers
{
    public class RegisterVolViewModel:ViewModelBase
    {
        #region Private Variables
        private Visibility _pageOneVisibility;
        private Visibility _pageTwoVisibility;
        private Visibility _pageTwoBVisibility;
        private Visibility _pageThreeVisibility;
        private Visibility _pageFourVisibility;
        private ObservableCollection<SchoolDto> _schools;
        private string _imagePath;
        private ImageSource _profileImage;
        private Volunteer _volunteer;
        private string _otherClosestsSchool;
        private string _otherHighSchool;
        private FileData _file;
        private bool _isChecked;
        private VolunteerValidator _volunteerValidator;
        private List<string> _highSchoolList;
        private Visibility _otherSchoolVisibility;
        private string _selectedHighSchool;
        #endregion
        #region Constructor
        public RegisterVolViewModel(IHttpClientService httpClientService, INavigationService navigationService) : base(navigationService,httpClientService)
        {
            _pageOneVisibility = SetVisisbilityFor();
            _progressStackVisibility = Visibility.Collapsed;
            _otherSchoolVisibility = Visibility.Collapsed;
            _volunteer = new Volunteer();
            _volunteerValidator = new VolunteerValidator();
            RaisePropertyChangedForVisibility();
            _volunteer.DOB = DateTimeOffset.Now;
        }
        #endregion

        #region Properties
        public ObservableCollection<GenderEnum> GenderList => new ObservableCollection<GenderEnum>(GetGenderEnums().ToList());
        public ObservableCollection<SchoolDto> Schools
        {
            get { return _schools; }
            set { SetProperty(ref _schools, value); }
        }
        public string OtherClosestSchools
        {
            get { return _otherClosestsSchool; }
            set { SetProperty(ref _otherClosestsSchool, value); }
        }
        public string OtherHighSchool
        {
            get { return _otherHighSchool; }
            set { SetProperty(ref _otherHighSchool, value); }
        }
        public string ImagePath
        {
            get { return _imagePath; }
            set { SetProperty(ref _imagePath, value); }
        }
        public List<string> HighSchoolList
        {
            get { return _highSchoolList; }
            set { SetProperty(ref _highSchoolList,value); }
        }
        public string SelectedHighSchool
        {
            get { return _selectedHighSchool; }
            set
            {
                SetProperty(ref _selectedHighSchool, value);
                RaisePropertyChanged(nameof(SelectedHighSchool));
                _otherSchoolVisibility = _selectedHighSchool == "Other" ? Visibility.Visible : Visibility.Collapsed;
                RaisePropertyChanged(nameof(OtherSchoolVisibility));
            }
        }
        public ImageSource ProfileImageSource
        {
            get { return _profileImage; }
            set { _profileImage = value; OnPropertyChanged(nameof(ProfileImageSource)); }
        }
        public Volunteer Volunteer
        {
            get { return _volunteer; }
            set
            {
                SetProperty(ref _volunteer, value);
            }
        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked,value); }
        }
        public Visibility PageOneVisibility
        {
            get { return _pageOneVisibility; }
            set { _pageOneVisibility = value; OnPropertyChanged(nameof(_pageOneVisibility)); }
        }
        public Visibility PageTwoVisibility
        {
            get { return _pageTwoVisibility; }
            set { _pageTwoVisibility = value; OnPropertyChanged(nameof(PageTwoVisibility)); }
        }
        public Visibility PageTwoBVisibility
        {
            get { return _pageTwoBVisibility; }
            set { _pageTwoBVisibility = value; OnPropertyChanged(nameof(PageTwoBVisibility)); }
        }
        public Visibility PageThreeVisibility
        {
            get { return _pageThreeVisibility; }
            set { _pageThreeVisibility = value; OnPropertyChanged(nameof(PageThreeVisibility)); }
        }
        public Visibility PageFourVisibility
        {
            get { return _pageFourVisibility; }
            set { _pageFourVisibility = value; OnPropertyChanged(nameof(PageFourVisibility)); }
        }
        public Visibility OtherSchoolVisibility
        {
            get { return _otherSchoolVisibility; }
            set { SetProperty(ref _otherSchoolVisibility, value); }
        }
        #endregion
        #region Commands
        public DelegateCommand GoToPageTwoB => new DelegateCommand(() =>
        {
            _pageTwoBVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
        });
        public DelegateCommand GoToPageTwo => new DelegateCommand(() =>
        {
            _pageTwoVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
        });
        public DelegateCommand GoToPageOne => new DelegateCommand(() =>
        {
            _pageOneVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
        });
        public DelegateCommand GoToPageThree => new DelegateCommand(() =>
        {
            _pageThreeVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
        });
        public DelegateCommand GoToPageFour => new DelegateCommand(() =>
        {
            _pageFourVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
        });
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
        public DelegateCommand RegisterVolCommand => new DelegateCommand(async () =>
        {
            DisableButtons();
            if (!_schools.Any(s => s.IsChecked == true))
            {
                var mes = new MessageDialog("Kindly select closests schools")
                {
                    Title = "Error"
                };
                await mes.ShowAsync();
                EnableButtons();
                return;
            }
            if (!IsChecked)
            {
                MessageDialog messageDialog = new MessageDialog("Please accept our terms and conditions")
                {
                    Title = "Error"
                };
                await messageDialog.ShowAsync();
                EnableButtons();
                return;
            }
            _volunteer.HighSchool = _selectedHighSchool == "Other" ? _otherHighSchool : _selectedHighSchool;
            _volunteer.Email?.TrimEnd().ToLower();
            _volunteer.MobilePhone?.TrimEnd();
            _volunteer.Status = Core.Helper.StatusEnum.Registration;
            _volunteer.UploadDate = DateTime.Now;
            var valResult = _volunteerValidator.Validate(_volunteer);
            MessageDialog dialog = new MessageDialog("");
            if(valResult.IsValid)
            {
                try
                {
                    foreach(var school in _schools.Where(x=> x.IsChecked == true))
                    {
                        _volunteer.SchoolVolunteers.Add(new SchoolVolunteer
                        {
                            IsClosest = true,
                            IsActual = false,
                            SchoolId = school.School.Id
                        });
                    }
                    var response = await _httpClientService.PostBasicAsync(_volunteer, "volunteer/register");
                    if (response.IsSuccessStatusCode)
                    {
                        ProggressBarVisible(true);
                        var entity = JsonConvert.DeserializeObject<Volunteer>(await response.Content.ReadAsStringAsync());
                        if (_file != null)
                        {
                            var imageUri = await UplaodFileAsync(_httpClientService, entity, "volunteer", _file);
                        }
                        dialog.Title = "Success";
                        dialog.Content = $"{_volunteer.FirstName} {_volunteer.LastName} has been registered. Check your email for further information.";
                        _volunteer = new Volunteer();
                        RaisePropertyChanged(nameof(Volunteer));
                        ProggressBarVisible(false);
                        _navigationService.Navigate(typeof(StartupPage));
                    }
                    else
                    {
                        dialog.Title = "An error occured";
                        dialog.Content = await response.Content.ReadAsStringAsync();
                        
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
                    dialog.Content += "Try to check if all information you provided is coreect or try a different email address";
                }
                _volunteer.SchoolVolunteers.Clear();
                await dialog.ShowAsync();
                EnableButtons();
                return;
            }
            dialog.Content = valResult.ToString();
            await dialog.ShowAsync();
            EnableButtons();
        });
        #endregion

        #region Methodes
        public async Task LoadAsync()
        {
            var volunteers = await _httpClientService.GetListTAsync<Volunteer>("volunteer/getall");
            _highSchoolList = volunteers?.Select(h => h.HighSchool).ToList();
            var lastIndex = _highSchoolList?.Count != 0 || _highSchoolList?.Count == 1 ? _highSchoolList?.Count - 1 : 0 ;
            _highSchoolList.Insert((int)lastIndex, "Other");
           _highSchoolList =  _highSchoolList?.Distinct().ToList();
            RaisePropertyChanged(nameof(HighSchoolList));
            _schools = await GetSchoolDtos(_httpClientService);
            RaisePropertyChanged(nameof(Schools));
        }
        private Visibility SetVisisbilityFor()
        {
            _pageOneVisibility = Visibility.Collapsed;
            _pageTwoVisibility = Visibility.Collapsed;
            _pageTwoBVisibility = Visibility.Collapsed;
            _pageThreeVisibility = Visibility.Collapsed;
            _pageFourVisibility = Visibility.Collapsed;
            return Visibility.Visible;

        }
        private void RaisePropertyChangedForVisibility()
        {
            OnPropertyChanged(nameof(PageOneVisibility));
            OnPropertyChanged(nameof(PageTwoVisibility));
            OnPropertyChanged(nameof(PageTwoBVisibility));
            OnPropertyChanged(nameof(PageThreeVisibility));
            OnPropertyChanged(nameof(PageFourVisibility));
        }
        #endregion
    }
}
