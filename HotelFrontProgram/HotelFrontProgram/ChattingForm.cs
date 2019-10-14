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
    public partial class ChattingForm : Form
    {
        private string ipMsg;
        private HotelFront mainForm;

        public ChattingForm(string strIP, HotelFront form)
        {
            InitializeComponent();
            ipMsg = "CHATTO";
            ipMsg += strIP;
            mainForm = form;
        }

        public string GetStringIP() { return ipMsg.Remove(0, "CHATTO".Length); }

        public void Disconnected()
        {
            PrintMessage("[정보]  상대방과의 연결이 끊겼습니다.\n");
            txtbx_input.Enabled = false;
            txtbx_input.ReadOnly = true;
            btn_send.Enabled = false;
        }
        public void Reconnected()
        {
            PrintMessage("[정보]  상대방과 다시 연결 되었습니다.\n");
            txtbx_input.Enabled = true;
            btn_send.Enabled = true;
            txtbx_input.ReadOnly = false;
        }

        public void ReceiveChat(string txt)
        {
            PrintMessage("[받음]  " + txt);
            if ((txtbx_input.Enabled || btn_send.Enabled) == false)
            {
                Reconnected();
            }
        }

        private void txtbx_input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
            }
        }

        private void btn_send_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            if (txtbx_input.Text.Length > 0)
            {
                string input = txtbx_input.Text;
                txtbx_input.Clear();
                //입력한 채팅 전송
                mainForm.SendToServer(ipMsg + "\n" + input);
                PrintMessage("[보냄]  " + input);
            }
        }

        private void PrintMessage(string msg)
        {
            txt_chat_display.AppendText(msg + "\n");
        }

        private void ChattingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mainForm.CloseForm(this);
        }

        private void ChattingForm_Load(object sender, EventArgs e)
        {

        }

        private void txt_chat_display_TextChanged(object sender, EventArgs e)
        {
            txt_chat_display.ScrollToCaret();
        }
    }
}
