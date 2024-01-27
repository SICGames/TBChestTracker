using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace TBChestTracker
{
    /*
     https://stackoverflow.com/questions/856845/how-to-best-way-to-draw-table-in-console-app-c
    */

    public class TableColumn
    {
        public string Header { get; set; }
        private int _width = 75;
        public int Width
        {
            get => _width;
            set => _width = value;
        }
        public TableColumn()
        {

        }
        public TableColumn(string header, int width)
        {
            Header = header;
            Width = width;
        }
    }

    public class TableRow
    {
        public List<TableColumn> Columns { get; set; }  
        public TableRow()
        {
            if(Columns == null)
                Columns = new List<TableColumn>();
        }
        public void AddColumn(List<TableColumn> columns)
        {
            foreach (TableColumn col in columns)
            {
                Columns.Add(col);
            }
        }
        public void AddColumn(string  header, int width)
        {
            Columns.Add(new TableColumn(header, width));
        }
    }

    public class Table
    {
        public List<TableColumn> Columns { get; set; }
        public List<TableRow> Rows { get; set; }

        private int _width = 75;
        public int Width
        {
            get => _width;
            set => _width = value;
        }

        private string _tableString = "";
        public void AddColumn(string Header,  int Width)
        {
            Columns.Add(new TableColumn {  Header = Header, Width = Width });   
        }

        public void AddColumns(params TableColumn[] columns)
        {
            Columns.AddRange(columns);
        }
        public void RemoveColumn(string Header)
        {
            foreach (TableColumn col in Columns)
            {
                if(col.Header == Header)    
                    Columns.Remove(col);
            }
        }
        public void RemoveColumns()
        {
            Columns.Clear();
        }
        public void RemoveColumns(int startIndex, int count)
        {
            var endIndex = startIndex + count;
            for(int i = startIndex; i < endIndex;  i++)
                Columns.RemoveAt(i);
        }
        public void RemoveColumns(params int[] columns)
        {
            foreach(var col in columns)
            {
                Columns.RemoveAt(col);
            }
        }
        public Table() 
        {
            Rows = new List<TableRow>();
            Columns = new List<TableColumn>();  
        }

        private string AddLine(int width, bool newline = false)
        {
            return newline == false ? new string('-', width) : $"{new string('-', width)}\r\n";
        }

        public void AddRow(string text, int width)
        {
            TableRow tableRow = new TableRow();
            tableRow.AddColumn(text, width);
            Rows.Add(tableRow);
        }

        public void AddRows(params TableColumn[] pCols)
        {
            TableRow row = new TableRow();
            row.AddColumn(pCols.ToList());

            Rows.Add(row);
        }
        public void AddRows(List<TableColumn> sCols)
        {
            TableRow row = new TableRow();
            foreach (var col in sCols)
            {
                row.Columns.Add(col);
            }
            Rows.Add(row);  
        }
        public override string ToString()
        {
            string result = string.Empty;
            int totalWidth = 0;
            foreach(var col in Rows[0].Columns) 
            {
                totalWidth += col.Width;
            }

            result += $"{AddLine(totalWidth, true)}";
            foreach (var row in Rows)
            {
                result += "|";
                foreach (var columns in row.Columns)
                {
                    if (string.IsNullOrEmpty(columns.Header))
                        result += new string(' ', columns.Width);
                    else
                    {
                        result += columns.Header.PadRight(columns.Width - (columns.Width - columns.Header.Length) / 2).PadLeft(columns.Width - 1) + "|";
                    }
                }
                result += $"\r\n{AddLine(totalWidth, true)}";
            }
            return result;
        }

        /*
        private void AddRow(params TableColumn[] tableColumns)
        {
            _tableString += "|";

            foreach(TableColumn col in tableColumns)
            {
                if (string.IsNullOrEmpty(col.Header))
                    _tableString += new string(' ', col.Width);
                else
                    _tableString += col.Header.PadRight(col.Width);

                _tableString += "|";
            }
        }
        */

        /*
        public static string PrintRow(params string[] columns)
        {
            int width = (Width - columns.Length) / columns.Length;
            string row = "|";
            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }
            return row;
        }
        
        private static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2, ' ').PadLeft(width, ' ');
            }
        }
        */
    }
}
