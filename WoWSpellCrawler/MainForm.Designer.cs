namespace WoWSpellCrawler
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.cbxProfessions = new System.Windows.Forms.ComboBox();
            this.lblProfessions = new System.Windows.Forms.Label();
            this.rtbResult = new System.Windows.Forms.RichTextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbxProfessions
            // 
            this.cbxProfessions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxProfessions.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbxProfessions.FormattingEnabled = true;
            this.cbxProfessions.Location = new System.Drawing.Point(90, 27);
            this.cbxProfessions.Name = "cbxProfessions";
            this.cbxProfessions.Size = new System.Drawing.Size(162, 24);
            this.cbxProfessions.TabIndex = 0;
            // 
            // lblProfessions
            // 
            this.lblProfessions.AutoSize = true;
            this.lblProfessions.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblProfessions.Location = new System.Drawing.Point(28, 32);
            this.lblProfessions.Name = "lblProfessions";
            this.lblProfessions.Size = new System.Drawing.Size(56, 16);
            this.lblProfessions.TabIndex = 1;
            this.lblProfessions.Text = "类别：";
            // 
            // rtbResult
            // 
            this.rtbResult.AcceptsTab = true;
            this.rtbResult.Location = new System.Drawing.Point(24, 77);
            this.rtbResult.Name = "rtbResult";
            this.rtbResult.Size = new System.Drawing.Size(816, 581);
            this.rtbResult.TabIndex = 2;
            this.rtbResult.Text = "";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(677, 27);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(98, 28);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 680);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.rtbResult);
            this.Controls.Add(this.lblProfessions);
            this.Controls.Add(this.cbxProfessions);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WoWSpellCrawler";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbxProfessions;
        private System.Windows.Forms.Label lblProfessions;
        private System.Windows.Forms.RichTextBox rtbResult;
        private System.Windows.Forms.Button btnStart;
    }
}

