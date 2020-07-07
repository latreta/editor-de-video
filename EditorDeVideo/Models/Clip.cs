using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace EditorDeVideo.Models
{
    public class Clip: BindableBase
    {
        private string clipName;
        public string ClipName
        {
            get { return clipName; }
            set { SetProperty(ref clipName, value); }
        }

        private StorageFile file;
        public StorageFile File
        {
            get { return file; }
            set { SetProperty(ref file, value); }
        }

        private StorageItemThumbnail thumbnail;
        public StorageItemThumbnail Thumbnail
        {
            get { return thumbnail; }
            set { SetProperty(ref thumbnail, value); }
        }
    }
}
