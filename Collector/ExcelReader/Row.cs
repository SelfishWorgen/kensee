using System.Xml.Serialization;

namespace ExcelReader
{
    /// <summary>
    /// (c) 2014 Vienna, Dietmar Schoder
    /// 
    /// Code Project Open License (CPOL) 1.02
    /// 
    /// Deals with an Excel row
    /// </summary>
    public class Row
    {
        [XmlElement("c")]
        public Cell[] FilledCells;
        [XmlIgnore]
        public Cell[] Cells;

        public void ExpandCells(int NumberOfColumns)
        {
            Cells = new Cell[NumberOfColumns];
            if (FilledCells != null)
            {
                Cells = new Cell[FilledCells.Length];
                int i = 0;
                foreach (var cell in FilledCells)
                    Cells[i++] = cell;
                FilledCells = null;
            }
            else
            {
                Cells = new Cell[NumberOfColumns];

            }
        }
    }
}
