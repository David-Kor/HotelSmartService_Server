using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotelFrontProgram
{
    public partial class RoomsForm : Form
    {
        public RoomsForm()
        {
            InitializeComponent();
        }

        public void AddRows(string[] info)
        {
            try
            {
                for (int i = 1; i <= 2; i++)
                {
                    if (int.Parse(info[i].Substring(0, 4)) < 2000) { info[i] = ""; }
                }
                if (int.Parse(info[3]) == 0) { info[3] = "False"; }
                else { info[3] = "True"; }
                grid_room_table.Rows.Add(info);
            }
            catch (Exception) { }
        }

        private void grid_room_table_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            MessageBox.Show(grid_room_table.Rows[e.RowIndex].ToString());
        }
    }
}
