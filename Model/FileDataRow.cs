using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Audio;

namespace Sample_Librarian.Model
{
    public partial class FileDataRow : ObservableValidator
    {
        public int Id { get; set; }
        public int Pk { get; set; }
        public string FileName { get; set; }
        public string Length { get; set; }
        public string Format { get; set; }
        public string BitRate { get; set; }
        public string Size { get; set; }
        public string FilePath { get; set; }
        public string LocalFilePath { get; set; }
        public bool isPlaying { get; set; }
        public IAudioPlayer Player { get; set; }
        public bool HasPlayer { get; set; }
        public string PlayerIcon { get; set; }
        private bool isSelected;
        public bool IsSelected { 
            get => isSelected; 
            set => SetProperty(ref isSelected, value, true);
        }
    }
}
