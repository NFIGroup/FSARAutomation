namespace FSAR_Automation
{
    partial class SalesReleaseVINSelection
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Model_ComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.CustomerName_txtbx = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Search_Button = new System.Windows.Forms.Button();
            this.SR_Combobox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.VIN_Textbox = new System.Windows.Forms.TextBox();
            this.DataGridView = new System.Windows.Forms.DataGridView();
            this.ClearAll_CheckBox = new System.Windows.Forms.CheckBox();
            this.SelectAll_Checkbox = new System.Windows.Forms.CheckBox();
            this.Build_Button = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Model_ComboBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.CustomerName_txtbx);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.Search_Button);
            this.groupBox1.Controls.Add(this.SR_Combobox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.VIN_Textbox);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 81);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filters";
            // 
            // Model_ComboBox
            // 
            this.Model_ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Model_ComboBox.FormattingEnabled = true;
            this.Model_ComboBox.Location = new System.Drawing.Point(272, 23);
            this.Model_ComboBox.Name = "Model_ComboBox";
            this.Model_ComboBox.Size = new System.Drawing.Size(100, 21);
            this.Model_ComboBox.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Customer Name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select SR";
            // 
            // CustomerName_txtbx
            // 
            this.CustomerName_txtbx.Location = new System.Drawing.Point(88, 23);
            this.CustomerName_txtbx.Name = "CustomerName_txtbx";
            this.CustomerName_txtbx.Size = new System.Drawing.Size(100, 20);
            this.CustomerName_txtbx.TabIndex = 9;
            this.CustomerName_txtbx.MouseLeave += new System.EventHandler(this.CustomerName_txtbx_MouseLeave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(199, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Model Name";
            // 
            // Search_Button
            // 
            this.Search_Button.Location = new System.Drawing.Point(389, 34);
            this.Search_Button.Name = "Search_Button";
            this.Search_Button.Size = new System.Drawing.Size(75, 23);
            this.Search_Button.TabIndex = 5;
            this.Search_Button.Text = "Search";
            this.Search_Button.UseVisualStyleBackColor = true;
            this.Search_Button.Click += new System.EventHandler(this.Search_Button_Click);
            // 
            // SR_Combobox
            // 
            this.SR_Combobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SR_Combobox.FormattingEnabled = true;
            this.SR_Combobox.Location = new System.Drawing.Point(88, 50);
            this.SR_Combobox.Name = "SR_Combobox";
            this.SR_Combobox.Size = new System.Drawing.Size(100, 21);
            this.SR_Combobox.TabIndex = 0;
            this.SR_Combobox.DropDown += new System.EventHandler(this.SR_Combobox_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(241, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "VIN";
            // 
            // VIN_Textbox
            // 
            this.VIN_Textbox.Location = new System.Drawing.Point(272, 50);
            this.VIN_Textbox.Name = "VIN_Textbox";
            this.VIN_Textbox.Size = new System.Drawing.Size(98, 20);
            this.VIN_Textbox.TabIndex = 3;
            // 
            // DataGridView
            // 
            this.DataGridView.AllowUserToAddRows = false;
            this.DataGridView.AllowUserToDeleteRows = false;
            this.DataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            this.DataGridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.DataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView.GridColor = System.Drawing.SystemColors.ButtonHighlight;
            this.DataGridView.Location = new System.Drawing.Point(12, 123);
            this.DataGridView.Name = "DataGridView";
            this.DataGridView.RowHeadersWidth = 5;
            this.DataGridView.Size = new System.Drawing.Size(464, 406);
            this.DataGridView.TabIndex = 8;
            this.DataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1CellContentClick);
            // 
            // ClearAll_CheckBox
            // 
            this.ClearAll_CheckBox.AutoSize = true;
            this.ClearAll_CheckBox.Location = new System.Drawing.Point(390, 99);
            this.ClearAll_CheckBox.Name = "ClearAll_CheckBox";
            this.ClearAll_CheckBox.Size = new System.Drawing.Size(64, 17);
            this.ClearAll_CheckBox.TabIndex = 10;
            this.ClearAll_CheckBox.Text = "Clear All";
            this.ClearAll_CheckBox.UseVisualStyleBackColor = true;
            this.ClearAll_CheckBox.CheckedChanged += new System.EventHandler(this.ClearAll_CheckBox_CheckedChanged);
            // 
            // SelectAll_Checkbox
            // 
            this.SelectAll_Checkbox.AutoSize = true;
            this.SelectAll_Checkbox.Location = new System.Drawing.Point(314, 99);
            this.SelectAll_Checkbox.Name = "SelectAll_Checkbox";
            this.SelectAll_Checkbox.Size = new System.Drawing.Size(70, 17);
            this.SelectAll_Checkbox.TabIndex = 9;
            this.SelectAll_Checkbox.Text = "Select All";
            this.SelectAll_Checkbox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.SelectAll_Checkbox.UseVisualStyleBackColor = true;
            this.SelectAll_Checkbox.CheckedChanged += new System.EventHandler(this.SelectAll_Checkbox_CheckedChanged);
            // 
            // Build_Button
            // 
            this.Build_Button.Location = new System.Drawing.Point(355, 537);
            this.Build_Button.Name = "Build_Button";
            this.Build_Button.Size = new System.Drawing.Size(75, 23);
            this.Build_Button.TabIndex = 11;
            this.Build_Button.Text = "Build";
            this.Build_Button.UseVisualStyleBackColor = true;
            this.Build_Button.Click += new System.EventHandler(this.Build_Button_Click);
            // 
            // SalesReleaseVINSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 572);
            this.Controls.Add(this.Build_Button);
            this.Controls.Add(this.ClearAll_CheckBox);
            this.Controls.Add(this.SelectAll_Checkbox);
            this.Controls.Add(this.DataGridView);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SalesReleaseVINSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select SR and VIN";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SalesReleaseVINSelection_FormClosed);
            this.Load += new System.EventHandler(this.IncidentVinFormLoad);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Search_Button;
        public System.Windows.Forms.ComboBox SR_Combobox;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox VIN_Textbox;        
        private System.Windows.Forms.DataGridView DataGridView;
        public System.Windows.Forms.CheckBox ClearAll_CheckBox;
        public System.Windows.Forms.CheckBox SelectAll_Checkbox;
        public System.Windows.Forms.Button Build_Button;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox CustomerName_txtbx;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox Model_ComboBox;
    }
}