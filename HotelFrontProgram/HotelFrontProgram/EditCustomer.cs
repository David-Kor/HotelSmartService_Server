using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace HotelFrontProgram
{
    public partial class EditCustomer : Form
    {
        private HotelFront parentForm;
        private Mutex mutex;

        public EditCustomer(HotelFront calledForm)
        {
            InitializeComponent();
            parentForm = calledForm;
            mutex = new Mutex();
        }

        //폼 생성 시 호출
        private void EditCustomer_Load(object sender, EventArgs e)
        {
            RefreshCombobox();
            if (comboBox_serialPorts.Items.Count > 0)
            {
                comboBox_serialPorts.SelectedIndex = 0;
            }
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

        //취소버튼 클릭 시 호출
        private void btn_nagative_Click(object sender, EventArgs e)
        {
            Close();
        }

        //확인 버튼 클릭 시 호출
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

        //숫자 전용 입력 란에서 키입력 이벤트 발생 시 호출
        //오직 숫자만 입력가능하게 함
        private void OnlyNum_Txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            //숫자 또는 백스페이스 키가 아니라면 입력 무효
            if ((char.IsDigit(e.KeyChar) || e.KeyChar == (char)Keys.Back) == false)
            {
                e.Handled = true;
            }
        }

        private void RefreshCombobox()
        {
            try
            {
                mutex.WaitOne();
                comboBox_serialPorts.Text = string.Empty;
                comboBox_serialPorts.SelectedItem = null;
                MessageBox.Show(comboBox_serialPorts.SelectedIndex.ToString());
                comboBox_serialPorts.Items.Clear();
                string[] strSerials = SerialPort.GetPortNames();
                comboBox_serialPorts.Items.AddRange(strSerials);
                mutex.ReleaseMutex();
            }
            catch (Exception) { }
        }

        //시리얼 포트 선택 콤보박스를 드롭다운했을 경우 호출
        private void ComboBox_serialPorts_DropDown(object sender, EventArgs e)
        {
            RefreshCombobox();
        }

        //USB 디바이스에 변경이 있으면 호출
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == 0x0219)
                {
                    //string strSelected = comboBox_serialPorts.Text;
                    RefreshCombobox();
                    //if (comboBox_serialPorts.Items.Count > 0)
                    //{
                    //    if (comboBox_serialPorts.Items.Contains(strSelected))
                    //    {
                    //        comboBox_serialPorts.SelectedItem = strSelected;
                    //    }
                    //    else
                    //    {
                    //        comboBox_serialPorts.SelectedIndex = 0;
                    //    }
                    //}
                }
            }
            catch (Exception) { }
            finally
            {
                base.WndProc(ref m);
            }
        }

        //콤보박스에서 시리얼 포트를 선택할 시 호출
        private void ComboBox_serialPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBox_serialPorts.SelectedItem == null)
                {
                    return;
                }
                if (comboBox_serialPorts.Items.Count > 0)
                {
                    serialPort.Close();
                    serialPort.PortName = comboBox_serialPorts.SelectedItem.ToString();
                    serialPort.Open();
                }
            }
            catch (Exception except)
            {
                //MessageBox.Show(except.Message);
            }
        }

        //시리얼 포트에서 메시지를 수신받을 시 호출
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                txt_nid.Text = serialPort.ReadLine().Split(':')[1];
            }
            catch (Exception) { }
        }
    }
}
