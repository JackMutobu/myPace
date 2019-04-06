using Autofac;
using myPace.Core.Entities;
using myPace.Shared.Dtos;
using myPace.ViewModels.Coordinators;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views.Volunteers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewVolDetailsPage : Page
    {
        public NewVolDetailsViewModel NewVolDetailsViewModel { get; private set; }
        public NewVolDetailsPage()
        {
            this.InitializeComponent();
            NewVolDetailsViewModel = App.Container.Resolve<NewVolDetailsViewModel>();
            DataContext = NewVolDetailsViewModel;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var volunteerDto = JsonConvert.DeserializeObject<BaseEntityDto<Volunteer>>(e.Parameter.ToString());
            await NewVolDetailsViewModel.LoadAsync(volunteerDto);
            base.OnNavigatedTo(e);
        }
    }


}
