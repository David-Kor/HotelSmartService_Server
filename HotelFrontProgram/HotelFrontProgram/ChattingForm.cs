﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotelFrontProgram
{
    public partial class ChattingForm : Form
    {
        private string mv_strID;
        private HotelFront mv_mainForm;
        private bool mv_isConnected;

        public ChattingForm(string strID, HotelFront form)
        {
            InitializeComponent();
            mv_strID = strID;
            mv_mainForm = form;
            mv_isConnected = true;
            if (mv_strID == "SYSTEM_SERVER")
            {
                txtbx_input.Enabled = txtbx_input.Visible = false;
                btn_send.Enabled = btn_send.Visible = false;
            }
            try
            {
                using (StreamWriter file = new StreamWriter(Application.StartupPath + $@"\chtlg_{mv_strID.Replace("\r","")}", true))
                {
                    file.Write("");
                }
                using (StreamReader file = new StreamReader(Application.StartupPath + $@"\chtlg_{mv_strID.Replace("\r", "")}", true))
                {
                    if (file.EndOfStream)
                    {
                        return;
                    }
                    txt_chat_display.AppendText(file.ReadToEnd());
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message, mv_strID);
            }
        }

        public bool IsConnected() { return mv_isConnected; }

        public string GetStringID() { return mv_strID; }

        public void Disconnected()
        {
            PrintMessage("[정보]  상대방과의 연결이 끊겼습니다.\n");
            txtbx_input.Enabled = false;
            txtbx_input.ReadOnly = true;
            btn_send.Enabled = false;
            mv_isConnected = false;
        }
        public void Reconnected()
        {
            PrintMessage("[정보]  상대방과 다시 연결 되었습니다.\n");
            txtbx_input.Enabled = true;
            btn_send.Enabled = true;
            txtbx_input.ReadOnly = false;
            mv_isConnected = true;
        }

        public void ReceiveChat(string txt)
        {
            PrintMessage($"[{mv_strID}]  {txt}");
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
                mv_mainForm.SendToServer($"CHATTO:{mv_strID}:{input}");
                PrintMessage("[전송]  " + input);
            }
        }

        private void PrintMessage(string msg)
        {
            txt_chat_display.AppendText(msg + "\n");
            try
            {
                using (StreamWriter file = new StreamWriter(Application.StartupPath + $@"\chtlg_{mv_strID.Replace("\r", "")}", true))
                {
                    file.WriteLine(msg);
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message, "Chat Err");
            }
        }

        private void ChattingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mv_mainForm.CloseForm(this);
        }

        private void ChattingForm_Load(object sender, EventArgs e)
        {
            //제목표시줄
            Text = mv_strID;
        }

        private void txt_chat_display_TextChanged(object sender, EventArgs e)
        {
            txt_chat_display.ScrollToCaret();
        }
    }
}
