using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.ViewModels.Coordinators;
using myPace.Views;
using Newtonsoft.Json;
using Prism.Commands;

namespace myPace.ViewModels
{
    public class SchoolsAndTeachersViewModel:ViewModelBase
    {
        #region Private Memebers
        private ListSchoolsView _schoolsView;
        private ListTeachersView _teachersView;
        private string _title;
        private object _frameContent;
        private string _userEditDtoJson;
        
        private UserAuthDto _connectedUser;
        private UserEditDto _userEditDto;
        private ListSchoolsViewModel _listSchoolsViewModel;
        private ListTeachersViewModel _listTeachersViewModel;
        private bool _canNavigate;
        #endregion

        #region Constructor
        public SchoolsAndTeachersViewModel(IHttpClientService httpClientService,INavigationService navigationService) : base(navigationService,httpClientService)
        {
            _schoolsView = new ListSchoolsView();
            _frameContent = _schoolsView;

            _title = "Schools";
            CanNavigate = false;
            RaisePropertyChanged(nameof(CanNavigate));
            RaisePropertyChanged(nameof(FrameContent));
        }
        
        private async Task LoadItemsAsync()
        { 
            var getSchools = await _httpClientService.GetListTAsync<School>("school/getallwithmedia");
            var getSchoolsForCoordinator = _userEditDto.ConnectedUser.Role == "Admin" ? getSchools : getSchools.Where(s => s.CoordinatorId == _userEditDto.ConnectedUser.UserId).ToList();
            _listSchoolsViewModel = new ListSchoolsViewModel(_httpClientService,_navigationService,_connectedUser,getSchoolsForCoordinator);
            _listTeachersViewModel = new ListTeachersViewModel(_httpClientService,_navigationService, _userEditDto.ConnectedUser, getSchoolsForCoordinator);
            CanNavigate = true;
        }
        public async Task InitialLoad()
        {
            var getSchools = await _httpClientService.GetListTAsync<School>("school/getall");
            var getSchoolsForCoordinator = _userEditDto.ConnectedUser.Role == "Admin" ? getSchools : getSchools.Where(s => s.CoordinatorId == _userEditDto.ConnectedUser.UserId).ToList();
            _listSchoolsViewModel = new ListSchoolsViewModel(_httpClientService, _navigationService, _connectedUser, getSchoolsForCoordinator);
            _listTeachersViewModel = new ListTeachersViewModel(_httpClientService, _navigationService, _userEditDto.ConnectedUser, getSchoolsForCoordinator);
            CanNavigate = true;
        }
        #endregion

        #region Properties
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }
        public bool CanNavigate
        {
            get { return _canNavigate; }
            set { SetProperty(ref _canNavigate, value); }
        }
        public object FrameContent
        {
            get { return _frameContent; }
            set { _frameContent = value; OnPropertyChanged(nameof(FrameContent)); }
        }
        public ListTeachersView ListTeachersView
        {
            get { return _teachersView; }
            set { _teachersView = value; OnPropertyChanged(nameof(ListTeachersView)); }
        }
        public ListSchoolsView ListSchoolsView
        {
            get { return _schoolsView; }
            set { _schoolsView = value; OnPropertyChanged(nameof(ListSchoolsView)); }
        }
        #endregion

        #region Commands
        public DelegateCommand AddSchoolTeachers => new DelegateCommand(() => _navigationService.Navigate(typeof(AddSchoolTeachersPage), _userEditDtoJson));
        
        
        //public DelegateCommand ReloadSource => new DelegateCommand(async () => await this.Load(_connectedUser));
        public DelegateCommand GoToSchoolsTabView => new DelegateCommand(() =>
        {
            FrameContent = _schoolsView;
            _title = "Schools";
            RaisePropertyChanged(nameof(FrameContent));
            RaisePropertyChanged(nameof(Title));
        }).ObservesCanExecute(() => CanNavigate);
        public DelegateCommand GoToTeachersTabView => new DelegateCommand(() =>
        {
            FrameContent = _teachersView;
            _title = "Teachers";
            RaisePropertyChanged(nameof(FrameContent));
            RaisePropertyChanged(nameof(Title));
        }).ObservesCanExecute(()=>CanNavigate);
        #endregion

        #region Methodes
        public async  Task Load(UserAuthDto connectedUser)
        {
            ProggressBarVisible(true);
            _connectedUser = connectedUser;
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
            _userEditDto = new UserEditDto(new BaseEntity())
            {
                ConnectedUser = _connectedUser,
                Type = Shared.Helpers.TypeEnum.None,
                IsNew = null
            };//Used to notify while navigating to Addschooteacherpage wether it should create a new entity or upadate an existing one based on the Type
            _userEditDtoJson = JsonConvert.SerializeObject(_userEditDto);
            await InitialLoad();
            ProggressBarVisible(false);
            await LoadItemsAsync();
            _schoolsView = new ListSchoolsView(_listSchoolsViewModel);
            _teachersView = new ListTeachersView(_listTeachersViewModel);
            _frameContent = _schoolsView;
            RaisePropertyChanged(nameof(FrameContent));
            RaisePropertyChanged(nameof(CanNavigate));
        }
        #endregion

    }
}
