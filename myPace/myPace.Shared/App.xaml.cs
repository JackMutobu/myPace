using Autofac;
using Microsoft.EntityFrameworkCore;
using myPace.Core.Entities;
using myPace.Core.Filtering;
using myPace.Core.Interfaces;
using myPace.Infrastracture.Data;
using myPace.Infrastracture.Data.Repositories;
using myPace.Interfaces;
using myPace.Services;
using myPace.Shared.ViewModels.Volunteers;
using myPace.Shared.Views;
using myPace.ViewModels;
using myPace.ViewModels.Coordinators;
using myPace.ViewModels.Volunteers;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace myPace
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            Container = ConfigureServices();

            this.Suspending += OnSuspending;
        }
        public static IContainer Container { get; set; }
        private string GetDbPath()
        {
            string dbPath = default;
#if __ANDROID__
            // Android
            dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "pace.db");
#elif __IOS__
            // iOS
            dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", "pace.db");
#else

            // UWP
            dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "exrin.db");
#endif

            return dbPath;
        }
        private IContainer ConfigureServices()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<StartupViewModel>().AsSelf();
            containerBuilder.RegisterType<LoginViewModel>().AsSelf();
            containerBuilder.RegisterType<HomeCoordViewModel>().AsSelf();
            containerBuilder.RegisterType<SchoolsAndTeachersViewModel>().AsSelf();
            containerBuilder.RegisterType<AddSchoolTeachersViewModel>().AsSelf();
            containerBuilder.RegisterType<VolDetailsViewModel>().AsSelf();
            containerBuilder.RegisterType<HomeVolunteerViewModel>().AsSelf();
            containerBuilder.RegisterType<ProjectVolViewModel>().AsSelf();
            containerBuilder.RegisterType<ProjectDetailsViewModel>().AsSelf();
            containerBuilder.RegisterType<NewProjectViewModel>().AsSelf();
            containerBuilder.RegisterType<ReportsViewModel>().AsSelf();


            containerBuilder.RegisterType<AddCoordinatorViewModel>().AsSelf();
            containerBuilder.RegisterType<RegisterVolViewModel>().AsSelf();
            containerBuilder.RegisterType<VolunteerCoordViewModel>().AsSelf();
            containerBuilder.RegisterType<NewVolDetailsViewModel>().AsSelf();
            containerBuilder.RegisterType<HttpClientService>().As<IHttpClientService>();
            containerBuilder.RegisterType<NavigationService>().As<INavigationService>();

            #region Filtering Class
            containerBuilder.RegisterGeneric(typeof(DateFilterSpecification<>)).As(typeof(IDateFilterSpecification<>));
            containerBuilder.RegisterType<VolunteerFilterBySchoolSpecification>().As<IVolunteerFilterBySchoolSpecification>();
            containerBuilder.RegisterType<VolunteerFilterByStatusSpecification>().As<IVolunteerFilterByStatusSpecification>();
            containerBuilder.RegisterType<ProjectFilterByVolunteerSpecification>().As<IProjectFilterByVolunteerSpecification>();
            containerBuilder.RegisterType<ProjectFilterByStatusOrTypeSpecification>().As<IProjectFilterByStatusOrTypeSpecification>();
            #endregion

            var container = containerBuilder.Build();
            return container;
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
               // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Windows.UI.Xaml.Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Windows.UI.Xaml.Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(StartupPage), e.Arguments);
                }
                // Ensure the current window is active
                Windows.UI.Xaml.Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
