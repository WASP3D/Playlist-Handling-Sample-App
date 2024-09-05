namespace BeeSys.Wasp3d.Utilities
{
    partial class frmPlaylist
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
            Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTemplate = new System.Windows.Forms.Panel();
            this.splitterLetf = new System.Windows.Forms.Splitter();
            this.pnlPlaylist = new System.Windows.Forms.Panel();
            this.pnlMainPlaylist = new System.Windows.Forms.Panel();
            this.tvwPlaylist = new System.Windows.Forms.TreeView();
            this.lstBoxInstances = new System.Windows.Forms.ListBox();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.txtGroup = new System.Windows.Forms.TextBox();
            this.btnAddGroup = new System.Windows.Forms.Button();
            this.txtBoxPlaylistslug = new System.Windows.Forms.TextBox();
            this.btnPlaylist = new System.Windows.Forms.Button();
            this.pnlPlaylist.SuspendLayout();
            this.pnlMainPlaylist.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTemplate
            // 
            this.pnlTemplate.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlTemplate.Location = new System.Drawing.Point(0, 0);
            this.pnlTemplate.Name = "pnlTemplate";
            this.pnlTemplate.Size = new System.Drawing.Size(381, 578);
            this.pnlTemplate.TabIndex = 0;
            // 
            // splitter1
            // 
            this.splitterLetf.Location = new System.Drawing.Point(381, 0);
            this.splitterLetf.Name = "splitter1";
            this.splitterLetf.Size = new System.Drawing.Size(10, 578);
            this.splitterLetf.TabIndex = 1;
            this.splitterLetf.TabStop = false;
            // 
            // pnlPlaylist
            // 
            this.pnlPlaylist.Controls.Add(this.pnlMainPlaylist);
            this.pnlPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPlaylist.Location = new System.Drawing.Point(391, 0);
            this.pnlPlaylist.Name = "pnlPlaylist";
            this.pnlPlaylist.Size = new System.Drawing.Size(713, 578);
            this.pnlPlaylist.TabIndex = 2;
            // 
            // panel2
            // 
            this.pnlMainPlaylist.Controls.Add(this.tvwPlaylist);
            this.pnlMainPlaylist.Controls.Add(this.lstBoxInstances);
            this.pnlMainPlaylist.Controls.Add(this.pnlTop);
            this.pnlMainPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainPlaylist.Location = new System.Drawing.Point(0, 0);
            this.pnlMainPlaylist.Name = "panel2";
            this.pnlMainPlaylist.Size = new System.Drawing.Size(713, 578);
            this.pnlMainPlaylist.TabIndex = 1;
            // 
            // TreePlaylist
            // 
            this.tvwPlaylist.AllowDrop = true;
            this.tvwPlaylist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvwPlaylist.Location = new System.Drawing.Point(120, 38);
            this.tvwPlaylist.Name = "TreePlaylist";
            this.tvwPlaylist.Size = new System.Drawing.Size(593, 540);
            this.tvwPlaylist.TabIndex = 0;
            this.tvwPlaylist.DragDrop += new System.Windows.Forms.DragEventHandler(this.TreePlaylist_DragDrop);
            this.tvwPlaylist.DragEnter += new System.Windows.Forms.DragEventHandler(this.TreePlaylist_DragEnter);
            this.tvwPlaylist.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreePlaylist_KeyDown);
            // 
            // lstBoxInstances
            // 
            this.lstBoxInstances.Dock = System.Windows.Forms.DockStyle.Left;
            this.lstBoxInstances.FormattingEnabled = true;
            this.lstBoxInstances.Location = new System.Drawing.Point(0, 38);
            this.lstBoxInstances.Name = "lstBoxInstances";
            this.lstBoxInstances.Size = new System.Drawing.Size(120, 540);
            this.lstBoxInstances.TabIndex = 2;
            this.lstBoxInstances.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lstBoxInstances_MouseMove);
            // 
            // panel3
            // 
            this.pnlTop.Controls.Add(this.txtGroup);
            this.pnlTop.Controls.Add(this.btnAddGroup);
            this.pnlTop.Controls.Add(this.txtBoxPlaylistslug);
            this.pnlTop.Controls.Add(this.btnPlaylist);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "panel3";
            this.pnlTop.Size = new System.Drawing.Size(713, 38);
            this.pnlTop.TabIndex = 1;
            // 
            // txtGroup
            // 
            this.txtGroup.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtGroup.Location = new System.Drawing.Point(286, 12);
            this.txtGroup.Name = "txtGroup";
            this.txtGroup.Size = new System.Drawing.Size(188, 13);
            this.txtGroup.TabIndex = 21;
            // 
            // btnAddGroup
            // 
            this.btnAddGroup.Location = new System.Drawing.Point(480, 6);
            this.btnAddGroup.Name = "btnAddGroup";
            this.btnAddGroup.Size = new System.Drawing.Size(84, 25);
            this.btnAddGroup.TabIndex = 20;
            this.btnAddGroup.Text = "Add Group";
            this.btnAddGroup.UseVisualStyleBackColor = true;
            this.btnAddGroup.Click += new System.EventHandler(this.btnAddGroup_Click);
            // 
            // txtBoxPlaylistslug
            // 
            this.txtBoxPlaylistslug.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBoxPlaylistslug.Location = new System.Drawing.Point(6, 12);
            this.txtBoxPlaylistslug.Name = "txtBoxPlaylistslug";
            this.txtBoxPlaylistslug.Size = new System.Drawing.Size(157, 13);
            this.txtBoxPlaylistslug.TabIndex = 18;
            // 
            // btnPlaylist
            // 
            this.btnPlaylist.Location = new System.Drawing.Point(179, 6);
            this.btnPlaylist.Name = "btnPlaylist";
            this.btnPlaylist.Size = new System.Drawing.Size(88, 25);
            this.btnPlaylist.TabIndex = 19;
            this.btnPlaylist.Text = "Create Playlist";
            this.btnPlaylist.UseVisualStyleBackColor = true;
            this.btnPlaylist.Click += new System.EventHandler(this.btnPlaylist_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1104, 578);
            this.Controls.Add(this.pnlPlaylist);
            this.Controls.Add(this.splitterLetf);
            this.Controls.Add(this.pnlTemplate);
            this.Name = "Form1";
            this.Text = "Form1";
            this.pnlPlaylist.ResumeLayout(false);
            this.pnlMainPlaylist.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTemplate;
        private System.Windows.Forms.Splitter splitterLetf;
        private System.Windows.Forms.Panel pnlPlaylist;
        private System.Windows.Forms.Panel pnlMainPlaylist;
        private System.Windows.Forms.TreeView tvwPlaylist;
        private System.Windows.Forms.ListBox lstBoxInstances;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.TextBox txtGroup;
        private System.Windows.Forms.Button btnAddGroup;
        private System.Windows.Forms.TextBox txtBoxPlaylistslug;
        private System.Windows.Forms.Button btnPlaylist; 
    }
}

