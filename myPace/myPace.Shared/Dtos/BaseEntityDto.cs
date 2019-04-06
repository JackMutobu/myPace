using myPace.Core.Dtos;
using myPace.Core.Entities;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Media;

namespace myPace.Shared.Dtos
{
    public class BaseEntityDto<T> :BaseEntityMainDto<T> where T : BaseEntity
    {
        public  ImageSource ProfileImage { get; set; }
        public BaseEntityDto(T entity):base(entity)
        {
            
        }
    }
}
