using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
