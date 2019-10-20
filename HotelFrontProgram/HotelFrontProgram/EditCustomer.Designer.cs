namespace HotelFrontProgram
{
    partial class EditCustomer
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
            this.txt_nid = new System.Windows.Forms.TextBox();
            this.lb_nid = new System.Windows.Forms.Label();
            this.lb_name = new System.Windows.Forms.Label();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.txt_phone = new System.Windows.Forms.TextBox();
            this.lb_phone = new System.Windows.Forms.Label();
            this.txt_age = new System.Windows.Forms.TextBox();
            this.lb_age = new System.Windows.Forms.Label();
            this.txt_adress = new System.Windows.Forms.TextBox();
            this.lb_adress = new System.Windows.Forms.Label();
            this.txt_room = new System.Windows.Forms.TextBox();
            this.lb_room = new System.Windows.Forms.Label();
            this.btn_positive = new System.Windows.Forms.Button();
            this.btn_nagative = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_nid
            // 
            this.txt_nid.Location = new System.Drawing.Point(26, 28);
            this.txt_nid.Name = "txt_nid";
            this.txt_nid.Size = new System.Drawing.Size(251, 21);
            this.txt_nid.TabIndex = 0;
            // 
            // lb_nid
            // 
            this.lb_nid.AutoSize = true;
            this.lb_nid.Location = new System.Drawing.Point(24, 13);
            this.lb_nid.Name = "lb_nid";
            this.lb_nid.Size = new System.Drawing.Size(45, 12);
            this.lb_nid.TabIndex = 1;
            this.lb_nid.Text = "NFC ID";
            // 
            // lb_name
            // 
            this.lb_name.AutoSize = true;
            this.lb_name.Location = new System.Drawing.Point(24, 68);
            this.lb_name.Name = "lb_name";
            this.lb_name.Size = new System.Drawing.Size(39, 12);
            this.lb_name.TabIndex = 1;
            this.lb_name.Text = "Name";
            // 
            // txt_name
            // 
            this.txt_name.Location = new System.Drawing.Point(26, 83);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(251, 21);
            this.txt_name.TabIndex = 1;
            // 
            // txt_phone
            // 
            this.txt_phone.Location = new System.Drawing.Point(26, 151);
            this.txt_phone.Name = "txt_phone";
            this.txt_phone.Size = new System.Drawing.Size(251, 21);
            this.txt_phone.TabIndex = 2;
            // 
            // lb_phone
            // 
            this.lb_phone.AutoSize = true;
            this.lb_phone.Location = new System.Drawing.Point(24, 136);
            this.lb_phone.Name = "lb_phone";
            this.lb_phone.Size = new System.Drawing.Size(90, 12);
            this.lb_phone.TabIndex = 1;
            this.lb_phone.Text = "Phone Number";
            // 
            // txt_age
            // 
            this.txt_age.Location = new System.Drawing.Point(26, 215);
            this.txt_age.Name = "txt_age";
            this.txt_age.Size = new System.Drawing.Size(251, 21);
            this.txt_age.TabIndex = 3;
            // 
            // lb_age
            // 
            this.lb_age.AutoSize = true;
            this.lb_age.Location = new System.Drawing.Point(24, 200);
            this.lb_age.Name = "lb_age";
            this.lb_age.Size = new System.Drawing.Size(27, 12);
            this.lb_age.TabIndex = 1;
            this.lb_age.Text = "Age";
            // 
            // txt_adress
            // 
            this.txt_adress.Location = new System.Drawing.Point(26, 283);
            this.txt_adress.Name = "txt_adress";
            this.txt_adress.Size = new System.Drawing.Size(251, 21);
            this.txt_adress.TabIndex = 4;
            // 
            // lb_adress
            // 
            this.lb_adress.AutoSize = true;
            this.lb_adress.Location = new System.Drawing.Point(24, 268);
            this.lb_adress.Name = "lb_adress";
            this.lb_adress.Size = new System.Drawing.Size(52, 12);
            this.lb_adress.TabIndex = 1;
            this.lb_adress.Text = "Address";
            // 
            // txt_room
            // 
            this.txt_room.Location = new System.Drawing.Point(26, 349);
            this.txt_room.Name = "txt_room";
            this.txt_room.Size = new System.Drawing.Size(251, 21);
            this.txt_room.TabIndex = 5;
            // 
            // lb_room
            // 
            this.lb_room.AutoSize = true;
            this.lb_room.Location = new System.Drawing.Point(24, 334);
            this.lb_room.Name = "lb_room";
            this.lb_room.Size = new System.Drawing.Size(38, 12);
            this.lb_room.TabIndex = 1;
            this.lb_room.Text = "Room";
            // 
            // btn_positive
            // 
            this.btn_positive.Location = new System.Drawing.Point(324, 30);
            this.btn_positive.Name = "btn_positive";
            this.btn_positive.Size = new System.Drawing.Size(108, 48);
            this.btn_positive.TabIndex = 6;
            this.btn_positive.Text = "확인";
            this.btn_positive.UseVisualStyleBackColor = true;
            this.btn_positive.Click += new System.EventHandler(this.btn_positive_Click);
            // 
            // btn_nagative
            // 
            this.btn_nagative.Location = new System.Drawing.Point(324, 100);
            this.btn_nagative.Name = "btn_nagative";
            this.btn_nagative.Size = new System.Drawing.Size(108, 48);
            this.btn_nagative.TabIndex = 7;
            this.btn_nagative.Text = "취소";
            this.btn_nagative.UseVisualStyleBackColor = true;
            this.btn_nagative.Click += new System.EventHandler(this.btn_nagative_Click);
            // 
            // EditCustomer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 402);
            this.Controls.Add(this.btn_nagative);
            this.Controls.Add(this.btn_positive);
            this.Controls.Add(this.lb_room);
            this.Controls.Add(this.lb_adress);
            this.Controls.Add(this.lb_age);
            this.Controls.Add(this.lb_phone);
            this.Controls.Add(this.lb_name);
            this.Controls.Add(this.lb_nid);
            this.Controls.Add(this.txt_room);
            this.Controls.Add(this.txt_adress);
            this.Controls.Add(this.txt_age);
            this.Controls.Add(this.txt_phone);
            this.Controls.Add(this.txt_name);
            this.Controls.Add(this.txt_nid);
            this.Name = "EditCustomer";
            this.Text = "EditCustomer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_nid;
        private System.Windows.Forms.Label lb_nid;
        private System.Windows.Forms.Label lb_name;
        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.TextBox txt_phone;
        private System.Windows.Forms.Label lb_phone;
        private System.Windows.Forms.TextBox txt_age;
        private System.Windows.Forms.Label lb_age;
        private System.Windows.Forms.TextBox txt_adress;
        private System.Windows.Forms.Label lb_adress;
        private System.Windows.Forms.TextBox txt_room;
        private System.Windows.Forms.Label lb_room;
        private System.Windows.Forms.Button btn_positive;
        private System.Windows.Forms.Button btn_nagative;
    }
}