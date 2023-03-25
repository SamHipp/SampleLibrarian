using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Librarian.Model
{
    public class CategoryGroup
    {
        public int Id { get; set; }
        public int Pk { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<Category> Categories { get; set;}
        public bool IsAdding { get; set; }
        public string AddCategoryText { get; set; }
        public int RowsHeight { get; set; }
    }
}
