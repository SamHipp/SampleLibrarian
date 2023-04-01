using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Librarian.Model
{
    public class SourceFolder : ObservableValidator
    {
        public int Id { get; set; }
        public int Pk { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value, true);
        }
    }
}
