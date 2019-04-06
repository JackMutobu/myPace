using myPace.Core.Dtos;
using Prism.Commands;
using myPace.Interfaces;
using System.Collections.Generic;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using myPace.Shared.Dtos;
using System;
using Windows.UI.Popups;
using myPace.Core.Entities;
using System.Linq;

namespace myPace.ViewModels
{
    public class ReportsViewModel:ViewModelBase
    {
        private ObservableCollection<ReportDto> _reportsDto;
        private Visibility _signInButtonVisibility;
        private TimeSpan _selectedTime;
        private List<string> _hours;
        private List<string> _minutes;
        private List<string> _timeType;
        private string _selectedHour;
        private string _selectedMinute;
        private string _selectedType;
        private string _signType;
        private bool isSignedIn = false;
        private Report report;
        private Visibility _commandSectionVisibility;

        public ReportsViewModel(INavigationService navigationService, IHttpClientService httpClientService):base(navigationService,httpClientService)
        {
            SetClock();
            _selectedHour = _hours[0];
            _selectedMinute = _minutes[0];
            _selectedType = _timeType[0];
            _signType = "SignIn Time";
            _reportsDto = new ObservableCollection<ReportDto>();
            _signInButtonVisibility = _commandSectionVisibility =  Visibility.Collapsed;
        }

        private void SetClock()
        {
            _hours = new List<string>();
            _minutes = new List<string>();
            _timeType = new List<string>()
            {
                "AM",
                "PM"
            };
            for (int i = 0; i <= 59; i++)
            {
                if (i <= 12)
                {
                    if (i <= 9)
                    {
                        _hours.Add($"{0}{i}");
                        _minutes.Add($"{0}{i}");
                    }
                    else
                    {
                        _hours.Add($"{i}");
                        _minutes.Add($"{i}");
                    }
                }
                else
                {
                    _minutes.Add($"{i}");
                }


            }
        }

        public Visibility SignInButtonVisibility
        {
            get { return _signInButtonVisibility; }
            set { SetProperty(ref _signInButtonVisibility, value); }
        }
        public Visibility CommandSectionVisibility
        {
            get { return _commandSectionVisibility; }
            set { SetProperty(ref _commandSectionVisibility, value); }
        }
        public ObservableCollection<ReportDto> ReportDtos
        {
            get { return _reportsDto; }
            set
            {
                SetProperty(ref _reportsDto, value);
            }
        }
        public List<string> Hours
        {
            get { return _hours; }
            set
            {
                SetProperty(ref _hours, value);
            }
        }
        public List<string> Minutes
        {
            get { return _minutes; }
            set
            {
                SetProperty(ref _minutes, value);
            }
        }
        public List<string> TimeType
        {
            get { return _timeType; }
            set
            {
                SetProperty(ref _timeType, value);
            }
        }
        public string SelectedHour
        {
            get { return _selectedHour; }
            set { SetProperty(ref _selectedHour, value); }
        }
        public string SelectedMinute
        {
            get { return _selectedMinute; }
            set { SetProperty(ref _selectedMinute, value); }
        }
        public string SelectedType
        {
            get { return _selectedType; }
            set { SetProperty(ref _selectedType, value); }
        }
        public string SingType
        {
            get { return _signType; }
            set { SetProperty(ref _signType, value); }
        }
        public DelegateCommand SignInCommand => new DelegateCommand(() =>
        {
            _commandSectionVisibility = Visibility.Visible;
            RaisePropertyChanged(nameof(CommandSectionVisibility));

        });
        public DelegateCommand SaveTimeCommand => new DelegateCommand( () =>
        {
            _selectedTime = new TimeSpan(_selectedType == "AM" ? int.Parse(_selectedHour) : 12 + int.Parse(_selectedHour.Trim()), int.Parse(_selectedMinute), 0);
            if (isSignedIn == false)
            {
                _signType = "SignOut Time";               
                report = new Report()
                {
                    SignInDate = _selectedTime,
                    SubmitedSignInDate = DateTime.Now,
                    UploadDate = DateTime.Now,
                    VolunteerId = _connectedUserAuth.UserId
                };
                var reportDto = new ReportDto(report)
                {
                    SignInVisibility = Visibility.Visible,
                    SignOutBtnVisibility = Visibility.Visible,
                };
                reportDto.RaiseVisibilityChange();
                _reportsDto.Add(reportDto);
                _signInButtonVisibility = Visibility.Collapsed;
            }
            else
            {
                _signType = "SignIn Time";
                var reportDto = _reportsDto.First(r => r.Report.SignInDate == report.SignInDate && r.Report.SubmitedSignInDate == report.SubmitedSignInDate && r.Report.UploadDate == report.UploadDate);
                var newReport = reportDto.Report;
                newReport.SubmitedSignOutDate = DateTime.Now;
                newReport.SignOutDate = _selectedTime;
                _signInButtonVisibility = Visibility.Visible;

                _reportsDto.Remove(reportDto);
                var newReportDto = new ReportDto(newReport)
                {
                    SignInVisibility = Visibility.Visible,
                    SignOutBtnVisibility = Visibility.Collapsed,
                    SignOutVisibility = Visibility.Visible
                };
                newReportDto.RaiseVisibilityChange();
                _reportsDto.Add(newReportDto);
            }
            isSignedIn = !isSignedIn;
            _commandSectionVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(CommandSectionVisibility));
            RaisePropertyChanged(nameof(ReportDtos));
            RaisePropertyChanged(nameof(SignInButtonVisibility));
            RaisePropertyChanged(nameof(SingType));
        });
        public DelegateCommand SignOutCommand => new DelegateCommand(() =>
        {
            _commandSectionVisibility = Visibility.Visible;
            RaisePropertyChanged(nameof(CommandSectionVisibility));
        });
        

        internal void LoadAsync(UserAuthDto userAuthDto)
        {
            _connectedUserAuth = userAuthDto;
            if(_connectedUserAuth.Role == "Vol")
            {
                _signInButtonVisibility = Visibility.Visible;
                RaisePropertyChanged(nameof(SignInButtonVisibility));
            }
        }
    }
}
