using myPace.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media;

namespace myPace.Shared.Dtos
{
    public class TeacherDto: Teacher
    {
        public int Age { get; set; }
        public ImageSource ProfileImage { get; set; }
    }
}
