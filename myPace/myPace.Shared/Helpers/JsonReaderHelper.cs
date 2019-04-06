using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace myPace.Shared.Helpers
{
    public static class JsonReaderHelper
    {
        public async static Task<T> DeserializeObjectFromStreamAsync<T>(HttpResponseMessage httpResponse)
        {
            JsonSerializer _serializer = new JsonSerializer();
            try
            {
                using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var json = new JsonTextReader(reader))
                        {
                            return _serializer.Deserialize<T>(json);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
