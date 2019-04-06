using myPace.ViewModels.Coordinators;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using static myPace.Shared.Helpers.FileHelpers;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace myPace.Views
{
    public sealed partial class ListTeachersView : UserControl
    {
        public ListTeachersViewModel ViewModel {get;private set;}
        public ListTeachersView(ListTeachersViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
            Loaded += ListTeachersView_Loaded;
            
        }

        private async void ListTeachersView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ProggressBarVisible(true, ProgressStack, PrgoressLoading);
            try
            {
                parent.Visibility = Visibility.Collapsed;
                await ViewModel.LoadAsync();
                ViewModel.IsFirstLoad = false;
            }
            finally
            {
                ProggressBarVisible(false, ProgressStack, PrgoressLoading);
                parent.Visibility = Visibility.Visible;
            }
        }
    }
}
