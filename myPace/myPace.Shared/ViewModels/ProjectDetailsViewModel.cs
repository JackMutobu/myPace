using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using myPace.Views.Volunteers;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Essentials;

namespace myPace.ViewModels
{
    public class ProjectDetailsViewModel:ViewModelBase
    {
        private Project _project;
        private Visibility _commentVisibility;
        private Visibility _mediaVisibility;
        private Visibility _teamVisibility;
        private ObservableCollection<BaseEntityDto<Volunteer>> _team;
        private Visibility _coordCommentVisibility;
        private Comment _comment;
        private Visibility _volunteerVisibilityCommands;

        public ProjectDetailsViewModel(INavigationService navigationService, IHttpClientService httpClientService):base(navigationService,httpClientService)
        {
            _commentVisibility = Visibility.Collapsed;
            _teamVisibility = Visibility.Collapsed;
            _mediaVisibility = Visibility.Collapsed;
            _volunteerVisibilityCommands = Visibility.Collapsed;
            _team = new ObservableCollection<BaseEntityDto<Volunteer>>();
            _comment = new Comment();
        }
        #region Properties
        public ObservableCollection<BaseEntityDto<Volunteer>> Team
        {
            get { return _team; }
            private set { SetProperty(ref _team, value); }
        }
        
        public Project Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value);
            }
        }
        public Comment Comment
        {
            get { return _comment; }
            set { SetProperty(ref _comment, value); }
        }
        public Visibility CommentVisibility
        {
            get { return _commentVisibility; }
            set { SetProperty(ref _commentVisibility, value); }
        }
        public Visibility TeamVisibility
        {
            get { return _teamVisibility; }
            set { SetProperty(ref _teamVisibility, value); }
        }
        public Visibility MediaVisibility
        {
            get { return _mediaVisibility; }
            set { SetProperty(ref _mediaVisibility, value); }
        }
        public Visibility CoordCommentVisibility
        {
            get { return _coordCommentVisibility; }
            set { SetProperty(ref _coordCommentVisibility, value); }
        }
        public Visibility VolunteerVisibilityCommands
        {
            get { return _volunteerVisibilityCommands; }
            set { SetProperty(ref _volunteerVisibilityCommands, value); }
        }
        #endregion
        #region Commands
        public DelegateCommand CommentVisibilityCommand => new DelegateCommand(() =>
        {
            _commentVisibility = _commentVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            RaisePropertyChanged(nameof(CommentVisibility));
        });
        
        public DelegateCommand MediaVisibilityCommand => new DelegateCommand(() =>
        {
            _mediaVisibility = _mediaVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            RaisePropertyChanged(nameof(MediaVisibility));
        });
        public DelegateCommand AddCommentCommand => new DelegateCommand(async () =>
        {
            if (string.IsNullOrEmpty(_comment.Text))
            {
                MessageDialog dialog = new MessageDialog("Your comment can not be empty");
                await dialog.ShowAsync();
                return;
            }
            _comment.UploadDate = DateTime.Now;
            _comment.CoordinatorNames = $"{_connectedUserAuth.FirstName} {_connectedUserAuth.LastName}";
            _comment.CoordinatorId = _connectedUserAuth.UserId;
            _project.Comments.Add(_comment);
            await _httpClientService.PutBasicAsync(_project, $"project/update/{_project.Id}");

            MessageDialog messageDialog = new MessageDialog("Your comment has been added");
            _comment = new Comment();
            await messageDialog.ShowAsync();
            RaisePropertyChanged(nameof(Project));
            RaisePropertyChanged(nameof(Comment));

        });
        public DelegateCommand RefreshPageCommand => new DelegateCommand(async () =>
        {
            _team = new ObservableCollection<BaseEntityDto<Volunteer>>();

            await LoadItems(_project);
        });
        public DelegateCommand EditEntityCommand => new DelegateCommand(() =>
        {
            var projectDto = new ProjectDto(_project)
            {
                CanEdit = true,
                ConnectedUser = _connectedUserAuth
            };
            var projectDtoJson = JsonConvert.SerializeObject(projectDto);
            _navigationService.Navigate(typeof(NewProjectPage), projectDtoJson);
        });
        public DelegateCommand DeleteProjectCommand => new DelegateCommand(async () =>
        {
        MessageDialog dialog = new MessageDialog("Do you really want to delete this project?");
        dialog.Commands.Add(new UICommand("Yes")
        {
            Id = 0
        });
        dialog.Commands.Add(new UICommand("No")
        {
            Id = 1
        });
            dialog.CancelCommandIndex = 1;
            var dialogResult = await dialog.ShowAsync();
            if((int)dialogResult.Id == 0)
            {
                dialog = new MessageDialog("");
                var result = await _httpClientService.DeleteAsync($"project/delete/{_project.Id}");
                if(result.IsSuccessStatusCode)
                {
                    dialog.Content = $"{_project.Name} has been deleted";
                }
                else
                {
                    dialog.Content = $"An error occured: {result.Content.ReadAsStringAsync()}";
                }
               
                await dialog.ShowAsync();
                var connectedUserJson = JsonConvert.SerializeObject(_connectedUserAuth);
                _navigationService.Navigate(typeof(HomeVolunteerPage),connectedUserJson);
            }
        });
        public DelegateCommand<object> OpenFileCommand => new DelegateCommand<object>(async param =>
        {
            var mediaId = (int)param ;
            var mediaToOpen = _project.Medias.SingleOrDefault(m => m.Id == mediaId);
            var path = await _httpClientService.GetBasicAsync($"project/geturi?id={_project.Id}&mediaId={mediaToOpen.Id}");
            await Browser.OpenAsync(new Uri(path), BrowserLaunchMode.SystemPreferred);
        });
        #endregion
        #region Methodes
        public async Task LoadAsync(ProjectDto projectDto)
        {
            _connectedUserAuth = projectDto.ConnectedUser;
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUserAuth.Token);
            _project = projectDto.Project;
            RaisePropertyChanged(nameof(Project));
            if (_connectedUserAuth.Role == "Vol")
            {
                _coordCommentVisibility = Visibility.Collapsed;
                _volunteerVisibilityCommands = Visibility.Visible;
                RaisePropertyChanged(nameof(CoordCommentVisibility));
                RaisePropertyChanged(nameof(VolunteerVisibilityCommands));
            }
            await LoadItems(projectDto.Project);
        }

        private async Task LoadItems(Project project)
        {
            ProggressBarVisible(true);
            _project = await _httpClientService.GetTAsync<Project>($"project/getbyIdInclude/{project.Id}");
           

            foreach (var volProj in _project.VolunteerProjects)
            {
                var teamMember = await _httpClientService.GetTAsync<Volunteer>($"volunteer/getbyid/{volProj.VolunterId}");
                var teamMemberDto = new BaseEntityDto<Volunteer>(teamMember);
                var getMedia = await _httpClientService.GetListTAsync<Media>($"volunteer/getmedia?id={teamMember.Id}");
                if (getMedia != null)
                {
                    var latestProfileMedia = getMedia?.Where(x => x.Type == Core.Helper.MediaTypeEnum.Photo).OrderByDescending(x => x.UploadDate).FirstOrDefault();
                    var path = await FileHelpers.GetFilePath(_httpClientService, "volunteer", teamMember, latestProfileMedia);
                    teamMemberDto.ProfileImage = path == null ? null : new BitmapImage(new Uri(path ?? ""));
                }
                _team.Add(teamMemberDto);
            }
            _teamVisibility = Visibility.Visible;
            RaisePropertyChanged(nameof(TeamVisibility));
            RaisePropertyChanged(nameof(Team));
            RaisePropertyChanged(nameof(CoordCommentVisibility));
            RaisePropertyChanged(nameof(VolunteerVisibilityCommands));
            RaisePropertyChanged(nameof(Project));
            ProggressBarVisible(false);
        }
        #endregion
    }
}
