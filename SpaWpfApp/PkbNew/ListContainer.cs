using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PkbNew
{
    public class ListContainer
    {
        private string Name;
        private int Number;
        public List<bool> Lines { get; set; }

        public ListContainer(int number, string name)
        {
            this.Name = name;
            this.Number = number;
            this.Lines = new List<bool>();
        }

        public int LinesCount()
        {
            return Lines.Count;
        }

        public void Add(bool value)
        {
            Lines.Add(value);
        }

        public bool ValueAt(int i)
        {
            return Lines.ElementAt(i);
        }
    }
}
