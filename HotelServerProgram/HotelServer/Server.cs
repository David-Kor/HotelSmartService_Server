using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HotelServer
{
    public partial class Server : Form
    {
        //DB 커넥션 스트링
        private const string strConn = "Data Source = 27.117.199.42,1433; database = HotelDB; uid = sa; pwd = 9d8a7v7i8d2-@";
        //private const string strConn = "Data Source = David-PC,1433; database = HotelDB; uid = sa; pwd = 9d8a7v7i8d2-@";

        private SqlConnection con;  //DB 커넥션
        private SocketServer sv;    //소켓 서버 클래스

        public Server()
        {
            InitializeComponent();
        }

        public RichTextBox GetRichTextBox() { return display1; }

        private void Form1_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(strConn);
            sv = new SocketServer(this);
        }

        //프로그램의 창이 닫힐 때 호출됨
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            sv.AllSockClose();
        }

        private void display1_TextChanged(object sender, EventArgs e)
        {
            display1.ScrollToCaret();
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            display1.SelectionStart = 0;
            display1.ScrollToCaret();
            display1.Clear();
        }
    }
}
