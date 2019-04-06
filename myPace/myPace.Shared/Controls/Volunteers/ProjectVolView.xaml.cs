using System.Collections.Generic;
using Autofac;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Shared.Dtos;
using myPace.ViewModels.Volunteers;
using Windows.UI.Xaml.Controls;
using static myPace.Shared.Helpers.FileHelpers;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace myPace.Controls.Volunteers
{
    public sealed partial class ProjectVolView : UserControl
    {
        private BaseEntityDto<Volunteer> _volunteer;
        private UserAuthDto _connectedUser;
        private List<Volunteer> _volunteers;
        private List<School> _schools;
        private List<ProjectDto> _projects;

        public ProjectVolViewModel ProjectVolViewModel { get; private set; }

        public ProjectVolView()
        {
            this.InitializeComponent();
        }
        public ProjectVolView(BaseEntityDto<Volunteer> volunteer)
        {
            this.InitializeComponent();
            _volunteer = volunteer;
            _connectedUser = volunteer.ConnectedUser;
            ProjectVolViewModel = App.Container.Resolve<ProjectVolViewModel>();
            DataContext = ProjectVolViewModel;
            Loaded += ProjectVolView_Loaded;
        }

        public ProjectVolView(UserAuthDto user, List<Volunteer> volunteers,List<School> schools)
        {
            InitializeComponent();
            _volunteers = volunteers;
            _schools = schools;
            _connectedUser = user;
            ProjectVolViewModel = App.Container.Resolve<ProjectVolViewModel>();
            DataContext = ProjectVolViewModel;
            Loaded += ProjectVolView_Loaded;
        }

        

        public ProjectVolView(List<ProjectDto> projects,List<Volunteer> volunteers,List<School> schools)
        {
            InitializeComponent();
            _projects = projects;
            _volunteers = volunteers;
            _schools = schools;
            ProjectVolViewModel = App.Container.Resolve<ProjectVolViewModel>();
            DataContext = ProjectVolViewModel;
            Loaded += ProjectVolView_Loaded;
        }

        private async void ProjectVolView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_projects != null || _projects?.Count > 0)
            {
                await ProjectVolViewModel.LoadAsync(_projects,_volunteers,_schools);
            }
            else
            {
                if (_connectedUser == null || _volunteers == null)
                {
                    await ProjectVolViewModel.LoadAsync(_volunteer);
                }
                else
                {
                    await ProjectVolViewModel.LoadAsync(_connectedUser, _volunteers,_schools);
                }
            }
        }
    }
}
