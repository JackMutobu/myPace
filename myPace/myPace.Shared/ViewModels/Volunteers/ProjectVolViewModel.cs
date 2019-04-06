using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Filtering;
using myPace.Core.Helper;
using myPace.Core.Helpers;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Enums;
using myPace.Shared.Helpers;
using myPace.Views.Volunteers;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace myPace.ViewModels.Volunteers
{
    public class ProjectVolViewModel:ViewModelBase
    {
        private List<ProjectDto> _allProjects;
        #region Private variables
        private ObservableCollection<ProjectDto> _projects;
        private List<BaseEntityDto<Volunteer>> _volunteersDto;
        private List<Volunteer> _volunteers;
        private Visibility _selectMainTypeVisibility;
        private Visibility _selectSecondTypeVisibility;
        private Visibility _dateVisibility;
        private Visibility _searchTextVisibility;
        private SelectProjectMainTypeEnum _selectedMainType;
        private readonly IProjectFilterByVolunteerSpecification _projectFilterByVolunteerSpecification;
        private readonly IProjectFilterByStatusOrTypeSpecification _projectFilterByStatusOrTypeSpecification;
        private List<object> _selectSecondTypeList;
        private Visibility _statusEnumVisibility;
        private ProjectStatusEnum _selectedStatus;
        private DateTimeOffset _selectedDate;
        private readonly IDateFilterSpecification<Project> _dateFilterSpecification;
        private List<BaseEntityDto<School>> _schools;
        private object _selectedSecondType;
        private string _searchString;
        #endregion

        #region Constructors
        public ProjectVolViewModel(INavigationService navigationService, IHttpClientService httpClientService,IDateFilterSpecification<Project> dateFilterSpecification, IProjectFilterByVolunteerSpecification projectFilterByVolunteerSpecification, IProjectFilterByStatusOrTypeSpecification projectFilterByStatusOrTypeSpecification) :base(navigationService,httpClientService)
        {
            _allProjects = new List<ProjectDto>();
            _projects = new ObservableCollection<ProjectDto>();
            _schools = new List<BaseEntityDto<School>>();
            _volunteersDto = new List<BaseEntityDto<Volunteer>>();
            _selectedDate = DateTimeOffset.Now;
            _dateFilterSpecification = dateFilterSpecification;
            _selectedMainType = SelectProjectMainTypeEnum.All;
            _projectFilterByVolunteerSpecification = projectFilterByVolunteerSpecification;
            _projectFilterByStatusOrTypeSpecification = projectFilterByStatusOrTypeSpecification;
            _selectSecondTypeVisibility = _dateVisibility = _searchTextVisibility = _statusEnumVisibility = _selectSecondTypeVisibility = Visibility.Collapsed;
        }
        #endregion
        #region Properties
        public ObservableCollection<ProjectDto> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects,value); }
        }
        public List<SelectProjectMainTypeEnum> SelectProjectMainTypeEnums => Enum.GetValues(typeof(SelectProjectMainTypeEnum)).Cast<SelectProjectMainTypeEnum>().ToList();
        public List<ProjectStatusEnum> StatusEnums => Enum.GetValues(typeof(ProjectStatusEnum)).Cast<ProjectStatusEnum>().ToList();
        public List<object> SelectSecondTypeList
        {
            get { return _selectSecondTypeList; }
            set
            {
                SetProperty(ref _selectSecondTypeList, value);
            }

        }
        public SelectProjectMainTypeEnum SelectedMainType
        {
            get { return _selectedMainType; }
            set
            {
                SetProperty(ref _selectedMainType, value);
                OnSelectedMainType();
            }
        }
        public ProjectStatusEnum SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                SetProperty(ref _selectedStatus, value);
                var projects = _projectFilterByStatusOrTypeSpecification.FilterByStatus(_allProjects, _selectedStatus);
                _projects = new ObservableCollection<ProjectDto>(projects);
                RaisePropertyChanged(nameof(Projects));
            }
        }
        public DateTimeOffset SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                SetProperty(ref _selectedDate, value);
                var projects = _dateFilterSpecification.Filter(DtoHelper.MapProjectsDtoToProjects(_allProjects).ToList(), _selectedDate);
                _projects = new ObservableCollection<ProjectDto>(DtoHelper.MapProjectsToProjectDto(projects.ToList()));
                RaisePropertyChanged(nameof(Projects));
            }
        }
        public object SelectedSecondType
        {
            get { return _selectedSecondType; }
            set
            {
                SetProperty(ref _selectedSecondType, value);
                OnSecondTypeSelected();
            }
        }
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                SetProperty(ref _searchString, value);
                var projects = _allProjects.Where(p => p.Project.Name.Contains(_searchString, StringComparison.InvariantCultureIgnoreCase));
                _projects = new ObservableCollection<ProjectDto>(projects);
                RaisePropertyChanged(nameof(Projects));
            }
        }

        private void OnSecondTypeSelected()
        {
            if(_selectedSecondType is BaseEntityDto<School> school)
            {
                var projects = _projectFilterByVolunteerSpecification.FilterByVolunteers(DtoHelper.MapProjectsDtoToProjects(_allProjects).ToList(), GetVolunteersFromSchoolId(school.Entity.Id,DtoHelper.MapBaseEntityDtosToBaseEntities(_volunteersDto)).ToList());
                _projects = new ObservableCollection<ProjectDto>(DtoHelper.MapProjectsToProjectDto(projects.Distinct().ToList()));
                RaisePropertyChanged(nameof(Projects));
            }
            else
            {
               var vol = _selectedSecondType is BaseEntityDto<Volunteer> volunteer;
               
                if(vol)
                {
                    volunteer = (BaseEntityDto<Volunteer>)_selectedSecondType;
                    var projects = _projectFilterByVolunteerSpecification.Filter(DtoHelper.MapProjectsDtoToProjects(_allProjects).ToList(), volunteer.Entity);
                    _projects = new ObservableCollection<ProjectDto>(DtoHelper.MapProjectsToProjectDto(projects.Distinct().ToList()));
                    RaisePropertyChanged(nameof(Projects));
                }

            }
            IEnumerable<Volunteer> GetVolunteersFromSchoolId(int schoolId, IEnumerable<Volunteer> volunteers)
            {
                foreach(var volunteer in volunteers)
                {
                    foreach(var sv in volunteer.SchoolVolunteers)
                    {
                        if(sv.SchoolId == schoolId && sv.IsActual)
                        {
                            yield return volunteer;
                            break;
                        }
                    }
                }
            }
        }
        #region Visibility Props
        public Visibility SelectMainTypeVisibility
        {
            get { return _selectMainTypeVisibility; }
            set { SetProperty(ref _selectMainTypeVisibility, value); }
        }
        public Visibility SelectSecondTypeVisibility
        {
            get { return _selectSecondTypeVisibility; }
            set { SetProperty(ref _selectSecondTypeVisibility, value); }
        }
        public Visibility DateVisibility
        {
            get { return _dateVisibility; }
            set { SetProperty(ref _dateVisibility, value); }
        }
        public Visibility SearchTextVisibility
        {
            get { return _searchTextVisibility; }
            set { SetProperty(ref _searchTextVisibility, value); }
        }
        public Visibility StatusEnumVisibility
        {
            get { return _statusEnumVisibility; }
            set
            {
                SetProperty(ref _statusEnumVisibility, value);
            }
        }
        #endregion
        #endregion
        #region Methodes
        public async Task LoadAsync(BaseEntityDto<Volunteer> volunteer)
        {
            try
            {
                _volunteersDto = new List<BaseEntityDto<Volunteer>>()
                {
                    new BaseEntityDto<Volunteer>(volunteer.Entity)
                };
                var school = await _httpClientService.GetTAsync<School>($"school/getbyid/{volunteer.Entity.SchoolVolunteers.First(s => s.IsActual == true)}");
                _schools = new List<BaseEntityDto<School>>
                {
                    new BaseEntityDto<School>(school)
                };
                ProggressBarVisible(true);
                _connectedUserAuth = volunteer.ConnectedUser;
                _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUserAuth.Token);
                _projects = new ObservableCollection<ProjectDto>(await EntityLoadingHelper.LoadProjectsAsync(new List<Volunteer> { volunteer.Entity }, _httpClientService, _projects.ToList()));
                _allProjects = _projects.ToList();
                RaisePropertyChanged(nameof(Projects));
            }
            catch (Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("An error occured: " + ex.Message);
                await messageDialog.ShowAsync();
            }
            finally
            {
                ProggressBarVisible(false);
            }
        }

        internal async Task LoadAsync(List<ProjectDto> projects, List<Volunteer> volunteers,List<School> schools)
        {
            try
            {
                foreach(var volunteer in volunteers)
                {
                    _volunteersDto.Add(new BaseEntityDto<Volunteer>(volunteer));
                }
                foreach (var school in schools)
                {
                    _schools.Add(new BaseEntityDto<School>(school));
                }
                ProggressBarVisible(false);
                _connectedUserAuth = projects.FirstOrDefault().ConnectedUser;
                _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUserAuth.Token);
                _projects = new ObservableCollection<ProjectDto>(projects);
                _allProjects = _projects.ToList() ;
                RaisePropertyChanged(nameof(Projects));
                RaisePropertyChanged(nameof(SelectedMainType));
            }
            catch(Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("An error occured: " + ex.Message);
                await messageDialog.ShowAsync();
            }
           
        }

        internal async Task LoadAsync(UserAuthDto connectedUser, List<Volunteer> volunteers,List<School> schools)
        {
            try
            {
                foreach (var volunteer in volunteers)
                {
                    _volunteersDto.Add(new BaseEntityDto<Volunteer>(volunteer));
                }
                foreach (var school in schools)
                {
                    _schools.Add(new BaseEntityDto<School>(school));
                }
                ProggressBarVisible(true);
                _connectedUserAuth = connectedUser;
                _volunteers = volunteers;
                _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUserAuth.Token);

                foreach (var value in await EntityLoadingHelper.LoadProjectsAsync(_volunteers.Take(1).ToList(), _httpClientService, _projects.ToList()))
                {
                    _projects.Add(value);
                    _allProjects.Add(value);
                }
                RaisePropertyChanged(nameof(Projects));
                RaisePropertyChanged(nameof(SelectedMainType));
                ProggressBarVisible(false);
                await LoadRemainingProjects(1);
            }
            catch(Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("An error occured: " + ex.Message);
                await messageDialog.ShowAsync();
            }
            
        }

        private async Task LoadRemainingProjects(int numberOfItemsToLoad)
        {
            var loadedProjects = numberOfItemsToLoad;
            foreach (var value in await EntityLoadingHelper.LoadProjectsAsync(_volunteers.Skip(loadedProjects).Take(numberOfItemsToLoad).ToList(),_httpClientService,_projects.ToList()))
            {
                var selectedProject = _projects.FirstOrDefault(p => p.Project.Id == value.Project.Id);
                if (selectedProject == null)
                {
                    _projects.Add(value);
                    _allProjects.Add(value);
                    RaisePropertyChanged(nameof(Projects));
                }
            }
        }

        private void OnSelectedMainType()
        {
            switch(_selectedMainType)
            {
                case SelectProjectMainTypeEnum.All:
                    SearchBarVisibility(Visibility.Visible);
                    _projects = new ObservableCollection<ProjectDto>(_allProjects);
                    RaiseVisibilityChangedProperties();
                    RaisePropertyChanged(nameof(Projects));
                    break;
                case SelectProjectMainTypeEnum.Date:
                    _dateVisibility = SearchBarVisibility(Visibility.Visible);
                    break;
                case SelectProjectMainTypeEnum.School:
                    _selectSecondTypeVisibility = SearchBarVisibility(Visibility.Visible);
                    _selectSecondTypeList = (_schools as IEnumerable<object>).Cast<object>().Distinct().ToList();
                    RaisePropertyChanged(nameof(SelectSecondTypeList));
                    break;
                case SelectProjectMainTypeEnum.Volunteer:
                    _selectSecondTypeVisibility = SearchBarVisibility(Visibility.Visible);
                    _selectSecondTypeList = (_volunteersDto as IEnumerable<object>).Cast<object>().Distinct().ToList();
                    RaisePropertyChanged(nameof(SelectSecondTypeList));
                    break;
                case SelectProjectMainTypeEnum.Status:
                    _statusEnumVisibility = SearchBarVisibility(Visibility.Visible);
                    break;
            }
            RaiseVisibilityChangedProperties();
        }

        private Visibility SearchBarVisibility(Visibility selectMainTypeVisibility)
        {
            _selectSecondTypeVisibility = Visibility.Collapsed;
            _dateVisibility = Visibility.Collapsed;
            _searchTextVisibility = Visibility.Collapsed;
            _statusEnumVisibility = Visibility.Collapsed;
            _selectMainTypeVisibility = selectMainTypeVisibility;
            return Visibility.Visible;
        }
        private void RaiseVisibilityChangedProperties()
        {
            RaisePropertyChanged(nameof(SelectMainTypeVisibility));
            RaisePropertyChanged(nameof(SelectSecondTypeVisibility));
            RaisePropertyChanged(nameof(SearchTextVisibility));
            RaisePropertyChanged(nameof(DateVisibility));
            RaisePropertyChanged(nameof(StatusEnumVisibility));
        }
        #endregion


        #region Commands
        public DelegateCommand<object> GoToDetailsPage => new DelegateCommand<object>(param =>
        {
            var id = int.Parse(param.ToString());
            var projectDto = _projects.SingleOrDefault(p => p.Project.Id == id);
            projectDto.CanEdit = true;
            projectDto.ConnectedUser = _connectedUserAuth;
            
            var projectDtoJson = JsonConvert.SerializeObject(projectDto);
            _navigationService.Navigate(typeof(ProjectDetailsPage),projectDtoJson);
        });
        public DelegateCommand SearchCommand => new DelegateCommand(() =>
        {
            _searchTextVisibility = _searchTextVisibility == Visibility.Collapsed ? SearchBarVisibility(Visibility.Collapsed) : Visibility.Collapsed;
            if(_searchTextVisibility == Visibility.Collapsed)
            {
                SearchBarVisibility(Visibility.Visible);
            }
            RaiseVisibilityChangedProperties();
        });
        #endregion
    }
}
