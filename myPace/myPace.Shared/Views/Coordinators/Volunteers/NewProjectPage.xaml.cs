using Autofac;
using myPace.Core.Dtos;
using myPace.Shared.Dtos;
using myPace.ViewModels.Volunteers;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views.Volunteers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewProjectPage : Page
    {
        public NewProjectPage()
        {
            this.InitializeComponent();
            NewProjectViewModel = App.Container.Resolve<NewProjectViewModel>();
            DataContext = NewProjectViewModel;
        }
        public NewProjectViewModel NewProjectViewModel { get; private set; }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var projectDto = JsonConvert.DeserializeObject<ProjectDto>(e.Parameter.ToString());
            await NewProjectViewModel.LoadAsync(projectDto);
        }
    }
}
