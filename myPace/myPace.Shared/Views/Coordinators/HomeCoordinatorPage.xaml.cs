using Autofac;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using myPace.ViewModels.Coordinators;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views.Coordinators
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeCoordinatorPage : Page
    {

        public HomeCoordViewModel HomeCoordViewModel { get; private set; }
        public HomeCoordinatorPage()
        {
            this.InitializeComponent();
            HomeCoordViewModel = App.Container.Resolve<HomeCoordViewModel>();
            DataContext = HomeCoordViewModel;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var coordinator = JsonConvert.DeserializeObject<BaseEntityDto<Coordinator>>(e.Parameter.ToString());
            await HomeCoordViewModel.LoadAsync(coordinator);
        }
    }
}
