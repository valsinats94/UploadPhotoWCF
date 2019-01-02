using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.IO;

namespace PhotoService
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PhotoManager
    {
        [WebGet(UriTemplate = "GetPhotos")]
        public PhotoItem[] GetPhotos()
        {
            using (DataAcess data = new DataAcess())
            {
                var photos = data.GetPhotos();
                List<PhotoItem> ret = new List<PhotoItem>();
                foreach (var photo in photos)
                    ret.Add(new PhotoItem() { PhotoID = photo.PhotoID, Name = photo.Name, Description = photo.Description, UploadedOn = photo.DateTime, ImageData = photo.Data });

                return ret.ToArray();
            }
        }

        [WebInvoke(UriTemplate = "UploadPhoto/{fileName}/{description}", Method = "POST")]
        public void UploadPhoto(string fileName, string description, Stream fileContents)
        {
            byte[] buffer = new byte[32768];
            MemoryStream ms = new MemoryStream();
            int bytesRead, totalBytesRead = 0;
            do
            {
                bytesRead = fileContents.Read(buffer, 0, buffer.Length);
                totalBytesRead += bytesRead;

                ms.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);

            // Save the photo on database.
            using (DataAcess data = new DataAcess())
            {
                var photo = new Photo() { Name = fileName, Description = description, Data = ms.ToArray(), DateTime = DateTime.UtcNow };
                data.InsertPhoto(photo);
            }

            ms.Close();
            Console.WriteLine("Uploaded file {0} with {1} bytes", fileName, totalBytesRead);
        }

        [WebGet(UriTemplate = "GetLastPhoto", BodyStyle = WebMessageBodyStyle.Bare)]
        public Stream GetLastPhoto()
        {
            using (DataAcess data = new DataAcess())
            {
                // Retrieve the last taken photo.
                var photo = data.GetLastPhoto();
                if (photo != null)
                {
                    MemoryStream ms = new MemoryStream(photo.Data);
                    return ms;
                }
                else
                {
                    return null;
                }
            }
        }

        [WebGet(UriTemplate = "GetPhoto/{id}", BodyStyle = WebMessageBodyStyle.Bare)]
        public Stream GePhoto(string id)
        {
            using (DataAcess data = new DataAcess())
            {
                // Retrieve the last taken photo.
                var photo = data.GetPhoto(int.Parse(id));
                if (photo != null)
                {
                    MemoryStream ms = new MemoryStream(photo.Data);
                    return ms;
                }
                else
                {
                    return null;
                }
            }
        }

        [WebInvoke(UriTemplate = "DeletePhoto/{id}", Method="DELETE")]
        public void DeletePhoto(string id)
        {
            using (DataAcess data = new DataAcess())
            {
                data.DeletePhoto(int.Parse(id));
            }
        }
    }
}
