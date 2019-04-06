using myPace.Controls.Volunteers;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using myPace.ViewModels;
using myPace.Views;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace myPace.ViewModels.Coordinators
{
    public class HomeCoordViewModel:ViewModelBase
    {
        #region Private memebers TabView
        private object _frameContent;
        private ReportsVolView _reportCoordView;
        private TrainingCoordView _trainingCoordView;
        private AssignmentCoordView _assignmentCoordView;
        private NotificationsCoordView _notificationsCoordView;
        private ProjectVolView _projectsCoordView;
        private VolunteerCoordView _volunteerCoordView;
        #endregion
        private List<Volunteer> _volunteers;
        private UserAuthDto _connectedUser;
        private string _connectedUserJson;
        private string _title;
        private List<SchoolDto> _schoolDtos;
        private List<School> _schools;
        private List<BaseEntityDto<Volunteer>> _volunteersDto;
        private List<ProjectDto> _projects;
        private Visibility _addCoordCommandVisibility;

        public HomeCoordViewModel(INavigationService navigationService, IHttpClientService httpClientService) : base(navigationService,httpClientService)
        {
            _reportCoordView = new ReportsVolView();
            _projectsCoordView = new ProjectVolView();
            _assignmentCoordView = new AssignmentCoordView();
            _notificationsCoordView = new NotificationsCoordView();
            _trainingCoordView = new TrainingCoordView();
            _volunteers = new List<Volunteer>();
            _volunteersDto = new List<BaseEntityDto<Volunteer>>();
            _schoolDtos = new List<SchoolDto>();
            _projects = new List<ProjectDto>();
            _frameContent = _reportCoordView;
            _title = "Reports";
            OnPropertyChanged(nameof(FrameContent));
        }
        public Visibility AddCoordCommandVisibility
        {
            get { return _addCoordCommandVisibility; }
            set { _addCoordCommandVisibility = value; OnPropertyChanged(); }
        }
        
        #region View Properties
        public ReportsVolView ReportCoordView
        {
            get { return _reportCoordView; }
            set { _reportCoordView = value; OnPropertyChanged(); }
        }
        public VolunteerCoordView VolunteerCoordView
        {
            get { return _volunteerCoordView; }
            set { _volunteerCoordView = value; OnPropertyChanged(); }
        }
        public TrainingCoordView TrainingCoordView
        {
            get { return _trainingCoordView; }
            set { _trainingCoordView = value; OnPropertyChanged(); }
        }
        public NotificationsCoordView NotificationsCoordView
        {
            get { return _notificationsCoordView; }
            set { _notificationsCoordView = value; OnPropertyChanged(); }
        }
        public AssignmentCoordView AssignmentCoordView
        {
            get { return _assignmentCoordView; }
            set { _assignmentCoordView = value; OnPropertyChanged(); }
        }
        public ProjectVolView ProjectsCoordView
        {
            get { return _projectsCoordView; }
            set { _projectsCoordView = value; OnPropertyChanged(); }
        }
        #endregion
        #region Other Properties
        public object FrameContent
        {
            get { return _frameContent; }
            set { _frameContent = value; OnPropertyChanged(); }
        }
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }
        #endregion
        #region Navigation Commands

        public DelegateCommand AddCoordinator => new DelegateCommand(() => {
            _navigationService.Navigate(typeof(AddCoordinatorPage),_connectedUserJson);
        });
        public DelegateCommand GoToSchoolTeachersPage => new DelegateCommand(() => {
            _navigationService.Navigate(typeof(SchoolsAndTeachersPage), _connectedUserJson);
        });

        #endregion
        #region TabNavigationCommands
        public DelegateCommand GoToReportsCoordView => new DelegateCommand(() =>
        {
            FrameContent = _reportCoordView;
            _title = "Reports";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand GoToProjectsCoordView => new DelegateCommand(() =>
        {
            FrameContent = _projectsCoordView;
            _title = "Projects";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand GoToReportVolView => new DelegateCommand(() =>
        {
            FrameContent = _reportCoordView;
            _title = "Reports";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand GoToTrainingsCoordView => new DelegateCommand(() =>
        {
            FrameContent = _trainingCoordView;
            _title = "Trainings";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand GoToVolunteersCoordView => new DelegateCommand(() =>
        {
            FrameContent = _volunteerCoordView;
            _title = "Volunteers";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand GoToAssignmentCoordView => new DelegateCommand(() =>
        {
            FrameContent = _assignmentCoordView;
            _title = "Assignments";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(Title));
        });
        public DelegateCommand GoToNotificationsCoordView => new DelegateCommand(() =>
        {
            FrameContent = _notificationsCoordView;
            _title = "Notifications";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(Title));
        });
        #endregion
        #region Methods
        public async Task LoadAsync(BaseEntityDto<Coordinator> coordinator)
        {
            ProggressBarVisible(true);
            _connectedUserAuth = coordinator.ConnectedUser;
            _connectedUser = _connectedUserAuth;
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
            _connectedUserJson = JsonConvert.SerializeObject(_connectedUser, Formatting.Indented);
            if (_connectedUser.Role == "Admin")
            {
                AddCoordCommandVisibility = Visibility.Visible;
            }
            else
            {
                AddCoordCommandVisibility = Visibility.Collapsed;
            }
            _reportCoordView = new ReportsVolView(_connectedUserAuth);
            OnPropertyChanged(nameof(AddCoordCommandVisibility));
            List<Volunteer> getVolunteers = await LoadItems();
            _volunteerCoordView = new VolunteerCoordView(_connectedUser, _volunteers,_schools);
            _projectsCoordView = new ProjectVolView(_connectedUser, _volunteers,_schools);
            ProggressBarVisible(false);

            _volunteersDto = await LoadVolunteerDtosAsync(getVolunteers);
            _volunteerCoordView = new VolunteerCoordView(_volunteersDto,_schools);
            var projects = await LoadProjectsAsync(_volunteers);
            _projectsCoordView = new ProjectVolView(projects,_volunteers,_schools);
        }

        private async Task<List<BaseEntityDto<Volunteer>>> LoadVolunteerDtosAsync(List<Volunteer> volunteers)
        {
            var volunteersDto = new List<BaseEntityDto<Volunteer>>();
            foreach (var volunteer in volunteers)
            {
                var volunteerDto = await DtoHelper.LoadDto(volunteer, _httpClientService, "volunteer");
                volunteerDto.ConnectedUser = _connectedUserAuth;
                volunteersDto.Add(volunteerDto);
            }
            return volunteersDto;
        }

        private async Task<List<Volunteer>> LoadItems()
        {
            var getSchools = await _httpClientService.GetListTAsync<School>("school/getall");
            var getSchoolsForCoordinator = _connectedUser.Role == "Admin" ? getSchools : getSchools.Where(s => s.CoordinatorId == _connectedUser.UserId).ToList();
            _schools = getSchoolsForCoordinator;
            List<Volunteer> vols = new List<Volunteer>();
            foreach (var school in getSchoolsForCoordinator)
            {
                var volunteers = await _httpClientService.GetListTAsync<Volunteer>($"school/getvolunteersfromschool/{school.Id}");
                vols.AddRange(volunteers);
            }
            _volunteers = vols.GroupBy(x => new { x.Id }).Select(g => g.First()).ToList();
            return _volunteers;
        }
        private async Task<List<ProjectDto>> LoadProjectsAsync(List<Volunteer> volunteers)
        {

            return await EntityLoadingHelper.LoadProjectsAsync(_volunteers, _httpClientService, new List<ProjectDto>(), _connectedUserAuth);
        }



        #endregion
    }
}
