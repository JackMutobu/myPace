using Autofac;
using myPace.Core.Entities;
using myPace.Shared.Dtos;
using myPace.ViewModels.Coordinators;
using Newtonsoft.Json;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace myPace.Views.Volunteers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VolDetailsPage : Page
    {
        public VolDetailsPage()
        {
            this.InitializeComponent();
            VolDetailsViewModel = App.Container.Resolve<VolDetailsViewModel>();
            DataContext = VolDetailsViewModel;
        }

        public VolDetailsViewModel VolDetailsViewModel { get; private set; }
        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            var volunteerDto = JsonConvert.DeserializeObject<BaseEntityDto<Volunteer>>(e.Parameter.ToString());
             VolDetailsViewModel.Load(volunteerDto);
            base.OnNavigatedTo(e);
        }
    }
}
