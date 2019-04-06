using FluentValidation.Results;
using myPace.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace myPace.Shared.Helpers
{
    public class BaseValidationHelpers:ViewModelBase
    {
        public BaseValidationHelpers()
        {
            ErrorVisibility = Visibility.Collapsed;
        }
        public void RaiseValidationErrors(ValidationResult result)
        {
            _validationErrors = result.ToString();
            _errorVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(ErrorVisibility));
            OnPropertyChanged(nameof(ValidationErrors));
        }
        public void HideValidationErrors()
        {
            _errorVisibility = Visibility.Collapsed;
            OnPropertyChanged(nameof(ErrorVisibility));
        }
    }
}
