using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Sample_Librarian.Model
{
    public class FileDirectory
    {
        [PrimaryKey, AutoIncrement]
        public int Pk { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
    }
}
