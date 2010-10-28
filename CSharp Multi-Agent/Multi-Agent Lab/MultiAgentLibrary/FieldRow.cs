using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace MultiAgentLibrary
{
    public class FieldRow : ObservableCollection<FieldSquare>
    {
        public FieldRow(int position, int width)
        {
            for (var i = 0; i < width; i++)
                Add(new FieldSquare(new Point(i, position)));
        }
    }
}
