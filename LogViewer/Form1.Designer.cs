namespace LogViewer
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ChannelDropdown = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.MonthDropdown = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.DayDropdown = new System.Windows.Forms.ComboBox();
            this.LogOutput = new System.Windows.Forms.TextBox();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Enabled = false;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(12, 540);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(422, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Log viewer by Justin Eittreim <eittreim.justin@live.com>   (DivinityArcane @ devi" +
    "antART)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Select a channel:";
            // 
            // ChannelDropdown
            // 
            this.ChannelDropdown.FormattingEnabled = true;
            this.ChannelDropdown.Location = new System.Drawing.Point(109, 10);
            this.ChannelDropdown.Name = "ChannelDropdown";
            this.ChannelDropdown.Size = new System.Drawing.Size(199, 21);
            this.ChannelDropdown.TabIndex = 2;
            this.ChannelDropdown.SelectedIndexChanged += new System.EventHandler(this.ChannelDropdown_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(328, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Select a month:";
            // 
            // MonthDropdown
            // 
            this.MonthDropdown.Enabled = false;
            this.MonthDropdown.FormattingEnabled = true;
            this.MonthDropdown.Location = new System.Drawing.Point(415, 10);
            this.MonthDropdown.Name = "MonthDropdown";
            this.MonthDropdown.Size = new System.Drawing.Size(145, 21);
            this.MonthDropdown.TabIndex = 4;
            this.MonthDropdown.SelectedIndexChanged += new System.EventHandler(this.MonthDropdown_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(576, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Select a day:";
            // 
            // DayDropdown
            // 
            this.DayDropdown.Enabled = false;
            this.DayDropdown.FormattingEnabled = true;
            this.DayDropdown.Location = new System.Drawing.Point(651, 10);
            this.DayDropdown.Name = "DayDropdown";
            this.DayDropdown.Size = new System.Drawing.Size(121, 21);
            this.DayDropdown.TabIndex = 6;
            this.DayDropdown.SelectedIndexChanged += new System.EventHandler(this.DayDropdown_SelectedIndexChanged);
            // 
            // LogOutput
            // 
            this.LogOutput.Enabled = false;
            this.LogOutput.Location = new System.Drawing.Point(12, 37);
            this.LogOutput.Multiline = true;
            this.LogOutput.Name = "LogOutput";
            this.LogOutput.ReadOnly = true;
            this.LogOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogOutput.Size = new System.Drawing.Size(760, 489);
            this.LogOutput.TabIndex = 8;
            // 
            // ReloadButton
            // 
            this.ReloadButton.Location = new System.Drawing.Point(620, 532);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(152, 26);
            this.ReloadButton.TabIndex = 9;
            this.ReloadButton.Text = "Refresh File";
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.ReloadButton);
            this.Controls.Add(this.LogOutput);
            this.Controls.Add(this.DayDropdown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.MonthDropdown);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ChannelDropdown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Log Viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ChannelDropdown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox MonthDropdown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox DayDropdown;
        private System.Windows.Forms.TextBox LogOutput;
        private System.Windows.Forms.Button ReloadButton;
    }
}

