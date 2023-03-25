﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Librarian.Model
{
    public class Category
    {
        public int Id { get; set; }
        public int Pk { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public bool IsSelected { get; set; }
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
    }
}
