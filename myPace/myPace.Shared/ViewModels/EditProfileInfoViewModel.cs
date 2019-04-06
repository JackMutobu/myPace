using myPace.Core.Entities;
using myPace.Core.Helpers;
using myPace.Interfaces;
using myPace.Shared.Helpers;
using myPace.ViewModels;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static myPace.Shared.Helpers.FileHelpers;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;

namespace myPace.Shared.ViewModels
{
    public class EditProfileInfoViewModel:ViewModelBase
    {
        private Coordinator _coordinator;
        private BaseValidationHelpers _validationHelpers;
        private ImageSource _profileImage;
        private FileData _file;
        private string _imagePath;

        public EditProfileInfoViewModel(IHttpClientService httpClientService, INavigationService navigationService) : base(navigationService,httpClientService)
        {
            _coordinator = new Coordinator();
            _validationHelpers = new BaseValidationHelpers();
            _errorVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(ErrorVisibility));
        }
        public ObservableCollection<GenderEnum> GenderList => new ObservableCollection<GenderEnum>(GetGenderEnums().ToList());
        public string ImagePath
        {
            get { return _imagePath; }
            set { SetProperty(ref _imagePath, value); }
        }
        public ImageSource ProfileImageSource
        {
            get { return _profileImage; }
            set { _profileImage = value; OnPropertyChanged(nameof(ProfileImageSource)); }
        }
        public Coordinator Coordinator
        {
            get { return _coordinator; }
            set
            {
                _coordinator = value; OnPropertyChanged(nameof(Coordinator));
            }
        }
        
        public DelegateCommand UploadProfileCommand => new DelegateCommand(async () =>
        {
            _file = await CrossFilePicker.Current.PickFile();
            if (_file == null)
                return; // user canceled file picking
            _imagePath = _file.FileName;
            _profileImage = new BitmapImage();//Remove actual displayed image
            RaisePropertyChanged(nameof(ImagePath));
            RaisePropertyChanged(nameof(ProfileImageSource));
        });
    }
}
