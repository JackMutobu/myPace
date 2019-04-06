using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace myPace.Shared.Helpers
{
    public static class ImageHelper
    {
//        public async static Task<ImageSource> ConvertStreamToImageAsync(Stream fileStream)
//        {
//            byte[] imageBuffer = default;
//#if __ANDROID__
//#elif __IOS__
//#else
//            using (var memStream = new MemoryStream())
//            {
//                await fileStream.CopyToAsync(memStream);
//                imageBuffer = memStream.ToArray();
//                using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
//                {
//                    // Writes the image byte array in an InMemoryRandomAccessStream
//                    // that is needed to set the source of BitmapImage.
//                    using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
//                    {
//                        writer.WriteBytes(imageBuffer);
//                        await writer.StoreAsync();
//                    }

//                    var image = new BitmapImage();
//                    await image.SetSourceAsync(ms);
//                    return image;
//                }
//            }
            
//#endif
//        }
    }
}
