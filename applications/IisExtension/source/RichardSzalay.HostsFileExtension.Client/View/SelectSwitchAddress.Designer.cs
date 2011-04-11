namespace RichardSzalay.HostsFileExtension.Client.View
{
    partial class SelectSwitchAddress
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
            this.addressComboBox = new System.Windows.Forms.ComboBox();
            this.addressLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // addressComboBox
            // 
            this.addressComboBox.FormattingEnabled = true;
            this.addressComboBox.Location = new System.Drawing.Point(109, 18);
            this.addressComboBox.Name = "addressComboBox";
            this.addressComboBox.Size = new System.Drawing.Size(284, 21);
            this.addressComboBox.TabIndex = 1;
            this.addressComboBox.TextChanged += new System.EventHandler(this.addressComboBox_TextChanged);
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Location = new System.Drawing.Point(12, 22);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(48, 13);
            this.addressLabel.TabIndex = 0;
            this.addressLabel.Text = "&Address:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.addressLabel);
            this.panel1.Controls.Add(this.addressComboBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(406, 100);
            this.panel1.TabIndex = 3;
            // 
            // SelectSwitchAddress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 94);
            this.Controls.Add(this.panel1);
            this.Name = "SelectSwitchAddress";
            this.Text = "SelectSwitchAddress";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox addressComboBox;
        private System.Windows.Forms.Label addressLabel;
        private System.Windows.Forms.Panel panel1;
    }
}