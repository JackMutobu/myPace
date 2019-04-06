using myPace.Controls.Volunteers;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using myPace.Views.Volunteers;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myPace.ViewModels.Volunteers
{
    public class HomeVolunteerViewModel : ViewModelBase
    {
        #region Private memebers TabView
        private object _frameContent;
        private ReportsVolView _reportsVolView;
        private HomeVolView _homeVolView;
        private AssignmentVolView _assignmentVolView;
        private ProjectVolView _projectVolView;
        private NotificationVolView _notificationVolView;

        #endregion
        private UserEditDto _userEditDto;
        private string _userEditDtoJson;
        private Visibility _projectCommandBarVisibility;
        private List<Project> _projects;
        private Volunteer _connectedVolunteer;
        private BaseEntityDto<Volunteer> _volunteer;

        public HomeVolunteerViewModel(INavigationService navigationService, IHttpClientService httpClientService) : base(navigationService, httpClientService)
        {
            _reportsVolView = new ReportsVolView();
            _homeVolView = new HomeVolView();
            _assignmentVolView = new AssignmentVolView();
            _projectVolView = new ProjectVolView();
            _notificationVolView = new NotificationVolView();
            _frameContent = _homeVolView;
            _projectCommandBarVisibility = Visibility.Collapsed;
        }
        #region Properties
        public object FrameContent
        {
            get { return _frameContent; }
            set { SetProperty(ref _frameContent, value); }
        }
        public Visibility ProjectCommandBarVisibility
        {
            get { return _projectCommandBarVisibility; }
            set { SetProperty(ref _projectCommandBarVisibility, value); }
        }
        #endregion
        #region TabCommands
        public DelegateCommand GoToHomeVolView => new DelegateCommand(() => { NavigateToTab(_homeVolView); });
        public DelegateCommand GoToProjectVolView => new DelegateCommand(() => 
        {
            NavigateToTab(_projectVolView);
            _projectCommandBarVisibility = SetVisisbilityFor();
            RaisePropertyChangedForVisibility();
        });
        public DelegateCommand GoToNewProjectPage => new DelegateCommand(() =>
        {
            var projectDto = new ProjectDto(new Project())
            {
                CanEdit = false,
                ConnectedUser = _connectedUserAuth
            };
           var projectDtoJson = JsonConvert.SerializeObject(projectDto);
            _navigationService.Navigate(typeof(NewProjectPage),projectDtoJson);
        });
        public DelegateCommand GoToReportVolView => new DelegateCommand(() => { NavigateToTab(_reportsVolView); });
        public DelegateCommand GoToAssignmentVolView => new DelegateCommand(() => { NavigateToTab(_assignmentVolView); });
        public DelegateCommand GoToNotificationVolView => new DelegateCommand(() => { NavigateToTab(_notificationVolView); });
        public DelegateCommand RefreshPageCommand => new DelegateCommand(async () =>
        {
            await LoadAsync(_volunteer);
        });
        #endregion
        public async Task LoadAsync(BaseEntityDto<Volunteer> volunteer)
        {
            ProggressBarVisible(true);
            _volunteer = volunteer;
            _connectedVolunteer = _volunteer.Entity;
            _connectedUserAuth = volunteer.ConnectedUser;
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUserAuth.Token);
            _projectVolView = new ProjectVolView(volunteer);
            _reportsVolView = new ReportsVolView(_connectedUserAuth);
            ProggressBarVisible(false);
            var school = await _httpClientService.GetTAsync<School>($"school/getbyid/{volunteer.Entity.SchoolVolunteers.First(s => s.IsActual == true)}");
            var projects = await EntityLoadingHelper.LoadProjectsAsync(new List<Volunteer> { _connectedVolunteer }, _httpClientService, new List<ProjectDto>(), _connectedUserAuth);
            _projectVolView = new ProjectVolView(projects, new List<Volunteer> { volunteer.Entity }, new List<School> { school });
        }
        
        #region Methodes
        private void NavigateToTab(UserControl view)
        {
            _frameContent = view;
            RaisePropertyChanged(nameof(FrameContent));
        }
        private Visibility SetVisisbilityFor()
        {
            _projectCommandBarVisibility = Visibility.Collapsed;
            return Visibility.Visible;

        }
        private void RaisePropertyChangedForVisibility()
        {
            RaisePropertyChanged(nameof(ProjectCommandBarVisibility));
        }
        #endregion

    }
}
