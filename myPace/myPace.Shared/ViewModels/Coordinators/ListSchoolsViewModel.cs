using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.Shared.Helpers;
using myPace.Views;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace myPace.ViewModels.Coordinators
{
    public class ListSchoolsViewModel:ViewModelBase
    {
        private readonly UserAuthDto _connectedUser;
        private ObservableCollection<BaseEntityDto<School>> _schoolsDto;
        private readonly List<School> _schools;

        public ListSchoolsViewModel(IHttpClientService httpClientService, INavigationService navigationService, UserAuthDto connectedUser, List<School> schools) : base(navigationService,httpClientService)
        {
            _connectedUser = connectedUser;
            _schoolsDto = new ObservableCollection<BaseEntityDto<School>>();
            _schools = schools;
        }
        public ObservableCollection<BaseEntityDto<School>> Schools
        {
            get { return _schoolsDto; }
            set { SetProperty(ref _schoolsDto, value); }
        }
        public bool IsFirstLoad { get; set; } = true;
        public DelegateCommand<object> EditSchoolCommand => new DelegateCommand<object>(OnEditSchoolCommand);

        private void OnEditSchoolCommand(object id)
        {
            var schoolId = int.Parse(id?.ToString());
            var school = _schoolsDto.Select(s => s.Entity).SingleOrDefault(e => e.Id == schoolId);
            var userEditDto = new UserEditDto(school)
            {
                Type = Shared.Helpers.TypeEnum.School,
                ConnectedUser = _connectedUser,
                IsNew = true
            };
            var userEditDtoJson = JsonConvert.SerializeObject(userEditDto);
            _navigationService.Navigate(typeof(AddSchoolTeachersPage), userEditDtoJson);
        }



        public async Task LoadAsync()
        {
            var coord = JsonConvert.DeserializeObject<Coordinator>( await _httpClientService.GetBasicAsync($"coordinator/getbyemailinclude/{_connectedUser.Email}"));
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
            if (IsFirstLoad || _schools.Count != _schoolsDto.Count)//blocks duplication of items when navigating to this tab
            {
                foreach (var school in _schools)
                {
                    school.Coordinator = coord;
                    BaseEntityDto<School> schoolDto = new BaseEntityDto<School>(school);
                    var latestProfileMedia = school.Medias.Where(x => x.Type == Core.Helper.MediaTypeEnum.Photo).OrderByDescending(x => x.UploadDate).FirstOrDefault();
                    var path = await FileHelpers.GetFilePath(_httpClientService, "school", school, latestProfileMedia);                  
                    schoolDto.ProfileImage = path == null ? null : new BitmapImage(new Uri(path));
                    _schoolsDto.Add(schoolDto);
                    RaisePropertyChanged(nameof(Schools));
                }
                IsFirstLoad = false;
            }
            RaisePropertyChanged(nameof(Schools));
        }
    }
}
