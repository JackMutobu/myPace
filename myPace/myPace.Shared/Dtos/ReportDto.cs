using myPace.Core.Entities;
using myPace.ViewModels;
using System;
using System.Linq;
using Windows.UI.Xaml;

namespace myPace.Shared.Dtos
{
    public class ReportDto:ViewModelBase
    {
        private Visibility _signInVisibility;
        private Visibility _signOutVisibility;
        private Visibility _textTimeVisibility;
        private Visibility _commandSectionVisibility;
        private Visibility _signOutBtnVisibility;

        public Report Report { get;}
        public string SchoolName { get; }
        public string VolunteerNames { get; }
        public TimeSpan TotalHours { get; }
        public Visibility SignInVisibility
        {
            get { return _signInVisibility; }
            set
            {
                SetProperty(ref _signInVisibility, value);
            }
        }
        public Visibility SignOutVisibility
        {
            get { return _signOutVisibility; }
            set
            {
                SetProperty(ref _signOutVisibility, value);
            }
        }
        public Visibility TextTimeVisibility
        {
            get { return _textTimeVisibility; }
            set
            {
                SetProperty(ref _textTimeVisibility, value);
            }
        }
        public Visibility CommandSectionVisibility
        {
            get { return _commandSectionVisibility; }
            set
            {
                SetProperty(ref _commandSectionVisibility, value);
            }
        }
        public Visibility SignOutBtnVisibility
        {
            get { return _signOutBtnVisibility; }
            set
            {
                SetProperty(ref _signOutBtnVisibility, value);
            }
        }
        public ReportDto(Report report)
        {
            Report = report;
            if(report.SignOutDate != null)
            {
                TotalHours = (TimeSpan)(report.SignOutDate - report.SignInDate);
            }
            InitializeVisibility();
        }
        public ReportDto(Report report,Volunteer volunteer,School school)
        {
            Report = report;
            InitializeVisibility();
            VolunteerNames = $"{volunteer.FirstName} {volunteer.MiddleName} {volunteer.LastName}";
            SchoolName = school.Name;
        }

        private void InitializeVisibility()
        {
            _signInVisibility = Visibility.Collapsed;
            _signOutBtnVisibility = Visibility.Visible;
            _signOutVisibility = Visibility.Collapsed;
            _commandSectionVisibility = Visibility.Visible;
            _textTimeVisibility = Visibility.Visible;
        }
        public void RaiseVisibilityChange()
        {
            RaisePropertyChanged(nameof(SignInVisibility));
            RaisePropertyChanged(nameof(SignOutBtnVisibility));
            RaisePropertyChanged(nameof(SignOutVisibility));
        }
    }
}
