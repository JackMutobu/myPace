using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Helper;
using myPace.Core.Validation;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using Newtonsoft.Json;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Essentials;
using static myPace.Shared.Helpers.FileHelpers;

namespace myPace.ViewModels.Volunteers
{
    public class NewProjectViewModel:ViewModelBase
    {
        private Project _project;
        private bool _isNew;
        private Visibility _teamVisibility;
        private Visibility _mediaVisibility;
        private string _fileDescription;
        private string _teamMemberEmail;
        private List<Volunteer> _volunteers;
        private ObservableCollection<BaseEntityDto<Volunteer>> _volunteersDto;
        private ProjectCategory _projectCategory;
        private Volunteer _actualVolunteer; //The connected volunteer
        private ProjectValidator _projectValidator;
        private ObservableCollection<MediaFile> _mediaFiles;



        #region Private Members
        #endregion
        public NewProjectViewModel(INavigationService navigationService, IHttpClientService httpClientService):base(navigationService,httpClientService)
        {
            _errorVisibility = Visibility.Collapsed;
            _teamVisibility = Visibility.Collapsed;
            _mediaVisibility = Visibility.Collapsed;
            _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>();
            _mediaFiles = new ObservableCollection<MediaFile>();
        }
        #region Properties
        public ObservableCollection<ProjectType> ProjectTypeList => new ObservableCollection<ProjectType>(Enum.GetValues(typeof(ProjectType)).Cast<ProjectType>().ToList());
        public ObservableCollection<ProjectCategory> ProjectCategoryList => new ObservableCollection<ProjectCategory>(Enum.GetValues(typeof(ProjectCategory)).Cast<ProjectCategory>().ToList());
        public Project Project
        {
            get { return _project; }
            set
            {
                SetProperty(ref _project, value);              
            }
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
        public string FileDescription
        {
            get { return _fileDescription; }
            set { SetProperty(ref _fileDescription, value); }
        }
        public string TeamMemberEmail
        {
            get { return _teamMemberEmail; }
            set { SetProperty(ref _teamMemberEmail, value); }
        }
        public ObservableCollection<BaseEntityDto<Volunteer>> Volunteers
        {
            get { return _volunteersDto; }
            set { SetProperty(ref _volunteersDto, value); }
        }
        public ObservableCollection<MediaFile> Medias
        {
            get { return _mediaFiles; }
            set { SetProperty(ref _mediaFiles, value); }
        }
        public ProjectCategory ProjectCategory
        {
            get { return _projectCategory; }
            set {
                SetProperty(ref _projectCategory, value);
                _teamVisibility = _projectCategory == ProjectCategory.Team ? Visibility.Visible : Visibility.Collapsed;
                RaisePropertyChanged(nameof(TeamVisibility));
            }
        }
        #endregion
        public class MediaFile
        {
            public Media Media { get; set; }
            public FileData File {get;set;}
        }
        #region Commands
        public DelegateCommand TeamVisibilityCommand => new DelegateCommand(() =>
        {
            _teamVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(TeamVisibility));
        });
        public DelegateCommand MediaVisibilityCommand => new DelegateCommand(() =>
        {
            _mediaVisibility =_mediaVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            RaisePropertyChanged(nameof(MediaVisibility));
        });
        public DelegateCommand AddTeamMemberCommand => new DelegateCommand( async () =>
        {
            if(string.IsNullOrEmpty(_teamMemberEmail))
            {
                _validationErrors = "Kindly specify a team member email before adding";
                _errorVisibility = Visibility.Visible;
                RaisePropertyChanged(nameof(ValidationErrors));
                RaisePropertyChanged(nameof(ErrorVisibility));
                return;
            }
            var newVolMember = _volunteers.SingleOrDefault(v => v.Email?.ToLower() == _teamMemberEmail?.Trim().ToLower());
            if(newVolMember == null)
            {
                _validationErrors = $"Unable to find volunteer with email :{_teamMemberEmail}";
                _errorVisibility = Visibility.Visible;
                RaisePropertyChanged(nameof(ValidationErrors));
                RaisePropertyChanged(nameof(ErrorVisibility));
                return;
            }
            var newVolMemberDto = new BaseEntityDto<Volunteer>(newVolMember);
            if (_volunteersDto.SingleOrDefault(v=> v.Entity.Email == newVolMemberDto.Entity.Email) != null)
                return;
            var getMedia = await _httpClientService.GetListTAsync<Media>($"volunteer/getmedia?id={newVolMember.Id}");
            if (getMedia != null)
            {
                var latestProfileMedia = getMedia?.Where(x => x.Type == Core.Helper.MediaTypeEnum.Photo).OrderByDescending(x => x.UploadDate).FirstOrDefault();
                var path = await FileHelpers.GetFilePath(_httpClientService, "volunteer", newVolMember, latestProfileMedia);
                newVolMemberDto.ProfileImage = path == null ? null : new BitmapImage(new Uri(path ?? ""));
            }
            _volunteersDto.Add(newVolMemberDto);
            RaisePropertyChanged(nameof(Volunteers));
            _errorVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(ErrorVisibility));
        });
        public DelegateCommand AttachFileCommand => new DelegateCommand(async () =>
        {
            if (string.IsNullOrEmpty(_fileDescription))
            {
                _validationErrors = "Kindly specify a description of your file before adding";
                _errorVisibility = Visibility.Visible;
                RaisePropertyChanged(nameof(ValidationErrors));
                RaisePropertyChanged(nameof(ErrorVisibility));
                return;
            }
            if (_fileDescription.Count() > 150)
            {
                _validationErrors = "Kindly specify a short description (10 words)";
                _errorVisibility = Visibility.Visible;
                RaisePropertyChanged(nameof(ValidationErrors));
                RaisePropertyChanged(nameof(ErrorVisibility));
                return;
            }
            var newfile = await CrossFilePicker.Current.PickFile();
            if (newfile == null)
                return; // user canceled file picking
            var newMediaFile = new MediaFile
            {
                Media = new Media
                {
                    Description = _fileDescription,
                    Format = Path.GetExtension(newfile.FilePath)
                },
                File = newfile
            };

            if (_mediaFiles.Any(m=> m.File == newfile || m.Media.Description == _fileDescription))
                return;
            _mediaFiles.Add(newMediaFile);
            _errorVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(ErrorVisibility));
        });
        public DelegateCommand SaveProjectCommand => new DelegateCommand(async () =>
        {
            DisableButtons();
            ProggressBarVisible(true);
            _projectValidator = new ProjectValidator();
            var valResult = await _projectValidator.ValidateAsync(_project);
            if(valResult.IsValid)
            {
                MessageDialog dialog = new MessageDialog("");
                _project.Category = _projectCategory;
                _project.UploadDate = DateTimeOffset.Now.DateTime;
                HttpResponseMessage saveProjectResult = default;
                if(_isNew)
                {
                    saveProjectResult = await _httpClientService.PostBasicAsync(_project, "project/register");
                }
                else
                {
                    saveProjectResult = await _httpClientService.PutBasicAsync(_project, $"project/update/{_project.Id}");
                }
                if (saveProjectResult.IsSuccessStatusCode)
                {
                    var registeredEntity = JsonConvert.DeserializeObject<Project>(await saveProjectResult.Content.ReadAsStringAsync());
                    #region Update Volunteer - Project Relationship
                    if (registeredEntity.Category == ProjectCategory.Team)
                    {
                        foreach (var volunteerDto in _volunteersDto)
                        {
                            var entity = volunteerDto.Entity;
                            entity.VolunteerProjects.Add(new VolunteerProject
                            {
                                ProjectId = registeredEntity.Id
                            });
                            var updateResult = await _httpClientService.PutBasicAsync(entity, $"volunteer/update/{entity.Id}");
                            if (updateResult.IsSuccessStatusCode)
                            {
                                volunteerDto.Entity = JsonConvert.DeserializeObject<Volunteer>(await updateResult.Content.ReadAsStringAsync());
                                continue;
                            }
                            else
                            {
                                if(_isNew)
                                    dialog.Content += await updateResult.Content.ReadAsStringAsync();
                                continue;
                            }
                        }
                    }
                    _actualVolunteer.VolunteerProjects.Add(new VolunteerProject
                    {
                        ProjectId = registeredEntity.Id
                    });
                    var updatePersonelResult = await _httpClientService.PutBasicAsync(_actualVolunteer, $"volunteer/update/{_actualVolunteer.Id}");
                    if (updatePersonelResult.IsSuccessStatusCode)
                    {
                        _actualVolunteer = JsonConvert.DeserializeObject<Volunteer>(await updatePersonelResult.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        if(_isNew)
                            dialog.Content += await updatePersonelResult.Content.ReadAsStringAsync();
                    }
                    #endregion
                    #region Add-Media
                    if (_mediaFiles.Count > 0)
                    {
                        foreach (var mediaFile in _mediaFiles)
                        {
                            if (mediaFile.File is null)
                                continue;
                            var path = await UplaodFileAsync(_httpClientService, registeredEntity, "project", mediaFile?.File, mediaFile.Media.Description);
                        }
                    }
                    #endregion
                    dialog.Content += $"{_project.Name} has been successfully saved";
                    await dialog.ShowAsync();
                    _project = new Project();
                    _project.StartDate = DateTimeOffset.Now;
                    _project.EndDate = DateTimeOffset.Now;
                    _mediaFiles = new ObservableCollection<MediaFile>();
                    _volunteersDto = new ObservableCollection<BaseEntityDto<Volunteer>>();
                    HideValidationErros();
                    EnableButtons();
                    RaisePropertyChanged(nameof(Volunteers));
                    RaisePropertyChanged(nameof(Medias));
                    RaisePropertyChanged(nameof(Project));
                    ProggressBarVisible(false);
                    return;
                }
                else
                {
                    dialog.Title = "Error";
                    dialog.Content = await saveProjectResult.Content.ReadAsStringAsync();
                    await dialog.ShowAsync();
                    HideValidationErros();
                    EnableButtons();
                }
                ProggressBarVisible(false);
                return;
            }
            DisplayValidationErros(valResult.ToString());          
            EnableButtons();
            ProggressBarVisible(false);
        });
        public DelegateCommand<object> RemoveMediaFromListCommand => new DelegateCommand<object>(async param =>
         {
             var file = param as FileData;
             var mediaToRemove = _mediaFiles.SingleOrDefault(m => m.File.FileName == file.FileName);
             _mediaFiles.Remove(mediaToRemove);
             RaisePropertyChanged(nameof(Medias));
             if(_isNew == false)
             {

                 _project.Medias.Remove(mediaToRemove.Media);
                var  saveProjectResult = await _httpClientService.PutBasicAsync(_project, $"project/update/{_project.Id}");
                 if(saveProjectResult.IsSuccessStatusCode)
                 {
                     MessageDialog dialog = new MessageDialog("Done");
                    await  dialog.ShowAsync();
                 }
             }
         });
        public DelegateCommand<object> OpenFileCommand => new DelegateCommand<object>(async param =>
        {
            var file = param as FileData;
            var mediaToOpen = _mediaFiles.SingleOrDefault(m => m.File.FileName == file.FileName);
            var path = await _httpClientService.GetBasicAsync($"project/geturi?id={_project.Id}&mediaId={mediaToOpen.Media.Id}");
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    var uri = new Uri(mediaToOpen.File.FilePath);
                    path = uri.AbsoluteUri;
                }
                await Browser.OpenAsync(new Uri(path), BrowserLaunchMode.SystemPreferred);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
           
        });
        #endregion
        #region Methodes
        internal async Task LoadAsync(ProjectDto projectDto)
        {
            try
            {
                _connectedUserAuth = projectDto.ConnectedUser;
                _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUserAuth.Token);
                if (projectDto.CanEdit == false)
                {
                    _project = new Project();

                    _isNew = true;
                    _project.StartDate = DateTimeOffset.Now;
                    _project.EndDate = DateTimeOffset.Now;
                }
                else
                {
                    _isNew = false;
                    _project = projectDto.Project;
                    DisableButtons();
                    RaisePropertyChanged(nameof(Project));
                    ProggressBarVisible(true);
                    _project = await _httpClientService.GetTAsync<Project>($"project/getbyIdInclude/{_project.Id}");
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
                        _volunteersDto.Add(teamMemberDto);
                        if (_project.Category == ProjectCategory.Team)
                        {
                            _teamVisibility = Visibility.Visible;
                            RaisePropertyChanged(nameof(TeamVisibility));
                        }
                    }
                    EnableButtons();
                    foreach (var media in _project.Medias)
                    {
                        _mediaFiles.Add(new MediaFile
                        {
                            File = new FileData
                            {
                                FileName = media.Name
                            },
                            Media = media
                        });
                    }
                    _mediaVisibility = Visibility.Visible;
                    RaisePropertyChanged(nameof(MediaVisibility));
                }

                _volunteers = await _httpClientService.GetListTAsync<Volunteer>("volunteer/getall");
                _actualVolunteer = _volunteers.SingleOrDefault(v => v.Email == _connectedUserAuth.Email);
                _volunteers.Remove(_actualVolunteer);
                RaisePropertyChanged(nameof(Project));
                ProggressBarVisible(false);
            }
     
            catch(Exception ex)
            {
                MessageDialog dialog = new MessageDialog(ex.Message);
                EnableButtons();
                ProggressBarVisible(false);
            }
        }
        public void CategoryChanged()
        {
            _teamVisibility = _project.Category == ProjectCategory.Team ? Visibility.Visible : Visibility.Collapsed;
            RaisePropertyChanged(nameof(TeamVisibility));
        }
        #endregion

    }
}
