using Autofac;
using myPace.Core.Dtos;
using myPace.ViewModels;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace myPace.Controls.Volunteers
{
    public sealed partial class ReportsVolView : UserControl
    {
        public ReportsVolView()
        {
            this.InitializeComponent();
            ReportsViewModel = App.Container.Resolve<ReportsViewModel>();
            DataContext = ReportsViewModel;
            Loaded += ReportsVolView_Loaded;
        }
        public ReportsVolView(UserAuthDto connectedUserAuth)
        {
            this.InitializeComponent();
            _connectedUserAuth = connectedUserAuth;
            ReportsViewModel = App.Container.Resolve<ReportsViewModel>();
            DataContext = ReportsViewModel;
            Loaded += ReportsVolView_Loaded;
        }

        private void ReportsVolView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if(_connectedUserAuth != null)
                ReportsViewModel.LoadAsync(_connectedUserAuth);
        }

        private readonly UserAuthDto _connectedUserAuth;

        public ReportsViewModel ReportsViewModel { get; set; }
    }
}
