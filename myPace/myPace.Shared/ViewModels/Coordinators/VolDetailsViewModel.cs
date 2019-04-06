using myPace.Controls.Coordinators.Volunteers;
using myPace.Controls.Volunteers;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace myPace.ViewModels.Coordinators
{
    public class VolDetailsViewModel: ViewModelBase
    {
        #region Private memebers TabView
        private object _frameContent;
        private GeneralInfoView _generalInfoView;
        private ProjectVolView _projectVolView;
        #endregion
        private BaseEntityDto<Volunteer> _volunteerDto;
        private UserAuthDto _connectedUser;

        public VolDetailsViewModel(INavigationService navigationService,IHttpClientService httpClientService) : base(navigationService,httpClientService)
        {
            _generalInfoView = new GeneralInfoView();
            _projectVolView = new ProjectVolView();
        }
        public object FrameContent
        {
            get { return _frameContent; }
            set { SetProperty(ref _frameContent, value); }
        }
        #region Commands
        public DelegateCommand GoToOverviewPage => new DelegateCommand(() =>
        {
            _frameContent = _generalInfoView;
            _viewTitle = "General Info";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(ViewTitle));
        });
        public DelegateCommand GoToProjectPage => new DelegateCommand(() =>
        {
            _frameContent = _projectVolView;
            _viewTitle = "Projects";
            OnPropertyChanged(nameof(FrameContent));
            OnPropertyChanged(nameof(ViewTitle));
        });
        #endregion
        public void Load(BaseEntityDto<Volunteer> volunteerDto)
        {
            _volunteerDto = volunteerDto;
            _connectedUser = volunteerDto.ConnectedUser;
            _generalInfoView = new GeneralInfoView(_volunteerDto);
            _projectVolView = new ProjectVolView(_volunteerDto);
            _frameContent = _generalInfoView;
            RaisePropertyChanged(nameof(FrameContent));

        }
    }
}
