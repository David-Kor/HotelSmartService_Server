namespace HotelFrontProgram
{
    partial class RoomsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grid_room_table = new System.Windows.Forms.DataGridView();
            this.rid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rStartDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rEndDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rTemp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rHumid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rDust = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grid_room_table)).BeginInit();
            this.SuspendLayout();
            // 
            // grid_room_table
            // 
            this.grid_room_table.AllowUserToAddRows = false;
            this.grid_room_table.AllowUserToDeleteRows = false;
            this.grid_room_table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_room_table.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.rid,
            this.rStartDate,
            this.rEndDate,
            this.rTemp,
            this.rHumid,
            this.rDust});
            this.grid_room_table.Location = new System.Drawing.Point(12, 12);
            this.grid_room_table.Name = "grid_room_table";
            this.grid_room_table.ReadOnly = true;
            this.grid_room_table.RowTemplate.Height = 23;
            this.grid_room_table.Size = new System.Drawing.Size(743, 211);
            this.grid_room_table.TabIndex = 0;
            // 
            // rid
            // 
            this.rid.Frozen = true;
            this.rid.HeaderText = "방번호";
            this.rid.Name = "rid";
            this.rid.ReadOnly = true;
            // 
            // rStartDate
            // 
            this.rStartDate.Frozen = true;
            this.rStartDate.HeaderText = "체크인";
            this.rStartDate.Name = "rStartDate";
            this.rStartDate.ReadOnly = true;
            // 
            // rEndDate
            // 
            this.rEndDate.Frozen = true;
            this.rEndDate.HeaderText = "체크아웃";
            this.rEndDate.Name = "rEndDate";
            this.rEndDate.ReadOnly = true;
            // 
            // rTemp
            // 
            this.rTemp.Frozen = true;
            this.rTemp.HeaderText = "온도";
            this.rTemp.Name = "rTemp";
            this.rTemp.ReadOnly = true;
            // 
            // rHumid
            // 
            this.rHumid.Frozen = true;
            this.rHumid.HeaderText = "습도";
            this.rHumid.Name = "rHumid";
            this.rHumid.ReadOnly = true;
            // 
            // rDust
            // 
            this.rDust.Frozen = true;
            this.rDust.HeaderText = "먼지농도";
            this.rDust.Name = "rDust";
            this.rDust.ReadOnly = true;
            // 
            // RoomsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(767, 235);
            this.Controls.Add(this.grid_room_table);
            this.Name = "RoomsForm";
            this.Text = "RoomsForm";
            ((System.ComponentModel.ISupportInitialize)(this.grid_room_table)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grid_room_table;
        private System.Windows.Forms.DataGridViewTextBoxColumn rid;
        private System.Windows.Forms.DataGridViewTextBoxColumn rStartDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn rEndDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn rTemp;
        private System.Windows.Forms.DataGridViewTextBoxColumn rHumid;
        private System.Windows.Forms.DataGridViewTextBoxColumn rDust;
    }
}