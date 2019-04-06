using myPace.ViewModels.Coordinators;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using static myPace.Shared.Helpers.FileHelpers;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace myPace.Views
{
    public sealed partial class ListSchoolsView : UserControl
    {
        public ListSchoolsViewModel ViewModel { get; set; }
        public ListSchoolsView()
        {
            this.InitializeComponent();
            ProggressBarVisible(true, ProgressStack, PrgoressLoading);
        }
        public ListSchoolsView(ListSchoolsViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
            Loaded += ListSchoolsView_Loaded;
        }

        private async void ListSchoolsView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
                ProggressBarVisible(false,ProgressStack,PrgoressLoading);
                parent.Visibility = Visibility.Visible;
            }
        }
       
    }
}
