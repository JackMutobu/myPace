using System;
using Autofac;
using myPace.Core.Dtos;
using myPace.ViewModels;
using Newtonsoft.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SchoolsAndTeachersPage : Page
    {
        private UserAuthDto _connectedUser;

        public SchoolsAndTeachersViewModel SchoolsAndTeachersViewModel  { get; private set; }
        public SchoolsAndTeachersPage()
        {
            this.InitializeComponent();
            SchoolsAndTeachersViewModel = App.Container.Resolve<SchoolsAndTeachersViewModel>();
            DataContext = SchoolsAndTeachersViewModel;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            _connectedUser = JsonConvert.DeserializeObject<UserAuthDto>(e.Parameter.ToString());
            await SchoolsAndTeachersViewModel.Load(_connectedUser);
            base.OnNavigatedTo(e);
        }
        protected  override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {


            base.OnNavigatingFrom(e);
        }
    }
}
