using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO.Ports;

namespace HotelFrontProgram
{
    public partial class HotelFront : Form
    {
        private Socket sock_sv;
        private const int BUFF_SIZE = 1024;
        private LinkedList<ChattingForm> chatFromList;
        private RoomsForm roomFrom;
        private int selectedRow = 0;
        private class Customer
        {
            public string nid;
            public string name;
            public string phone;
            public int age;
            public string adress;
        }

        private class Reservation
        {
            public int rid;
            public string nid;
        }

        public HotelFront()
        {
            InitializeComponent();
            chatFromList = new LinkedList<ChattingForm>();
            ConnectToServer();
        }

        //윈도우 폼을 불러올 때 이벤트
        private void Form1_Load(object sender, EventArgs e)
        {
            SendToServer("GET_CUSTOMER");
            SelectAllCustomers();
        }

        //서버에 연결 시도
        private void ConnectToServer()
        {
            //이미 연결된 상태인 경우
            if (sock_sv != null && sock_sv.Connected)
            {
                MessageBox.Show("이미 서버와 연결되어있습니다.");
                return;
            }
            try
            {
                byte[] r_buff = new byte[BUFF_SIZE];
                byte[] s_buff = Encoding.UTF8.GetBytes("PC");
                sock_sv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ip_end = new IPEndPoint(IPAddress.Parse("27.117.199.42"), 14334);
                //서버 연결시도
                sock_sv.Connect(ip_end);

                //"PC" 지정 메시지 전송
                sock_sv.BeginSend(s_buff, 0, s_buff.Length, SocketFlags.None, new AsyncCallback(SendMessage), s_buff);
                //비동기 수신 시작
                sock_sv.BeginReceive(r_buff, 0, BUFF_SIZE, SocketFlags.None, new AsyncCallback(ReceiveMessage), r_buff);
            }
            catch (Exception)
            {
                //MessageBox.Show(except.Message, "Connect");
                sock_sv.Close();
                ConnectToServer();
            }
        }


        //비동기 소켓 메시지 수신
        private void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                if (sock_sv.Connected)
                {
                    byte[] buffer = (byte[])ar.AsyncState;
                    int rcvSize = sock_sv.EndReceive(ar);
                    string rcvMsg = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                    Array.Clear(buffer, 0, BUFF_SIZE);
                    sock_sv.BeginReceive(buffer, 0, BUFF_SIZE, SocketFlags.None, new AsyncCallback(ReceiveMessage), buffer);
                    CommandAction(rcvMsg);
                }
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message, "Recv");
                sock_sv.Close();
                ConnectToServer();
            }
        }


        //비동기 소켓 메시지 전송
        private void SendMessage(IAsyncResult ar)
        {
            try
            {
                byte[] buffer = (byte[])ar.AsyncState;
                int sndSize = sock_sv.EndSend(ar);
            }
            catch (Exception except)
            {
                MessageBox.Show(except.Message, "Send");
                sock_sv.Close();
            }
        }


        //수신 메시지 별 행동
        private void CommandAction(string cmd)
        {
            if (cmd == null || cmd.Length == 0) { return; }

            string[] strSplit = cmd.Split(':');

            try
            {
                switch (strSplit[0])
                {
                    //채팅방 호출 명령어 (CHTCALL:id)
                    case "CHTCALL":
                        {
                            //args 부족
                            if (strSplit.Length <= 1)
                            {
                                break;
                            }
                            //현재 열려있는 chattingForm 중에서 ID가 같은 폼을 탐색
                            ChattingForm chattingForm = FindByID(strSplit[1]);
                            //ID가 존재하지 않으면 새로운 폼 생성
                            if (chattingForm == null)
                            {
                                chattingForm = new ChattingForm(strSplit[1], this);
                                Application.Run(chatFromList.AddLast(chattingForm).Value);
                            }
                            //ID가 존재하고 연결 상태가 끊김이면 상태를 변경
                            else if (chattingForm.IsConnected() == false)
                            {
                                chattingForm.Reconnected();
                            }
                            break;
                        }
                    //채팅 내용 전달 명령어 (CHAT:id:내용)
                    case "CHAT":
                        {
                            //args 부족
                            if (strSplit.Length <= 2)
                            {
                                break;
                            }

                            //현재 열려있는 chattingForm 중에서 ID가 같은 폼을 탐색
                            ChattingForm chattingForm = FindByID(strSplit[1]);

                            for (int i = 3; i < strSplit.Length; i++)
                            {
                                strSplit[2] += $":{strSplit[i]}";
                            }
                            chattingForm.ReceiveChat(strSplit[2]);
                            break;
                        }
                    //어플의 연결이 끊겼음을 알리는 명령어
                    case "CCLOSE":
                        {
                            //args 부족
                            if (strSplit.Length <= 1)
                            {
                                break;
                            }

                            ChattingForm chattingForm = FindByID(strSplit[1]);
                            if (chattingForm != null)
                            {
                                chattingForm.Disconnected();
                            }
                            break;
                        }
                    //어플이 재연결 됨을 알리는 명령어
                    case "CCON":
                        {
                            //args 부족
                            if (strSplit.Length <= 1)
                            {
                                break;
                            }

                            ChattingForm chattingForm = FindByID(strSplit[1]);
                            if (chattingForm != null)
                            {
                                chattingForm.Reconnected();
                            }
                            break;
                        }
                    //고객 정보를 전송하는 명령어(CLIST:@ID:nid:@NM:name:@PN:phone:@AG:age:@AD:address:@RM:rid)
                    case "CLIST":
                        {
                            break;
                        }
                }

                return;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            if (false)
            {
                try
                {
                    if (cmd.StartsWith("CallChat"))
                    {
                        cmd = cmd.Remove(0, "CallChat".Length);
                        ChattingForm chatForm = FindByID(cmd);
                        if (chatForm == null)
                        {
                            chatForm = new ChattingForm(cmd, this);
                            Application.Run(chatFromList.AddLast(chatForm).Value);
                            //chatFromList.AddLast(form).Value.Show();
                        }
                        else
                        {
                            chatForm.Reconnected();
                        }
                    }
                    else if (cmd.StartsWith("CHATFROM"))
                    {
                        string ip = cmd.Split('\n')[0].Remove(0, "CHATFROM".Length);
                        cmd = cmd.Split('\n')[1];

                        ChattingForm form = FindByID(ip);
                        if (form != null)
                        {
                            form.ReceiveChat(cmd);
                        }
                        else
                        {
                            form = new ChattingForm(cmd, this);
                            Application.Run(chatFromList.AddLast(form).Value);
                            form.ReceiveChat(cmd);
                        }
                    }
                    else if (cmd.StartsWith("CLOSECHAT"))
                    {
                        string ip = cmd.Remove(0, "CLOSECHAT".Length);
                        ChattingForm form = FindByID(ip);
                        if (form != null)
                        {
                            form.Disconnected();
                        }
                    }
                    else if (false && cmd.StartsWith("CUSTOMERINFO"))
                    {
                        string[] info = cmd.Remove(0, "CUSTOMERINFO".Length + 1).Split('\n');
                        grid_customer_table.Rows.Add(info);
                    }
                    else if (cmd.StartsWith("ROOMINFO"))
                    {
                        string[] info = cmd.Remove(0, "ROOMINFO".Length + 1).Split('\n');
                        roomFrom.AddRows(info);
                    }
                    else if (cmd.EndsWith("FAIL"))
                    {
                        if (cmd.StartsWith("UPDATE"))
                        {
                            MessageBox.Show("고객 정보 수정에 실패하였습니다.\n정확한 형식에 맞게 입력했는 지 확인하여주십시오.",
                                "UPDATE ERROR");
                        }
                        else if (cmd.StartsWith("INSERT"))
                        {
                            MessageBox.Show("고객 정보 추가에 실패하였습니다.\n정확한 형식에 맞게 입력했는 지 확인하여주십시오.",
                                "INSERT ERROR");
                        }
                        else if (cmd.StartsWith("DELETE"))
                        {
                            MessageBox.Show("고객 정보 삭제에 실패하였습니다.\n잘못된 정보이거나 이미 삭제된 정보입니다. 새로고침을 눌러보십시오.",
                                   "DELETE ERROR");
                        }
                    }
                    else if (cmd.EndsWith("SUCCESS"))
                    {
                        if (cmd.StartsWith("UPDATE"))
                        {
                            MessageBox.Show("고객 정보를 성공적으로 수정하였습니다.",
                                "UPDATE COMPLETE");
                            SendToServer("GET_CUSTOMER");
                            SelectAllCustomers();
                        }
                        else if (cmd.StartsWith("INSERT"))
                        {
                            MessageBox.Show("고객 정보를 성공적으로 추가하였습니다.",
                                "INSERT COMPLETE");
                            SendToServer("GET_CUSTOMER");
                            SelectAllCustomers();
                        }
                        else if (cmd.StartsWith("DELETE"))
                        {
                            MessageBox.Show("고객 정보를 성공적으로 삭제하였습니다.",
                                   "DELETE COMPLETE");
                            SendToServer("GET_CUSTOMER");
                            SelectAllCustomers();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "MethodError_CommandAction");
                }
            }
        }

        private ChattingForm FindByID(string id)
        {
            LinkedListNode<ChattingForm> node = chatFromList.First;
            if (node == null) { return null; }

            while (node != null)
            {
                if (node.Value.GetStringID() == id) { return node.Value; }
                node = node.Next;
            }
            return null;
        }

        public void SendToServer(string message)
        {
            byte[] buff = Encoding.UTF8.GetBytes(message);
            if (sock_sv != null)
            {
                sock_sv.BeginSend(buff, 0, buff.Length, SocketFlags.None, new AsyncCallback(SendMessage), buff);
            }
        }

        public void CloseForm(ChattingForm form)
        {
            chatFromList.Remove(form);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LinkedListNode<ChattingForm> form = chatFromList.First;
            if (form == null) { return; }
            while (form != null)
            {
                if (form.Value != null) { form.Value.Close(); }
                form = form.Next;
            }
        }

        private void btn_refresh_Click(object sender, EventArgs e)
        {
            SendToServer("GET_CUSTOMER");
            SelectAllCustomers();
        }

        private void btn_room_info_Click(object sender, EventArgs e)
        {
            try
            {
                roomFrom = new RoomsForm();
                roomFrom.Show();
                SendToServer("GET_ROOM");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "    " + (roomFrom == null), "BTN");
            }
        }

        private void grid_customer_table_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;
        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            try
            {
                EditCustomer customer = new EditCustomer(this);
                Dictionary<int, string> contents = new Dictionary<int, string>();
                for (int i = 0; i < grid_customer_table.ColumnCount; i++)
                {
                    contents.Add(i, grid_customer_table[i, selectedRow].Value.ToString());
                }
                customer.SetValues(contents);
                customer.RoomTextBoxDisable();
                customer.Show();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public void UpdateValues(Dictionary<int, string> contents)
        {
            try
            {
                string cmd = "";
                for (int i = 0; i < grid_customer_table.RowCount - 1; i++)
                {
                    if (grid_customer_table[0, i].Value.ToString() == contents[0])
                    {
                        cmd = "UPDATE\n";
                        for (int j = 0; j < grid_customer_table.ColumnCount; j++)
                        {
                            cmd += contents[j] + "\n";
                            grid_customer_table[j, i].Value = contents[j];
                        }
                        cmd = cmd.Remove(cmd.Length - 1, 1);
                        break;
                    }
                }
                if (cmd == "")
                {
                    cmd = "INSERT\n";
                    for (int i = 0; i < grid_customer_table.ColumnCount; i++)
                    {
                        cmd += contents[i] + "\n";
                    }
                    cmd = cmd.Remove(cmd.Length - 1, 1);
                }
                SendToServer(cmd);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btn_append_customer_Click(object sender, EventArgs e)
        {
            new EditCustomer(this).Show();
        }

        private void btn_delete_customer_Click(object sender, EventArgs e)
        {
            try
            {
                //주의 메시지 창을 띄우고 OK 버튼을 누르면 실행
                if (MessageBox.Show("정말로 삭제하시겠습니까?\n(삭제된 데이터는 복구되지 않습니다.)",
                    "Customer Delete", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    string nid = grid_customer_table[0, selectedRow].Value.ToString();
                    SendToServer("DELETE\n" + nid);
                    grid_customer_table.Rows.Clear();
                    SendToServer("GET_CUSTOMER");
                    SelectAllCustomers();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void SelectAllCustomers()
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection("Data Source = 27.117.199.42,1433; database = HotelDB; uid = sa; pwd = 9d8a7v7i8d2-@");
                List<Customer> customerList = new List<Customer>();
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Customer", con);
                SqlDataReader rd = cmd.ExecuteReader();
                Customer customer;

                while (rd.Read())
                {
                    customer = new Customer();
                    //데이터를 읽어들임
                    customer.nid = rd.GetString(0);
                    customer.name = rd.IsDBNull(1) ? "" : rd.GetString(1);
                    customer.phone = rd.IsDBNull(2) ? "" : rd.GetString(2);
                    customer.age = rd.IsDBNull(3) ? int.MinValue : rd.GetInt32(3);
                    customer.adress = rd.IsDBNull(4) ? "" : rd.GetString(4);
                    customerList.Add(customer);
                }
                //DB닫기
                con.Close();
                con.Open();
                cmd.CommandText = "SELECT * FROM Reservation";
                rd = cmd.ExecuteReader();
                List<Reservation> reservList = new List<Reservation>();
                Reservation reserv;
                while (rd.Read())
                {
                    reserv = new Reservation();
                    //데이터가 Null값이면 -1, 아니면 데이터를 받아옴
                    reserv.rid = rd.IsDBNull(0) ? -1 : rd.GetInt32(0);
                    reserv.nid = rd.IsDBNull(1) ? "" : rd.GetString(1);

                    reservList.Add(reserv);
                }
                //DB닫기
                con.Close();
                //검색 결과가 없으면 null반환
                if (customerList.Count == 0) { return; }

                grid_customer_table.Rows.Clear();
                for (int i = 0; i < customerList.Count; i++)
                {
                    if (customerList[i] == null) { continue; }
                    grid_customer_table.Rows.Add();
                    int j = 0;
                    grid_customer_table[j++, i].Value = customerList[i].nid;
                    grid_customer_table[j++, i].Value = customerList[i].name;
                    grid_customer_table[j++, i].Value = customerList[i].phone;
                    if (customerList[i].age < 0) { grid_customer_table[j++, i].Value = ""; }
                    else { grid_customer_table[j++, i].Value = customerList[i].age; }
                    grid_customer_table[j++, i].Value = customerList[i].adress;
                    for (int k = 0; k < reservList.Count; k++)
                    {
                        if (reservList[k].nid == customerList[i].nid)
                        {
                            grid_customer_table[j, i].Value = reservList[k].rid;
                            break;
                        }
                    }
                }

                return;
            }
            catch (Exception e)
            {
                if (con != null)
                {
                    con.Close();
                }
                MessageBox.Show(e.Message, "Method_SelectAllCustomers");
                return;
            }
        }

    }
}
