using Autofac;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using myPace.ViewModels.Coordinators;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static myPace.Shared.Helpers.FileHelpers;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace myPace.Views
{
    public sealed partial class VolunteerCoordView : UserControl
    {
        private List<Volunteer> _volunteers;
        private List<School> _schools;
        private UserAuthDto _userAuthDto;
        private List<BaseEntityDto<Volunteer>> _volunteersDto;

        public VolunteerCoordViewModel VolunteerCoordViewModel { get; private set; }
        public VolunteerCoordView(UserAuthDto userAuthDto,List<Volunteer> volunteers, List<School> schools)
        {
            this.InitializeComponent();
            _volunteers = volunteers;
            _schools = schools;
            _userAuthDto = userAuthDto;
            VolunteerCoordViewModel = App.Container.Resolve<VolunteerCoordViewModel>();
            DataContext = VolunteerCoordViewModel;
            this.Loaded += VolunteerCoordView_Loaded;
        }

        public VolunteerCoordView(List<BaseEntityDto<Volunteer>> volunteersDto, List<School> schools)
        {
            this.InitializeComponent();
            _volunteersDto = volunteersDto;
            _schools = schools;
            VolunteerCoordViewModel = App.Container.Resolve<VolunteerCoordViewModel>();
            DataContext = VolunteerCoordViewModel;
            this.Loaded += VolunteerCoordView_Loaded;
        }

        private async void VolunteerCoordView_Loaded(object sender, RoutedEventArgs e)
        {
                if (_volunteersDto != null || _volunteersDto?.Count > 0)
                {
                    await VolunteerCoordViewModel.LoadAsync(_volunteersDto,_schools);
                }
                else
                {
                    await VolunteerCoordViewModel.LoadAsync(_userAuthDto, _volunteers,_schools);
                }
            stack.Visibility = Visibility.Collapsed;
        }
    }
}
