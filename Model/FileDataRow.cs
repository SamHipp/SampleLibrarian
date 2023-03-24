using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

namespace Sample_Librarian.Model
{
    public class FileDataRow
    {
        public int Id { get; set; }
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
        public bool IsSelected { get; set; }
    }
}
