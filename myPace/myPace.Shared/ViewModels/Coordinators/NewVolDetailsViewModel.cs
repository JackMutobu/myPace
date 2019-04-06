using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
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
    public class NewVolDetailsViewModel : ViewModelBase
    {
        private Visibility _scheduleInterviewVisibility;
        private BaseEntityDto<Volunteer> _volunteerDto;
        private ObservableCollection<SchoolDto> _schools;
        private UserAuthDto _connectedUser;
        private List<School> _closestsSchool;
        private InterviewModel _interview;
        private Visibility _interviewVisibility;

        public NewVolDetailsViewModel(INavigationService navigationService, IHttpClientService httpClientService) : base(navigationService,httpClientService)
        {
            _scheduleInterviewVisibility = Visibility.Collapsed;
            _interviewVisibility = Visibility.Collapsed;
            _schools = new ObservableCollection<SchoolDto>();
            _interview = new InterviewModel();
            _interview.Date = DateTime.Now;
        }
        public BaseEntityDto<Volunteer> Volunteer
        {
            get { return _volunteerDto; }
            set { SetProperty(ref _volunteerDto, value); }
        }
        public InterviewModel Interview
        {
            get { return _interview; }
            set { SetProperty(ref _interview, value); }
        }
        public Visibility InterviewVisibility
        {
            get { return _interviewVisibility; }
            set { SetProperty(ref _interviewVisibility, value); }
        }
        public class InterviewModel
        {
            public string Location { get; set; }
            public DateTimeOffset Date { get; set; }
            public string Time { get; set; }
        }        
        public ObservableCollection<SchoolDto> ClosestsSchools
        {
            get { return _schools; }
            set { SetProperty(ref _schools, value); }
        }
       
        public DelegateCommand ScheduleInterviewCommandExec => new DelegateCommand(async () =>
        {
            DisableButtons();
            var entity = Volunteer.Entity;
            try
            {
                var time = _interview.Time?.Split(":");
                var date = _interview.Date.DateTime;
                entity.Interview = new Interview
                {
                    Location = _interview.Location,
                    Date = _interview.Date.DateTime,
                    Status = Core.Helper.InterviewStatus.Upcoming,
                    From = new DateTime(date.Year, date.Month, date.Day, int.Parse(time[0]), int.Parse(time[1]), 0),
                    UploadDate = DateTime.Now
                };
                entity.Status = Core.Helper.StatusEnum.Interview;
                var result = await _httpClientService.PutBasicAsync(entity, $"volunteer/scheduleInterview/{entity.Id}");
                if (result.IsSuccessStatusCode)
                {
                    _scheduleInterviewVisibility = _scheduleInterviewVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    _volunteerDto.Entity.Status = Core.Helper.StatusEnum.Interview;
                    RaisePropertyChanged(nameof(ScheduleInterviewVisibility));
                    RaisePropertyChanged(nameof(Interview));
                    RaisePropertyChanged(nameof(Volunteer));
                    EnableButtons();
                    return;
                }
                MessageDialog messageDialog = new MessageDialog("An error occured: " + await result.Content.ReadAsStringAsync());
                await messageDialog.ShowAsync();
            }
            catch(Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("An error occured: " + ex.Message);
                await messageDialog.ShowAsync();
            }
            EnableButtons();
        });
        public DelegateCommand ScheduleInterviewCommand => new DelegateCommand(() =>
        {
            _scheduleInterviewVisibility = _scheduleInterviewVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            RaisePropertyChanged(nameof(ScheduleInterviewVisibility));
        });
        public DelegateCommand ConfrimRegistrationCOmmand => new DelegateCommand(async () =>
        {
            DisableButtons();
            try
            {
                var checkedSchools = _schools.Where(x => x.IsChecked == true);
                if(checkedSchools.Count() == 0 || checkedSchools.Count() > 1)
                {
                    MessageDialog dialog = new MessageDialog("Kindly select one school that will be assigned to the volunteer");
                    await dialog.ShowAsync();
                    EnableButtons();
                    return;
                }
                _volunteerDto.Entity.Interview = null;
                _volunteerDto.Entity.SchoolVolunteers.FirstOrDefault(x=> x.SchoolId == checkedSchools.First().School.Id).IsActual = true;
               
                
            _volunteerDto.Entity.Status = Core.Helper.StatusEnum.Active;
            var result = await _httpClientService.PutBasicAsync(_volunteerDto.Entity, $"volunteer/confirmRegistration/{_volunteerDto.Entity.Id}");
            if (result.IsSuccessStatusCode)
            {
                    MessageDialog d = new MessageDialog("Done");
                    await d.ShowAsync();
                    EnableButtons();
                return;
            }
            MessageDialog messageDialog = new MessageDialog("An error occured: " + await result.Content.ReadAsStringAsync());
            await messageDialog.ShowAsync();
        }
            catch(Exception ex)
            {
                MessageDialog messageDialog = new MessageDialog("An error occured: " + ex.Message);
                await messageDialog.ShowAsync();
            }
            EnableButtons();
        });
        public Visibility ScheduleInterviewVisibility
        {
            get { return _scheduleInterviewVisibility; }
            set { SetProperty(ref _scheduleInterviewVisibility, value); }
        }

        public async Task LoadAsync(BaseEntityDto<Volunteer> volunteer)
        {
            _volunteerDto = volunteer;
            _connectedUser = volunteer.ConnectedUser;
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
        
            var getMedia = await _httpClientService.GetListTAsync<Media>($"volunteer/getmedia?id={_volunteerDto.Entity.Id}");
            var latestProfileMedia = getMedia.Where(x => x.Type == Core.Helper.MediaTypeEnum.Photo).OrderByDescending(x => x.UploadDate).FirstOrDefault();
            var path = await FileHelpers.GetFilePath(_httpClientService, "volunteer", _volunteerDto.Entity, latestProfileMedia);
            _volunteerDto.ProfileImage = path == null ? null : new BitmapImage(new Uri(path ?? ""));

            _closestsSchool = await _httpClientService.GetListTAsync<School>($"school/getschoolsfromvolunteer/{_volunteerDto.Entity.Id}");
            foreach(var school in _closestsSchool)
            {
                if (!_schools.Any(x => x.School.Id == school.Id))
                {
                    _schools.Add(new SchoolDto(school));
                }
            }
            _volunteerDto.Age = (DateTime.Now - _volunteerDto.Entity.DOB.DateTime).Days / 365;
            if(_volunteerDto.Entity.Status == Core.Helper.StatusEnum.Interview)
            {
                var getInterview = await _httpClientService.GetTAsync<Interview>($"volunteer/getInterview/{_volunteerDto.Entity.Id}");
                _interview = new InterviewModel
                {
                    Location = getInterview.Location,
                    Date = getInterview.Date,
                    Time = getInterview.From.ToShortTimeString()
                };
                _interviewVisibility = Visibility.Visible;
                RaisePropertyChanged(nameof(InterviewVisibility));
            }
            RaisePropertyChanged(nameof(Volunteer));
            RaisePropertyChanged(nameof(ClosestsSchools));
            RaisePropertyChanged(nameof(Interview));
        }

    }
}
