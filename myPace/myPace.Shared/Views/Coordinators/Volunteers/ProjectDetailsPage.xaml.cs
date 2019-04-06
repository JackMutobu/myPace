using Autofac;
using myPace.Core.Dtos;
using myPace.Shared.Dtos;
using myPace.ViewModels;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views.Volunteers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectDetailsPage : Page
    {
        public ProjectDetailsPage()
        {
            this.InitializeComponent();
            ProjectDetailsViewModel = App.Container.Resolve<ProjectDetailsViewModel>();
            DataContext = ProjectDetailsViewModel;
        }

        public ProjectDetailsViewModel ProjectDetailsViewModel { get; private set; }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var projectDto = JsonConvert.DeserializeObject<ProjectDto>(e.Parameter.ToString());
            await ProjectDetailsViewModel.LoadAsync(projectDto);
        }
    }
}
