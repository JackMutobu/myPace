using Autofac;
using myPace.Core.Entities;
using myPace.Shared.Dtos;
using myPace.ViewModels.Coordinators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace myPace.Controls.Coordinators.Volunteers
{
    public sealed partial class GeneralInfoView : UserControl
    {
        private readonly BaseEntityDto<Volunteer> _volunteer;
        public GeneralInfoView()
        {
            this.InitializeComponent();
           
        }
        public GeneralInfoView(BaseEntityDto<Volunteer> volunteer)
        {
            this.InitializeComponent();
            NewVolDetailsViewModel = App.Container.Resolve<NewVolDetailsViewModel>();
            DataContext = NewVolDetailsViewModel;
            _volunteer = volunteer;
            Loaded += GeneralInfoView_Loaded;
        }

        private async void GeneralInfoView_Loaded(object sender, RoutedEventArgs e)
        {
            await NewVolDetailsViewModel.LoadAsync(_volunteer);
        }

        public NewVolDetailsViewModel NewVolDetailsViewModel { get; private set; }
    }
}
