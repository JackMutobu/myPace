using Microsoft.Xaml.Interactivity;
using myPace.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myPace.Behaviours
{
    public partial class TimeChangedEventBehavior: DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            if (!(associatedObject is TimePicker tp))
            {
                throw new ArgumentException("Error Associating Object");
            }

            this.AssociatedObject = associatedObject;

            tp.TimeChanged += OnTimeChanged;
        }

        private void OnTimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            var timePicker = (sender as TimePicker);
            var mainVM = (timePicker.DataContext as ViewModelBase);
            mainVM.OnTimePickerTimeSelected(timePicker.Time);
        }

        public void Detach()
        {
            if (this.AssociatedObject is TimePicker tp)
            {
                tp.TimeChanged -= this.OnTimeChanged;
            }
        }

    }
}
