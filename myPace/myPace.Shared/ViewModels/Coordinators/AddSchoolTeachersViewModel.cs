using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentValidation.Results;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Validation;
using myPace.Interfaces;
using Newtonsoft.Json;
using Prism.Commands;
using Windows.UI.Xaml;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Internal;
using myPace.Core.Model;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using myPace.Core.Helper;
using static myPace.Shared.Helpers.FileHelpers;
using myPace.Core.Helpers;
using System.Net.Http;
using System.Diagnostics;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;

namespace myPace.ViewModels
{
    public class AddSchoolTeachersViewModel:ViewModelBase
    {
        #region Private Memebers
        private Visibility _pageSchoolVisibility;
        private Visibility _pageTeacherVisibility;
        private string _title;
        private string _schoolImagePath;
        private string _teacherImagePath;
        private bool isNewTeacher = true;
        private bool isNewSchool = true;

        private string _userEditDtoJson;
        private UserEditDto _userEditDto;
        private UserAuthDto _connectedUser;
        private School _school;
        private Teacher _teacher;
        private TeacherValidator _teacherValidator;
        private SchoolValidator _schoolValidator;
        private ObservableCollection<string> _selectTeachers;
        private List<string> _teachersList;
        private ImageSource _schoolProfileImage;
        private FileData _schoolFileData;
        private ImageSource _teacherProfileImage;
        private FileData _teacherFileData;
        private IEnumerable<TypeShcoolEnum> _typeSchoolEnum;
        private IEnumerable<GenderEnum> _genderEnums;
        private ObservableCollection<SchoolDto> _schools;
        #endregion

        #region Constructor

        public AddSchoolTeachersViewModel(IHttpClientService httpClientService ,INavigationService navigationService):base(navigationService,httpClientService)
        {
            _genderEnums = Enum.GetValues(typeof(GenderEnum)).Cast<GenderEnum>();
            _typeSchoolEnum = Enum.GetValues(typeof(TypeShcoolEnum)).Cast<TypeShcoolEnum>();
            _schools = new ObservableCollection<SchoolDto>();
            _teachersList = new List<string>();
            _selectTeachers = new ObservableCollection<string>(_teachersList);
            _school = new School();
            _teacher = new Teacher();
            _teacher.DOB = DateTimeOffset.Now;
            _pageSchoolVisibility = SetVisisbilityFor();
            _title = "Add School";
            _errorVisibility = Visibility.Collapsed;           
            OnPropertyChanged(nameof(ErrorVisibility));
            RaisePropertyChanged(nameof(SchoolsType));
            RaisePropertyChanged(nameof(GenderList));
            RaisePropertyChangedForVisibility();
            
        }

        #endregion

        #region Properties
        public ObservableCollection<TypeShcoolEnum> SchoolsType => new ObservableCollection<TypeShcoolEnum>(_typeSchoolEnum.ToList());
        public ObservableCollection<GenderEnum> GenderList => new ObservableCollection<GenderEnum>(_genderEnums.ToList());
        public string TeacherImagePath
        {
            get { return _teacherImagePath; }
            set { SetProperty(ref _teacherImagePath,value); }
        }
        public string ImagePath
        {
            get { return _schoolImagePath; }
            set { SetProperty(ref _schoolImagePath, value); }
        }
        public ImageSource ProfileImageSource
        {
            get { return _schoolProfileImage; }
            set { _schoolProfileImage = value; OnPropertyChanged(nameof(ProfileImageSource)); }
        }
        public ImageSource TeacherProfileImage
        {
            get { return _teacherProfileImage; }
            set { SetProperty(ref _teacherProfileImage, value); }
        }
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }
        public Visibility PageSchoolVisibility
        {
            get { return _pageSchoolVisibility; }
            set { _pageSchoolVisibility = value; OnPropertyChanged(nameof(_pageSchoolVisibility)); }
        }
        public Visibility PageTeacherVisibility
        {
            get { return _pageTeacherVisibility; }
            set { _pageTeacherVisibility = value; OnPropertyChanged(nameof(PageTeacherVisibility)); }
        }
        
        public string SelectedTeacher { get; set; }
        public School School
        {
            get { return _school; }
            set { _school = value; OnPropertyChanged(nameof(School)); }
        }
        public Teacher Teacher
        {
            get { return _teacher; }
            set { _teacher = value; OnPropertyChanged(nameof(Teacher)); }
        }
        public ObservableCollection<string> SelectTeachers
        {
            get { return _selectTeachers; }
            set { _selectTeachers = value; OnPropertyChanged(nameof(SelectTeachers)); }
        }
        public ObservableCollection<SchoolDto> Schools
        {
            get { return _schools; }
            set { SetProperty(ref _schools, value); }
        }
        public SchoolDto SelectedSchool { get; set; }

        #endregion

        #region Commands
        public DelegateCommand AddSchoolViewVisible => new DelegateCommand(() =>
        {
            _title = "Add School";
            _pageSchoolVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand AddTeacherViewVisible => new DelegateCommand(() =>
        {
            _title = "Add Teacher";
            _pageTeacherVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand RegisterTeacherCommand => new DelegateCommand(OnRegisteringTeacher);
        public DelegateCommand RegisterSchoolCommand => new DelegateCommand(OnRegisteringSchool);
        public DelegateCommand FilterTeachers => new DelegateCommand(OnFilteringTeachers);
        public DelegateCommand UploadSchoolPicCommand => new DelegateCommand(OnUplaodPicture);
        public DelegateCommand UploadTeacherPicCommand => new DelegateCommand(OnUploadTeacherPic);

        private async void OnRegisteringSchool()
        {
            DisableButtons();
            _schoolValidator = new SchoolValidator();
            var valResult = await _schoolValidator.ValidateAsync(_school);
            if (valResult.IsValid)
            {
                _school.CoordinatorId = _connectedUser.UserId;
                HttpResponseMessage response = default;
                if(isNewSchool)
                {
                    response = await _httpClientService.PostBasicAsync(_school, "school/register");
                }
                else
                {
                    response = await _httpClientService.PutBasicAsync(_school, $"school/update/{_school.Id}");
                }
               
                if(response.IsSuccessStatusCode)
                {
                    var entity = JsonConvert.DeserializeObject<School>(await response.Content.ReadAsStringAsync());
                    if (_schoolFileData != null)
                    {
                        var imageUri = await UplaodFileAsync(_httpClientService,entity, "school", _schoolFileData);
                        _schoolProfileImage = imageUri != null ?  new BitmapImage(new Uri(imageUri)) : null;
                        RaisePropertyChanged(nameof(ProfileImageSource));                        
                    }
                    MessageDialog dialog = new MessageDialog($"{_school.Name} has been registered.");
                    await dialog.ShowAsync();
                    _school = new School();
                    RaisePropertyChanged(nameof(School));
                    HideValidationErrors();
                    EnableButtons();
                    return;
                }
                else
                {
                    MessageDialog dialog = new MessageDialog($"An error occured. {await response.Content.ReadAsStringAsync()}");
                    await dialog.ShowAsync();
                    EnableButtons();
                    return;
                }
                
            }
            RaiseValidationErrors(valResult);
            EnableButtons();
        }

        private async void OnRegisteringTeacher()
        {
            DisableButtons();
            _teacherValidator = new TeacherValidator();
            if(SelectedSchool == null)
            {
                MessageDialog dialog = new MessageDialog($"Kindly select a school");
                await dialog.ShowAsync();
                EnableButtons();
                return;
            }
            _teacher?.Email?.Trim();
            _teacher?.MobilePhone?.Trim();
            _teacher.SchoolId = SelectedSchool.School.Id;
            var valResult = await _teacherValidator.ValidateAsync(_teacher);
            HttpResponseMessage response = default;
            if (valResult.IsValid)
            {
                if (isNewTeacher)
                {
                    response = await _httpClientService.PostBasicAsync(_teacher, "teacher/register");
                }
                else
                {
                    response = await _httpClientService.PutBasicAsync(_teacher, $"teacher/update/{_teacher.Id}");
                }
                if (response.IsSuccessStatusCode)
                {
                    var entity = JsonConvert.DeserializeObject<Teacher>(await response.Content.ReadAsStringAsync());
                    if (_teacherFileData != null)
                    {
                        var imageUri  = await UplaodFileAsync(_httpClientService, entity, "teacher", _teacherFileData);
                        _teacherProfileImage = imageUri !=null ? new BitmapImage(new Uri(imageUri)) : null;
                        RaisePropertyChanged(nameof(TeacherProfileImage));
                    }
                    MessageDialog dialog = new MessageDialog($"{_teacher.FirstName} {_teacher.MiddleName} {_teacher.LastName} has been registered.");
                    await dialog.ShowAsync();
                    _teacher = new Teacher();
                    RaisePropertyChanged(nameof(Teacher));
                    HideValidationErrors();
                    EnableButtons();
                    return;
                }
                else
                {
                    MessageDialog dialog = new MessageDialog($"An error occured. {await response.Content.ReadAsStringAsync()}");
                    await dialog.ShowAsync();
                    EnableButtons();
                    return;
                }

            }
            RaiseValidationErrors(valResult);
            EnableButtons();
        }
        public void OnFilteringTeachers()
        {
            if(string.IsNullOrWhiteSpace(SelectedTeacher))
            {
                _selectTeachers = new ObservableCollection<string>(_teachersList);
                RaisePropertyChanged(nameof(SelectTeachers));
                return;
            }
            try
            {
                _selectTeachers = new ObservableCollection<string>(_teachersList?.Where(t => t.Contains(SelectedTeacher)));
            }
            catch(Exception ex)
            {
                Debugger.Log(0, "Error", ex.Message);
            }

            RaisePropertyChanged(nameof(SelectTeachers));
        }
        public async void OnUplaodPicture()
        {
            _schoolFileData = await CrossFilePicker.Current.PickFile();
            if (_schoolFileData == null)
                return; // user canceled file picking
            _schoolImagePath = _schoolFileData.FileName;
            _schoolProfileImage = new BitmapImage();//Remove actual displayed image
            RaisePropertyChanged(nameof(ImagePath));
            RaisePropertyChanged(nameof(ProfileImageSource));
        }
        private async void OnUploadTeacherPic()
        {
            _teacherFileData = await CrossFilePicker.Current.PickFile();
            if (_teacherFileData == null)
                return; // user canceled file picking
            _teacherImagePath = _teacherFileData.FileName;
            _teacherProfileImage = new BitmapImage();//Remove actual displayed image
            RaisePropertyChanged(nameof(TeacherImagePath));
            RaisePropertyChanged(nameof(TeacherProfileImage));
        }


        #endregion

        #region Methodes

        public async Task LoadAsync(UserEditDto userEditDto)
        {
            _userEditDtoJson = JsonConvert.SerializeObject(userEditDto);
            _userEditDto = userEditDto;
            _connectedUser = userEditDto.ConnectedUser;
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
            if (_userEditDto.IsNew == null)
            {
                isNewSchool = true;
                isNewTeacher = true;
            }
            else
            {
                switch(_userEditDto.Type)
                {
                    case Shared.Helpers.TypeEnum.School:
                        isNewSchool = false;
                        _school = await _httpClientService.GetTAsync<School>($"school/getbyIdInclude/{_userEditDto.Entity.Id}");
                        var mediaSchool = _school.Medias.OrderByDescending(x => x.UploadDate).FirstOrDefault();
                        _schoolImagePath = await GetFilePath(_httpClientService, "school", _school, mediaSchool);
                        _schoolProfileImage = _schoolImagePath != null ? new BitmapImage(new Uri(_schoolImagePath)) : null;
                        break;
                    case Shared.Helpers.TypeEnum.Teacher:
                        isNewTeacher = false; 
                        _teacher = await _httpClientService.GetTAsync<Teacher>($"teacher/getbyIdInclude/{_userEditDto.Entity.Id}");
                        SelectedSchool = new SchoolDto(_teacher.School);
                        var media = _teacher.Medias.OrderByDescending(x => x.UploadDate).FirstOrDefault();
                        _teacherImagePath = await GetFilePath(_httpClientService, "teacher", _teacher, media);
                        _teacherProfileImage = _teacherImagePath != null ? new BitmapImage(new Uri(_teacherImagePath)) : null;
                        break;
                    default:
                        isNewTeacher = true;
                        isNewSchool = true;
                        break;
                }
                RaisePropertyChanged(nameof(School));
                RaisePropertyChanged(nameof(Teacher));
                RaisePropertyChanged(nameof(SelectedSchool));
                RaisePropertyChanged(nameof(ProfileImageSource));
                RaisePropertyChanged(nameof(TeacherProfileImage));
            }
            
            var getSchools = await _httpClientService.GetListTAsync<School>("school/getall");
            foreach(var school in getSchools)
            {
                _schools.Add(new SchoolDto(school));
            }
            _teachersList = getSchools.Select(s => s.Principle).Distinct().ToList();
            _selectTeachers = new ObservableCollection<string>(_teachersList);
            RaisePropertyChanged(nameof(Schools));
            RaisePropertyChanged(nameof(SelectTeachers));
        }

        private Visibility SetVisisbilityFor()
        {
            _pageSchoolVisibility = Visibility.Collapsed;
            _pageTeacherVisibility = Visibility.Collapsed;
            return Visibility.Visible;

        }
        private void RaisePropertyChangedForVisibility()
        {
            OnPropertyChanged(nameof(PageSchoolVisibility));
            OnPropertyChanged(nameof(PageTeacherVisibility));
        }
        private void RaiseValidationErrors(ValidationResult result)
        {
            _validationErrors = result.ToString();
            _errorVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(ErrorVisibility));
            OnPropertyChanged(nameof(ValidationErrors));
        }
        private void HideValidationErrors()
        {
            _errorVisibility = Visibility.Collapsed;
            OnPropertyChanged(nameof(ErrorVisibility));
        }
        
        #endregion

    }
}
