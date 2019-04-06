using myPace.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace myPace.Views
{
    public sealed partial class AddSchoolView : UserControl
    {
        public AddSchoolTeachersViewModel ViewModel
        {
            get { return this.DataContext as AddSchoolTeachersViewModel; }
        }
        public AddSchoolView()
        {
            this.InitializeComponent();
        }
    }
}
