using myPace.Core.Dtos;
using myPace.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.UI.Xaml;

namespace myPace.ViewModels
{
    public abstract class ViewModelBase: INotifyPropertyChanged
    {
        protected UserAuthDto _connectedUserAuth;
        protected string _viewTitle;
        protected readonly INavigationService _navigationService;
        protected readonly IHttpClientService _httpClientService;
        protected Visibility _progressStackVisibility;
        protected bool _progressRingActive;
        
        public ViewModelBase(INavigationService navigationService,IHttpClientService httpClientService)
        {
            _navigationService = navigationService;
            _httpClientService = httpClientService;
        }
        public ViewModelBase()
        {
            
        }

        public virtual void OnTimePickerTimeSelected(TimeSpan time)
        {
            
        }

        protected Visibility _errorVisibility;
        protected string _validationErrors;
        public DelegateCommand GoToBackPage => new DelegateCommand(() => _navigationService.GoBack());
        public DelegateCommand GoToForwardPage => new DelegateCommand(() => _navigationService.GoForward()).ObservesProperty(() => _navigationService.CanGoForward);        
        public Visibility ErrorVisibility
        {
            get { return _errorVisibility; }
            set
            {
                _errorVisibility = value;
                OnPropertyChanged();
            }
        }
        public string ValidationErrors
        {
            get { return _validationErrors; }
            set
            {
                _validationErrors = value;
                OnPropertyChanged();
            }
        }
        protected bool _btnVisibility = true;
        private Visibility _parentVisibility;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool BtnVisibility
        {
            get { return _btnVisibility; }
            set { SetProperty(ref _btnVisibility, value); }
        }
        public string ViewTitle
        {
            get { return _viewTitle; }
            set { SetProperty(ref _viewTitle, value); }
        }
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected virtual void DisableButtons()
        {
            _btnVisibility = false;
            RaisePropertyChanged(nameof(BtnVisibility));
        }
        protected virtual void EnableButtons()
        {
            _btnVisibility = true;
            RaisePropertyChanged(nameof(BtnVisibility));
        }
        protected virtual void DisplayValidationErros(string errors)
        {
            _validationErrors = errors;
            _errorVisibility = Visibility.Visible;
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(ErrorVisibility));
        }
        protected virtual void HideValidationErros()
        {
            _errorVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(ValidationErrors));
            RaisePropertyChanged(nameof(ErrorVisibility));
        }
        public Visibility ParentVisibility
        {
            get { return _parentVisibility; }
            set { SetProperty(ref _parentVisibility, value); }
        }
        public Visibility ProgressStackVisibility
        {
            get { return _progressStackVisibility; }
            set { SetProperty(ref _progressStackVisibility, value); }
        }
        public bool ProgressRingActive
        {
            get { return _progressRingActive; }
            set { SetProperty(ref _progressRingActive, value); }
        }
        public void ProggressBarVisible(bool visible)
        {
            _progressStackVisibility = visible ? Visibility.Visible : Visibility.Collapsed;
            _parentVisibility = visible ? Visibility.Collapsed : Visibility.Visible;
            _progressRingActive = visible;
            RaisePropertyChanged(nameof(ProgressStackVisibility));
            RaisePropertyChanged(nameof(ProgressRingActive));
            RaisePropertyChanged(nameof(ParentVisibility));
        }
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {

            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);
            return true;

        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)

        {

            OnPropertyChanged(propertyName);

        }


        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)

        {

            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)

        {

            PropertyChanged?.Invoke(this, args);

        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)

        {

            var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);

            OnPropertyChanged(propertyName);

        }

    }
}

