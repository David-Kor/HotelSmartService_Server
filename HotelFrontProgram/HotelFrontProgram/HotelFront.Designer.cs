namespace HotelFrontProgram
{
    partial class HotelFront
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btn_append_customer = new System.Windows.Forms.Button();
            this.btn_refresh = new System.Windows.Forms.Button();
            this.btn_delete_customer = new System.Windows.Forms.Button();
            this.btn_edit = new System.Windows.Forms.Button();
            this.grid_customer_table = new System.Windows.Forms.DataGridView();
            this.cNID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cPhone = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cAge = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cAdress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cRooms = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btn_room_info = new System.Windows.Forms.Button();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.grid_customer_table)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_append_customer
            // 
            this.btn_append_customer.Location = new System.Drawing.Point(12, 291);
            this.btn_append_customer.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btn_append_customer.Name = "btn_append_customer";
            this.btn_append_customer.Size = new System.Drawing.Size(158, 46);
            this.btn_append_customer.TabIndex = 3;
            this.btn_append_customer.Text = "신규 고객 등록";
            this.btn_append_customer.UseVisualStyleBackColor = true;
            this.btn_append_customer.Click += new System.EventHandler(this.btn_append_customer_Click);
            // 
            // btn_refresh
            // 
            this.btn_refresh.Location = new System.Drawing.Point(428, 291);
            this.btn_refresh.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(158, 46);
            this.btn_refresh.TabIndex = 4;
            this.btn_refresh.Text = "새로고침";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
            // 
            // btn_delete_customer
            // 
            this.btn_delete_customer.Location = new System.Drawing.Point(12, 347);
            this.btn_delete_customer.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btn_delete_customer.Name = "btn_delete_customer";
            this.btn_delete_customer.Size = new System.Drawing.Size(158, 46);
            this.btn_delete_customer.TabIndex = 5;
            this.btn_delete_customer.Text = "고객 정보 삭제";
            this.btn_delete_customer.UseVisualStyleBackColor = true;
            this.btn_delete_customer.Click += new System.EventHandler(this.btn_delete_customer_Click);
            // 
            // btn_edit
            // 
            this.btn_edit.Location = new System.Drawing.Point(176, 291);
            this.btn_edit.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btn_edit.Name = "btn_edit";
            this.btn_edit.Size = new System.Drawing.Size(158, 46);
            this.btn_edit.TabIndex = 5;
            this.btn_edit.Text = "고객 정보 수정";
            this.btn_edit.UseVisualStyleBackColor = true;
            this.btn_edit.Click += new System.EventHandler(this.btn_edit_Click);
            // 
            // grid_customer_table
            // 
            this.grid_customer_table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_customer_table.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cNID,
            this.cName,
            this.cPhone,
            this.cAge,
            this.cAdress,
            this.cRooms});
            this.grid_customer_table.Location = new System.Drawing.Point(12, 14);
            this.grid_customer_table.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.grid_customer_table.Name = "grid_customer_table";
            this.grid_customer_table.RowTemplate.Height = 23;
            this.grid_customer_table.Size = new System.Drawing.Size(698, 266);
            this.grid_customer_table.TabIndex = 6;
            this.grid_customer_table.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_customer_table_CellClick);
            // 
            // cNID
            // 
            this.cNID.Frozen = true;
            this.cNID.HeaderText = "NFC ID";
            this.cNID.Name = "cNID";
            this.cNID.ReadOnly = true;
            // 
            // cName
            // 
            this.cName.Frozen = true;
            this.cName.HeaderText = "Name";
            this.cName.Name = "cName";
            this.cName.ReadOnly = true;
            // 
            // cPhone
            // 
            this.cPhone.Frozen = true;
            this.cPhone.HeaderText = "Phone Number";
            this.cPhone.Name = "cPhone";
            this.cPhone.ReadOnly = true;
            // 
            // cAge
            // 
            this.cAge.Frozen = true;
            this.cAge.HeaderText = "Age";
            this.cAge.Name = "cAge";
            this.cAge.ReadOnly = true;
            // 
            // cAdress
            // 
            this.cAdress.Frozen = true;
            this.cAdress.HeaderText = "Adress";
            this.cAdress.Name = "cAdress";
            this.cAdress.ReadOnly = true;
            // 
            // cRooms
            // 
            this.cRooms.Frozen = true;
            this.cRooms.HeaderText = "Rooms";
            this.cRooms.Name = "cRooms";
            this.cRooms.ReadOnly = true;
            // 
            // btn_room_info
            // 
            this.btn_room_info.Location = new System.Drawing.Point(176, 347);
            this.btn_room_info.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.btn_room_info.Name = "btn_room_info";
            this.btn_room_info.Size = new System.Drawing.Size(158, 46);
            this.btn_room_info.TabIndex = 5;
            this.btn_room_info.Text = "객실 현황 보기";
            this.btn_room_info.UseVisualStyleBackColor = true;
            this.btn_room_info.Click += new System.EventHandler(this.btn_room_info_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(506, 361);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(203, 20);
            this.comboBox1.TabIndex = 7;
            // 
            // HotelFront
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 407);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.grid_customer_table);
            this.Controls.Add(this.btn_room_info);
            this.Controls.Add(this.btn_edit);
            this.Controls.Add(this.btn_delete_customer);
            this.Controls.Add(this.btn_refresh);
            this.Controls.Add(this.btn_append_customer);
            this.Name = "HotelFront";
            this.Text = "HotelFront";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grid_customer_table)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_append_customer;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Button btn_delete_customer;
        private System.Windows.Forms.Button btn_edit;
        private System.Windows.Forms.DataGridView grid_customer_table;
        private System.Windows.Forms.Button btn_room_info;
        private System.Windows.Forms.DataGridViewTextBoxColumn cNID;
        private System.Windows.Forms.DataGridViewTextBoxColumn cName;
        private System.Windows.Forms.DataGridViewTextBoxColumn cPhone;
        private System.Windows.Forms.DataGridViewTextBoxColumn cAge;
        private System.Windows.Forms.DataGridViewTextBoxColumn cAdress;
        private System.Windows.Forms.DataGridViewTextBoxColumn cRooms;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

