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
    public partial class EditCustomer : Form
    {
        HotelFront parentForm;
        public EditCustomer(HotelFront calledForm)
        {
            InitializeComponent();
            parentForm = calledForm;
        }

        public void SetValues(Dictionary<int, string> contents)
        {
            int i = 0;
            txt_nid.Text = contents[i++];
            txt_name.Text = contents[i++];
            txt_phone.Text = contents[i++];
            txt_age.Text = contents[i++];
            txt_adress.Text = contents[i++];
            txt_room.Text = contents[i++];
        }

        private void btn_nagative_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_positive_Click(object sender, EventArgs e)
        {
            Dictionary<int, string> contents = new Dictionary<int, string>();
            int i = 0;
            contents.Add(i++, txt_nid.Text);
            contents.Add(i++, txt_name.Text);
            contents.Add(i++, txt_phone.Text);
            contents.Add(i++, txt_age.Text);
            contents.Add(i++, txt_adress.Text);
            contents.Add(i++, txt_room.Text);
            parentForm.UpdateValues(contents);
            Close();
        }

        public void RoomTextBoxDisable() { txt_room.ReadOnly = true; }
    }
}
