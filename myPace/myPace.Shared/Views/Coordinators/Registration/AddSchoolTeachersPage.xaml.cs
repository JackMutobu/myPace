using Autofac;
using myPace.Core.Dtos;
using myPace.Shared.Dtos;
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
    public sealed partial class AddSchoolTeachersPage : Page
    {
        private UserEditDto _userEditDto;

        public AddSchoolTeachersViewModel AddSchoolTeachersViewModel { get; private set; }
    
        public AddSchoolTeachersPage()
        {
            this.InitializeComponent();
            AddSchoolTeachersViewModel = App.Container.Resolve<AddSchoolTeachersViewModel>();
            DataContext = AddSchoolTeachersViewModel;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _userEditDto = JsonConvert.DeserializeObject<UserEditDto>(e.Parameter.ToString());
            await AddSchoolTeachersViewModel.LoadAsync(_userEditDto);
            base.OnNavigatedTo(e);
        }

    }
}
