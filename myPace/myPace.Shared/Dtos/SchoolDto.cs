using myPace.Core.Entities;
using myPace.ViewModels;

namespace myPace.Shared.Dtos
{
    public class SchoolDto:ViewModelBase
    {
        public School School { get; set; }
        public string DisplayName                                                                                                                                                                        { get; set; }
        bool _IsChecked = default;
        public bool IsChecked { get { return _IsChecked; } set { SetProperty(ref _IsChecked, value); } }
        public SchoolDto(School school)
        {
            School = school;
            DisplayName = $"{School.Name} - {School.Location}";
        }
    }
}
