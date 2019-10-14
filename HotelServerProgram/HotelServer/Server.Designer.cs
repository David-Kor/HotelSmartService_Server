namespace HotelServer
{
    partial class Server
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
            this.display1 = new System.Windows.Forms.RichTextBox();
            this.btn_clear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // display1
            // 
            this.display1.Location = new System.Drawing.Point(12, 13);
            this.display1.Name = "display1";
            this.display1.ReadOnly = true;
            this.display1.Size = new System.Drawing.Size(614, 304);
            this.display1.TabIndex = 7;
            this.display1.Text = "";
            this.display1.TextChanged += new System.EventHandler(this.display1_TextChanged);
            // 
            // btn_clear
            // 
            this.btn_clear.Location = new System.Drawing.Point(632, 13);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(104, 46);
            this.btn_clear.TabIndex = 8;
            this.btn_clear.Text = "지우기";
            this.btn_clear.UseVisualStyleBackColor = true;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 329);
            this.Controls.Add(this.btn_clear);
            this.Controls.Add(this.display1);
            this.Name = "Server";
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Server_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox display1;
        private System.Windows.Forms.Button btn_clear;
    }
}

