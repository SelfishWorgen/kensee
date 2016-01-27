using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utl
{
    public class ParsingRow
    {
        List<ParsingCell> columns;
        public int notEmptyCell;
        public bool parsed;
        public bool special; // from split

        public ParsingRow()
        {
            columns = new List<ParsingCell>();
        }

        public void addEmptyCell()
        {
            columns.Add(new ParsingCell());
        }

        public void AddColumn(string txt, int fp, int fontSize = 0, int left = 0, int top = 0)
        {
            ParsingCell cell = new ParsingCell(left, top);
            cell.addText(new ParsingCellTextFragment(txt, fp, fontSize));
            columns.Add(cell);
        }

        internal void AddCell(ParsingCell cell)
        {
            if (cell == null)
                cell = new ParsingCell();
            columns.Add(cell);
        }

        internal void InsertCell(int n, ParsingCell cell)
        {
            columns.Insert(n, cell);
        }

        public ParsingCell this[int index]
        {
            get { return columns[index]; }
        }

        public int Count
        {
            get { return this.columns.Count; }
        }

        public List<ParsingCell> Cells
        {
            get { return this.columns; }
        }
    }
}
