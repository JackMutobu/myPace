using Autofac;
using myPace.Core.Dtos;
using myPace.ViewModels;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddCoordinatorPage : Page
    {
        private UserAuthDto _connectedUser;

        public AddCoordinatorViewModel AddCoordinatorViewModel { get; private set; }
        public AddCoordinatorPage()
        {
            this.InitializeComponent();
            AddCoordinatorViewModel = App.Container.Resolve<AddCoordinatorViewModel>();
            DataContext = AddCoordinatorViewModel;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _connectedUser = JsonConvert.DeserializeObject<UserAuthDto>(e.Parameter.ToString());
            await AddCoordinatorViewModel.LoadAsync(_connectedUser);
            base.OnNavigatedTo(e);
        }
    }
}
