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
    public class ListTeachersViewModel:ViewModelBase
    {
        private readonly UserAuthDto _connectedUser;
        private ObservableCollection<BaseEntityDto<Teacher>> _teachers;
        private readonly List<School> _schools;

        public ListTeachersViewModel(IHttpClientService httpClientService,INavigationService navigationService, UserAuthDto connectedUser,List<School> schools) : base(navigationService,httpClientService)
        {
            _connectedUser = connectedUser;
            _teachers = new ObservableCollection<BaseEntityDto<Teacher>>();
            _schools = schools;
        }

        public ObservableCollection<BaseEntityDto<Teacher>> Teachers
        {
            get { return _teachers; }
            set { SetProperty(ref _teachers, value); }
        }
        public bool IsFirstLoad { get; set; } = true;
        public DelegateCommand<object> EditTeacherCommand => new DelegateCommand<object>(OnEditTeacherCommand);

        private void OnEditTeacherCommand(object id)
        {
            var teacherId = int.Parse(id?.ToString());
            var teacher = _teachers.Select(t => t.Entity).SingleOrDefault(e => e.Id == teacherId);
            var userEditDto = new UserEditDto(teacher)
            {
                Type = Shared.Helpers.TypeEnum.Teacher,
                ConnectedUser = _connectedUser,
                IsNew = true
            };
            var userEditDtoJson = JsonConvert.SerializeObject(userEditDto);
            _navigationService.Navigate(typeof(AddSchoolTeachersPage), userEditDtoJson);
        }



        public async Task LoadAsync()
        {
            _httpClientService.SetAuthorizationHeaderToken(_httpClientService.HttpClient, _connectedUser.Token);
            if (IsFirstLoad )//blocks duplication of items when navigating to this tab
            {
                foreach (var school in _schools)
                {
                    var getTeachers = await _httpClientService.GetListTAsync<Teacher>($"teacher/getteachersbyschool/{school?.Id}");
                    foreach (var teacher in getTeachers)
                    {
                        BaseEntityDto<Teacher> teachedDto = new BaseEntityDto<Teacher>(teacher);
                        var latestProfileMedia = teacher.Medias.Where(x => x.Type == Core.Helper.MediaTypeEnum.Photo).OrderByDescending(x => x.UploadDate).FirstOrDefault();
                        var path = await FileHelpers.GetFilePath(_httpClientService, "teacher", teacher, latestProfileMedia);
                        teachedDto.ProfileImage = path == null ? null : new BitmapImage(new Uri(path ?? ""));
                        teachedDto.Age = (DateTime.Now - teacher.DOB.DateTime).Days / 365;
                        _teachers.Add(teachedDto);
                        RaisePropertyChanged(nameof(Teachers));
                    }
                }
            }
            RaisePropertyChanged(nameof(Teachers));
        }
    }
}
