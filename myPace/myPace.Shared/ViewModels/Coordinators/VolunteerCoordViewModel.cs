using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Filtering;
using myPace.Core.Helper;
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
using Windows.UI.Xaml.Media.Imaging;

namespace myPace.ViewModels.Coordinators
{
    public class VolunteerCoordViewModel:ViewModelBase
    {
        #region Private variables
        private UserAuthDto _connectedUser;
        private List<Volunteer> _volunteers;
        private List<BaseEntityDto<School>> _schools;
        private List<BaseEntityDto<Volunteer>> _allVolunteersDto;
        private ObservableCollection<BaseEntityDto<Volunteer>> _volunteersDto;
        private readonly IDateFilterSpecification<Volunteer> _dateFilterSpecification;
        private List<object> _selectSecondTypeList;
        private SelectVolunteerMainTypeEnum _selectedMainType;
        private readonly IVolunteerFilterBySchoolSpecification _volunteerFilterBySchoolSpecification;
        private readonly IVolunteerFilterByStatusSpecification _volunteerFilterByStatusSpecification;
        private object _selectedSecondType;
        private DateTimeOffset _selectedDate;
        private Visibility _selectMainTypeVisibility;
        private Visibility _selectSecondTypeVisibility;
        private Visibility _dateVisibility;
        private Visibility _searchTextVisibility;
        private Visibility _statusEnumVisibility;
        private StatusEnum _selectedStatus;
        private string _searchString;
        #endregion
        #region Constructor
        public VolunteerCoordViewModel(IHttpClientService httpClientService,INavigationService navigationService,IDateFilterSpecification<Volunteer> dateFilterSpecification, IVolunteerFilterBySchoolSpecification volunteerFilterBySchoolSpecification, IVolunteerFilterByStatusSpecification volunteerFilterByStatusSpecification) : base(navigationService,httpClientService)
        {
            _volunteers = new List<Volunteer>();
            _allVolunteersDto = new List<BaseEntityDto<Volunteer>>();
            _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>();
            _dateFilterSpecification = dateFilterSpecification;
            _schools = new List<BaseEntityDto<School>>();
            _selectedDate = DateTimeOffset.Now;
            _selectedMainType = SelectVolunteerMainTypeEnum.All;
            _volunteerFilterBySchoolSpecification = volunteerFilterBySchoolSpecification;
            _volunteerFilterByStatusSpecification = volunteerFilterByStatusSpecification;
            _selectSecondTypeVisibility = _dateVisibility = _searchTextVisibility = _statusEnumVisibility = _selectSecondTypeVisibility = Visibility.Collapsed;
        }
        #endregion
        #region Properties
        public ObservableCollection<BaseEntityDto<Volunteer>> Volunteers
        {
            get { return _volunteersDto; }
            set { SetProperty(ref _volunteersDto, value); }
        }
        public bool IsFirstLoad { get; set; } = true;
        public List<SelectVolunteerMainTypeEnum> SelectVolunteerMainTypeEnums => Enum.GetValues(typeof(SelectVolunteerMainTypeEnum)).Cast<SelectVolunteerMainTypeEnum>().ToList();
        public List<StatusEnum> StatusEnums => Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>().ToList();
        public List<object> SelectSecondTypeList
        {
            get { return _selectSecondTypeList; }
            set
            {
                SetProperty(ref _selectSecondTypeList, value);
            }

        }
        public SelectVolunteerMainTypeEnum SelectedMainType
        {
            get { return _selectedMainType; }
            set
            {
                SetProperty(ref _selectedMainType, value);
                OnSelectedMainType();
            }
        }
        public DateTimeOffset SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                SetProperty(ref _selectedDate, value);
                var volunteers = _dateFilterSpecification.FilterDto(DtoHelper.MapBaseEntityToBaseEntityMainDto(_allVolunteersDto).ToList(), _selectedDate);
                _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>(DtoHelper.LoadEntityWithImages(volunteers, _allVolunteersDto));
                RaisePropertyChanged(nameof(Volunteers));
            }
        }
        public object SelectedSecondType
        {
            get { return _selectedSecondType; }
            set
            {
                SetProperty(ref _selectedSecondType, value);
                if (_selectedSecondType is BaseEntityDto<School> school)
                {
                    var volunteers = _volunteerFilterBySchoolSpecification.FilterDto(DtoHelper.MapBaseEntityToBaseEntityMainDto(_allVolunteersDto).ToList(), school.Entity);
                    _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>(DtoHelper.LoadEntityWithImages(volunteers.Distinct(),_allVolunteersDto));
                    RaisePropertyChanged(nameof(Volunteers));
                }
            }
        }
        public StatusEnum SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                SetProperty(ref _selectedStatus, value);
                var volunteers = _volunteerFilterByStatusSpecification.FilterDto(DtoHelper.MapBaseEntityToBaseEntityMainDto(_allVolunteersDto.ToList()).ToList(), _selectedStatus);
                _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>(DtoHelper.LoadEntityWithImages(volunteers,_allVolunteersDto.ToList()));
                RaisePropertyChanged(nameof(Volunteers));
            }
        }
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                SetProperty(ref _searchString, value);
                var volunteers = _allVolunteersDto.Where(v => v.Name.Contains(_searchString, StringComparison.InvariantCultureIgnoreCase));
                _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>(volunteers);
                RaisePropertyChanged(nameof(Volunteers));
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
        #region Commands
        public DelegateCommand<object> GoToDetailsPage => new DelegateCommand<object>(param =>
        {
            var id = int.Parse(param.ToString());
            var getVolFromVolDto = _volunteersDto.SingleOrDefault(x => x.Entity.Id == id);
            getVolFromVolDto.ConnectedUser = _connectedUserAuth;
            getVolFromVolDto.ProfileImage = null;
            var getVolFromVolDtoJson = JsonConvert.SerializeObject(getVolFromVolDto);
            if (getVolFromVolDto.Entity.Status == Core.Helper.StatusEnum.Active)
            {
                _navigationService.Navigate(typeof(VolDetailsPage), getVolFromVolDtoJson);
                return;
            }
            _navigationService.Navigate(typeof(NewVolDetailsPage),getVolFromVolDtoJson);
        });
        #endregion
        #region Methodes
        internal async Task LoadAsync(List<BaseEntityDto<Volunteer>> volunteersDto, List<School> schools)
        {
            try
            {
                foreach(var school in schools)
                {
                    _schools.Add(new BaseEntityDto<School>(school)); 
                }
                _connectedUserAuth = volunteersDto.First().ConnectedUser;
                _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUserAuth.Token);
                foreach (var value in volunteersDto)
                {
                    _volunteers.Add(value.Entity);
                }
                _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>(volunteersDto);
                _allVolunteersDto = _volunteersDto.ToList();
                RaisePropertyChanged(nameof(Volunteers));
            }
            catch (Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("An error occured: " + ex.Message);
                await messageDialog.ShowAsync();
            }
        }

        internal async Task LoadAsync(UserAuthDto userAuthDto,List<Volunteer> volunteers, List<School> schools)
        {
            try
            { 
                ProggressBarVisible(true);
                foreach (var school in schools)
                {
                    _schools.Add(new BaseEntityDto<School>(school));
                }
                _volunteers = volunteers;
                _connectedUser = userAuthDto;
                _connectedUserAuth = _connectedUser;
                _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
                foreach (var value in await LoadVolunteersAsync(_volunteers.Take(5).ToList()))
                {
                    _volunteersDto.Add(value);
                    _allVolunteersDto.Add(value);
                }
                RaisePropertyChanged(nameof(Volunteers));
                ProggressBarVisible(false);
                await LoadRemainingVolunteers(5);
            }
            catch(Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("An error occured: " + ex.Message);
                await messageDialog.ShowAsync();
            }
            finally
            {
                ProggressBarVisible(false);
            }
            
        }

        private async Task LoadRemainingVolunteers(int numberofitemsToLoad)
        {
            var loadedVolunteers = numberofitemsToLoad;
            while (loadedVolunteers <= _volunteers.Count)
            {
                foreach (var value in await LoadVolunteersAsync(_volunteers.Skip(loadedVolunteers).Take(numberofitemsToLoad).ToList()))
                {
                    _volunteersDto.Add(value);
                    _allVolunteersDto.Add(value);
                }
                loadedVolunteers += numberofitemsToLoad;
                RaisePropertyChanged(nameof(Volunteers));
            }
        }

        private async Task<List<BaseEntityDto<Volunteer>>> LoadVolunteersAsync(List<Volunteer> volunteers)
        {
            var volunteersDto = new List<BaseEntityDto<Volunteer>>();
            foreach (var volunteer in volunteers)
            {
                BaseEntityDto<Volunteer> volunteerDto = await DtoHelper.LoadDto(volunteer, _httpClientService, "volunteer");
                if (_volunteersDto.Any(v => v.Entity.Id == volunteerDto.Entity.Id))
                    continue;
                volunteersDto.Add(volunteerDto);
            }
            return volunteersDto;
        }
        private void OnSelectedMainType()
        {
            switch (_selectedMainType)
            {
                case SelectVolunteerMainTypeEnum.All:
                    ProggressBarVisible(true);
                    SearchBarVisibility(Visibility.Visible);
                    _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>(_allVolunteersDto);
                    RaisePropertyChanged(nameof(Volunteers));
                    RaiseVisibilityChangedProperties();
                    ProggressBarVisible(false);
                    break;
                case SelectVolunteerMainTypeEnum.Date:
                    _dateVisibility = SearchBarVisibility(Visibility.Visible);
                    break;
                case SelectVolunteerMainTypeEnum.School:
                    _selectSecondTypeVisibility = SearchBarVisibility(Visibility.Visible);
                    _selectSecondTypeList = (_schools as IEnumerable<object>).Cast<object>().Distinct().ToList();
                    RaisePropertyChanged(nameof(SelectSecondTypeList));
                    break;
                case SelectVolunteerMainTypeEnum.Status:
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
        public DelegateCommand SearchCommand => new DelegateCommand(() =>
        {
            _searchTextVisibility = _searchTextVisibility == Visibility.Collapsed ? SearchBarVisibility(Visibility.Collapsed) : Visibility.Collapsed;
            if (_searchTextVisibility == Visibility.Collapsed)
            {
                SearchBarVisibility(Visibility.Visible);
            }
            RaiseVisibilityChangedProperties();
        });
        #endregion
    }
}
