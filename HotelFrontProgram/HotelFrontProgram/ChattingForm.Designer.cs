namespace HotelFrontProgram
{
    partial class ChattingForm
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
            this.txtbx_input = new System.Windows.Forms.TextBox();
            this.btn_send = new System.Windows.Forms.Button();
            this.txt_chat_display = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtbx_input
            // 
            this.txtbx_input.Location = new System.Drawing.Point(36, 390);
            this.txtbx_input.MaxLength = 1000;
            this.txtbx_input.Name = "txtbx_input";
            this.txtbx_input.Size = new System.Drawing.Size(577, 21);
            this.txtbx_input.TabIndex = 0;
            this.txtbx_input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtbx_input_KeyDown);
            // 
            // btn_send
            // 
            this.btn_send.Location = new System.Drawing.Point(638, 390);
            this.btn_send.Name = "btn_send";
            this.btn_send.Size = new System.Drawing.Size(75, 21);
            this.btn_send.TabIndex = 1;
            this.btn_send.Text = "전송";
            this.btn_send.UseVisualStyleBackColor = true;
            this.btn_send.Click += new System.EventHandler(this.btn_send_Click);
            // 
            // txt_chat_display
            // 
            this.txt_chat_display.Location = new System.Drawing.Point(36, 26);
            this.txt_chat_display.Name = "txt_chat_display";
            this.txt_chat_display.ReadOnly = true;
            this.txt_chat_display.Size = new System.Drawing.Size(577, 339);
            this.txt_chat_display.TabIndex = 2;
            this.txt_chat_display.Text = "";
            this.txt_chat_display.TextChanged += new System.EventHandler(this.txt_chat_display_TextChanged);
            // 
            // ChattingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txt_chat_display);
            this.Controls.Add(this.btn_send);
            this.Controls.Add(this.txtbx_input);
            this.Name = "ChattingForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChattingForm_FormClosing);
            this.Load += new System.EventHandler(this.ChattingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtbx_input;
        private System.Windows.Forms.Button btn_send;
        private System.Windows.Forms.RichTextBox txt_chat_display;
    }
}