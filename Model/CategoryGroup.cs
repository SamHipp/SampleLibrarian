using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Librarian.Model
{
    public class CategoryGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<Category> Categories { get; set;}
    }
}
