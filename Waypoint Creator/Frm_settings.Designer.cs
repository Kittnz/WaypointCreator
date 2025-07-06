namespace Frm_waypoint
{
    partial class frm_Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_Settings));
            this.groupSQL = new System.Windows.Forms.GroupBox();
            this.chkBoxVMaNGOS = new System.Windows.Forms.CheckBox();
            this.chkBoxCMaNGOS = new System.Windows.Forms.CheckBox();
            this.chkBoxTrinityCore = new System.Windows.Forms.CheckBox();
            this.chkBoxCPP = new System.Windows.Forms.CheckBox();
            this.groupGraph = new System.Windows.Forms.GroupBox();
            this.chkBoxSpline = new System.Windows.Forms.CheckBox();
            this.chkBoxLine = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupColours = new System.Windows.Forms.GroupBox();
            this.btnPointColour = new System.Windows.Forms.Button();
            this.btnLineColour = new System.Windows.Forms.Button();
            this.btnBackColour = new System.Windows.Forms.Button();
            this.btnTitleColour = new System.Windows.Forms.Button();
            this.picBoxPointColour = new System.Windows.Forms.PictureBox();
            this.picBoxLineColour = new System.Windows.Forms.PictureBox();
            this.picBoxBackColour = new System.Windows.Forms.PictureBox();
            this.picBoxTitleColour = new System.Windows.Forms.PictureBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.btnDefault = new System.Windows.Forms.Button();
            this.groupSQL.SuspendLayout();
            this.groupGraph.SuspendLayout();
            this.groupColours.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxPointColour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLineColour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxBackColour)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxTitleColour)).BeginInit();
            this.SuspendLayout();
            // 
            // groupSQL
            // 
            this.groupSQL.Controls.Add(this.chkBoxVMaNGOS);
            this.groupSQL.Controls.Add(this.chkBoxCMaNGOS);
            this.groupSQL.Controls.Add(this.chkBoxTrinityCore);
            this.groupSQL.Controls.Add(this.chkBoxCPP);
            this.groupSQL.Location = new System.Drawing.Point(16, 15);
            this.groupSQL.Margin = new System.Windows.Forms.Padding(4);
            this.groupSQL.Name = "groupSQL";
            this.groupSQL.Padding = new System.Windows.Forms.Padding(4);
            this.groupSQL.Size = new System.Drawing.Size(224, 138);
            this.groupSQL.TabIndex = 5;
            this.groupSQL.TabStop = false;
            this.groupSQL.Text = "Output Format";
            // 
            // chkBoxVMaNGOS
            // 
            this.chkBoxVMaNGOS.AutoSize = true;
            this.chkBoxVMaNGOS.Location = new System.Drawing.Point(8, 23);
            this.chkBoxVMaNGOS.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxVMaNGOS.Name = "chkBoxVMaNGOS";
            this.chkBoxVMaNGOS.Size = new System.Drawing.Size(125, 20);
            this.chkBoxVMaNGOS.TabIndex = 6;
            this.chkBoxVMaNGOS.Text = "VMaNGOS SQL";
            this.chkBoxVMaNGOS.UseVisualStyleBackColor = true;
            this.chkBoxVMaNGOS.CheckedChanged += new System.EventHandler(this.SQLFormat_CheckedChanged);
            // 
            // chkBoxCMaNGOS
            // 
            this.chkBoxCMaNGOS.AutoSize = true;
            this.chkBoxCMaNGOS.Location = new System.Drawing.Point(8, 51);
            this.chkBoxCMaNGOS.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxCMaNGOS.Name = "chkBoxCMaNGOS";
            this.chkBoxCMaNGOS.Size = new System.Drawing.Size(155, 20);
            this.chkBoxCMaNGOS.TabIndex = 3;
            this.chkBoxCMaNGOS.Text = "CMaNGOS Path SQL";
            this.chkBoxCMaNGOS.UseVisualStyleBackColor = true;
            this.chkBoxCMaNGOS.CheckedChanged += new System.EventHandler(this.SQLFormat_CheckedChanged);
            // 
            // chkBoxTrinityCore
            // 
            this.chkBoxTrinityCore.AutoSize = true;
            this.chkBoxTrinityCore.Location = new System.Drawing.Point(8, 79);
            this.chkBoxTrinityCore.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxTrinityCore.Name = "chkBoxTrinityCore";
            this.chkBoxTrinityCore.Size = new System.Drawing.Size(153, 20);
            this.chkBoxTrinityCore.TabIndex = 3;
            this.chkBoxTrinityCore.Text = "TrinityCore Path SQL";
            this.chkBoxTrinityCore.UseVisualStyleBackColor = true;
            this.chkBoxTrinityCore.CheckedChanged += new System.EventHandler(this.SQLFormat_CheckedChanged);
            // 
            // chkBoxCPP
            // 
            this.chkBoxCPP.AutoSize = true;
            this.chkBoxCPP.Location = new System.Drawing.Point(8, 107);
            this.chkBoxCPP.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxCPP.Name = "chkBoxCPP";
            this.chkBoxCPP.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.chkBoxCPP.Size = new System.Drawing.Size(158, 20);
            this.chkBoxCPP.TabIndex = 5;
            this.chkBoxCPP.Text = "C++ Position Constant";
            this.chkBoxCPP.UseVisualStyleBackColor = true;
            this.chkBoxCPP.CheckedChanged += new System.EventHandler(this.SQLFormat_CheckedChanged);
            // 
            // groupGraph
            // 
            this.groupGraph.Controls.Add(this.chkBoxSpline);
            this.groupGraph.Controls.Add(this.chkBoxLine);
            this.groupGraph.Location = new System.Drawing.Point(16, 161);
            this.groupGraph.Margin = new System.Windows.Forms.Padding(4);
            this.groupGraph.Name = "groupGraph";
            this.groupGraph.Padding = new System.Windows.Forms.Padding(4);
            this.groupGraph.Size = new System.Drawing.Size(224, 75);
            this.groupGraph.TabIndex = 6;
            this.groupGraph.TabStop = false;
            this.groupGraph.Text = "Point Graphing";
            // 
            // chkBoxSpline
            // 
            this.chkBoxSpline.AutoSize = true;
            this.chkBoxSpline.Location = new System.Drawing.Point(8, 52);
            this.chkBoxSpline.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxSpline.Name = "chkBoxSpline";
            this.chkBoxSpline.Size = new System.Drawing.Size(121, 20);
            this.chkBoxSpline.TabIndex = 6;
            this.chkBoxSpline.Text = "Linetype Spline";
            this.chkBoxSpline.UseVisualStyleBackColor = true;
            this.chkBoxSpline.Enabled = false;
            // 
            // chkBoxLine
            // 
            this.chkBoxLine.AutoSize = true;
            this.chkBoxLine.Location = new System.Drawing.Point(8, 23);
            this.chkBoxLine.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxLine.Name = "chkBoxLine";
            this.chkBoxLine.Size = new System.Drawing.Size(97, 20);
            this.chkBoxLine.TabIndex = 5;
            this.chkBoxLine.Text = "Show Lines";
            this.chkBoxLine.UseVisualStyleBackColor = true;
            this.chkBoxLine.CheckedChanged += new System.EventHandler(this.ChkBoxLine_CheckedChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(140, 244);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 28);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.btnCancel.Location = new System.Drawing.Point(140, 280);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // groupColours
            // 
            this.groupColours.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.groupColours.Controls.Add(this.btnPointColour);
            this.groupColours.Controls.Add(this.btnLineColour);
            this.groupColours.Controls.Add(this.btnBackColour);
            this.groupColours.Controls.Add(this.btnTitleColour);
            this.groupColours.Controls.Add(this.picBoxPointColour);
            this.groupColours.Controls.Add(this.picBoxLineColour);
            this.groupColours.Controls.Add(this.picBoxBackColour);
            this.groupColours.Controls.Add(this.picBoxTitleColour);
            this.groupColours.Location = new System.Drawing.Point(251, 15);
            this.groupColours.Margin = new System.Windows.Forms.Padding(4);
            this.groupColours.Name = "groupColours";
            this.groupColours.Padding = new System.Windows.Forms.Padding(4);
            this.groupColours.Size = new System.Drawing.Size(181, 175);
            this.groupColours.TabIndex = 9;
            this.groupColours.TabStop = false;
            this.groupColours.Text = "Graph Colours";
            // 
            // btnPointColour
            // 
            this.btnPointColour.Location = new System.Drawing.Point(55, 27);
            this.btnPointColour.Margin = new System.Windows.Forms.Padding(4);
            this.btnPointColour.Name = "btnPointColour";
            this.btnPointColour.Size = new System.Drawing.Size(113, 28);
            this.btnPointColour.TabIndex = 12;
            this.btnPointColour.Text = "Point Colour";
            this.btnPointColour.UseVisualStyleBackColor = true;
            this.btnPointColour.Click += new System.EventHandler(this.BtnPointColour_Click);
            // 
            // btnLineColour
            // 
            this.btnLineColour.Location = new System.Drawing.Point(55, 63);
            this.btnLineColour.Margin = new System.Windows.Forms.Padding(4);
            this.btnLineColour.Name = "btnLineColour";
            this.btnLineColour.Size = new System.Drawing.Size(113, 28);
            this.btnLineColour.TabIndex = 13;
            this.btnLineColour.Text = "Line Colour";
            this.btnLineColour.UseVisualStyleBackColor = true;
            this.btnLineColour.Click += new System.EventHandler(this.BtnLineColour_Click);
            // 
            // btnBackColour
            // 
            this.btnBackColour.Location = new System.Drawing.Point(55, 98);
            this.btnBackColour.Margin = new System.Windows.Forms.Padding(4);
            this.btnBackColour.Name = "btnBackColour";
            this.btnBackColour.Size = new System.Drawing.Size(113, 28);
            this.btnBackColour.TabIndex = 15;
            this.btnBackColour.Text = "Back Colour";
            this.btnBackColour.UseVisualStyleBackColor = true;
            this.btnBackColour.Click += new System.EventHandler(this.BtnBackColour_Click);
            // 
            // btnTitleColour
            // 
            this.btnTitleColour.Location = new System.Drawing.Point(55, 134);
            this.btnTitleColour.Margin = new System.Windows.Forms.Padding(4);
            this.btnTitleColour.Name = "btnTitleColour";
            this.btnTitleColour.Size = new System.Drawing.Size(113, 28);
            this.btnTitleColour.TabIndex = 17;
            this.btnTitleColour.Text = "Title Colour";
            this.btnTitleColour.UseVisualStyleBackColor = true;
            this.btnTitleColour.Click += new System.EventHandler(this.BtnTitleColour_Click);
            // 
            // picBoxPointColour
            // 
            this.picBoxPointColour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoxPointColour.Location = new System.Drawing.Point(8, 27);
            this.picBoxPointColour.Margin = new System.Windows.Forms.Padding(4);
            this.picBoxPointColour.Name = "picBoxPointColour";
            this.picBoxPointColour.Size = new System.Drawing.Size(30, 28);
            this.picBoxPointColour.TabIndex = 0;
            this.picBoxPointColour.TabStop = false;
            // 
            // picBoxLineColour
            // 
            this.picBoxLineColour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoxLineColour.Location = new System.Drawing.Point(8, 63);
            this.picBoxLineColour.Margin = new System.Windows.Forms.Padding(4);
            this.picBoxLineColour.Name = "picBoxLineColour";
            this.picBoxLineColour.Size = new System.Drawing.Size(30, 28);
            this.picBoxLineColour.TabIndex = 2;
            this.picBoxLineColour.TabStop = false;
            // 
            // picBoxBackColour
            // 
            this.picBoxBackColour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoxBackColour.Location = new System.Drawing.Point(8, 98);
            this.picBoxBackColour.Margin = new System.Windows.Forms.Padding(4);
            this.picBoxBackColour.Name = "picBoxBackColour";
            this.picBoxBackColour.Size = new System.Drawing.Size(30, 28);
            this.picBoxBackColour.TabIndex = 14;
            this.picBoxBackColour.TabStop = false;
            // 
            // picBoxTitleColour
            // 
            this.picBoxTitleColour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoxTitleColour.Location = new System.Drawing.Point(8, 134);
            this.picBoxTitleColour.Margin = new System.Windows.Forms.Padding(4);
            this.picBoxTitleColour.Name = "picBoxTitleColour";
            this.picBoxTitleColour.Size = new System.Drawing.Size(30, 28);
            this.picBoxTitleColour.TabIndex = 16;
            this.picBoxTitleColour.TabStop = false;
            // 
            // btnDefault
            // 
            this.btnDefault.Location = new System.Drawing.Point(276, 208);
            this.btnDefault.Margin = new System.Windows.Forms.Padding(4);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(156, 28);
            this.btnDefault.TabIndex = 10;
            this.btnDefault.Text = "Reset Default Values";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.BtnDefault_Click);
            // 
            // frm_Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 356);
            this.ControlBox = false;
            this.Controls.Add(this.groupSQL);
            this.Controls.Add(this.groupGraph);
            this.Controls.Add(this.groupColours);
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frm_Settings";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Frm_settings_Load);
            this.groupSQL.ResumeLayout(false);
            this.groupSQL.PerformLayout();
            this.groupGraph.ResumeLayout(false);
            this.groupGraph.PerformLayout();
            this.groupColours.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBoxPointColour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLineColour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxBackColour)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxTitleColour)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupSQL;
        private System.Windows.Forms.CheckBox chkBoxVMaNGOS;
        private System.Windows.Forms.CheckBox chkBoxCMaNGOS;
        private System.Windows.Forms.CheckBox chkBoxTrinityCore;
        private System.Windows.Forms.CheckBox chkBoxCPP;
        private System.Windows.Forms.GroupBox groupGraph;
        private System.Windows.Forms.CheckBox chkBoxSpline;
        private System.Windows.Forms.CheckBox chkBoxLine;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupColours;
        private System.Windows.Forms.PictureBox picBoxLineColour;
        private System.Windows.Forms.PictureBox picBoxPointColour;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button btnLineColour;
        private System.Windows.Forms.Button btnPointColour;
        private System.Windows.Forms.Button btnTitleColour;
        private System.Windows.Forms.PictureBox picBoxTitleColour;
        private System.Windows.Forms.Button btnBackColour;
        private System.Windows.Forms.PictureBox picBoxBackColour;
        private System.Windows.Forms.Button btnDefault;
    }
}