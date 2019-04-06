using Autofac;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Shared.Dtos;
using myPace.ViewModels.Volunteers;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static myPace.Shared.Helpers.FileHelpers;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views.Volunteers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeVolunteerPage : Page
    {
        public HomeVolunteerPage()
        {
            this.InitializeComponent();
            HomeVolunteerViewModel = App.Container.Resolve<HomeVolunteerViewModel>();
            DataContext = HomeVolunteerViewModel;
        }

        public HomeVolunteerViewModel HomeVolunteerViewModel { get; private set; }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var volunteerDto = JsonConvert.DeserializeObject<BaseEntityDto<Volunteer>>(e.Parameter.ToString());
            await HomeVolunteerViewModel.LoadAsync(volunteerDto);

        }
    }
}
