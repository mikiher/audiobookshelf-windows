namespace AudiobookshelfTray
{
    partial class SettingsDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.labelPort = new System.Windows.Forms.Label();
            this.labelDataFolder = new System.Windows.Forms.Label();
            this.groupBoxServer = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelServer = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutDataFolder = new System.Windows.Forms.TableLayoutPanel();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxDataFolder = new System.Windows.Forms.TextBox();
            this.tableLayoutPanelPort = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.errorProviderPort = new System.Windows.Forms.ErrorProvider(this.components);
            this.errorProviderDataFolder = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBoxServer.SuspendLayout();
            this.tableLayoutPanelServer.SuspendLayout();
            this.tableLayoutDataFolder.SuspendLayout();
            this.tableLayoutPanelPort.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderDataFolder)).BeginInit();
            this.SuspendLayout();
            // 
            // labelPort
            // 
            this.labelPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(3, 9);
            this.labelPort.Margin = new System.Windows.Forms.Padding(3);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(42, 20);
            this.labelPort.TabIndex = 0;
            this.labelPort.Text = "Port:";
            // 
            // labelDataFolder
            // 
            this.labelDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDataFolder.AutoSize = true;
            this.labelDataFolder.Location = new System.Drawing.Point(3, 10);
            this.labelDataFolder.Margin = new System.Windows.Forms.Padding(3);
            this.labelDataFolder.Name = "labelDataFolder";
            this.labelDataFolder.Size = new System.Drawing.Size(97, 20);
            this.labelDataFolder.TabIndex = 0;
            this.labelDataFolder.Text = "Data Folder:";
            // 
            // groupBoxServer
            // 
            this.groupBoxServer.Controls.Add(this.tableLayoutPanelServer);
            this.groupBoxServer.Location = new System.Drawing.Point(13, 13);
            this.groupBoxServer.Name = "groupBoxServer";
            this.groupBoxServer.Size = new System.Drawing.Size(562, 182);
            this.groupBoxServer.TabIndex = 0;
            this.groupBoxServer.TabStop = false;
            this.groupBoxServer.Text = "Server";
            // 
            // tableLayoutPanelServer
            // 
            this.tableLayoutPanelServer.ColumnCount = 1;
            this.tableLayoutPanelServer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelServer.Controls.Add(this.tableLayoutDataFolder, 0, 1);
            this.tableLayoutPanelServer.Controls.Add(this.tableLayoutPanelPort, 0, 0);
            this.tableLayoutPanelServer.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelServer.Location = new System.Drawing.Point(3, 22);
            this.tableLayoutPanelServer.Name = "tableLayoutPanelServer";
            this.tableLayoutPanelServer.Padding = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanelServer.RowCount = 2;
            this.tableLayoutPanelServer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelServer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelServer.Size = new System.Drawing.Size(556, 158);
            this.tableLayoutPanelServer.TabIndex = 0;
            // 
            // tableLayoutDataFolder
            // 
            this.tableLayoutDataFolder.ColumnCount = 2;
            this.tableLayoutDataFolder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutDataFolder.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutDataFolder.Controls.Add(this.buttonBrowse, 1, 1);
            this.tableLayoutDataFolder.Controls.Add(this.labelDataFolder, 0, 0);
            this.tableLayoutDataFolder.Controls.Add(this.textBoxDataFolder, 0, 1);
            this.tableLayoutDataFolder.Location = new System.Drawing.Point(9, 82);
            this.tableLayoutDataFolder.Name = "tableLayoutDataFolder";
            this.tableLayoutDataFolder.RowCount = 2;
            this.tableLayoutDataFolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutDataFolder.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutDataFolder.Size = new System.Drawing.Size(538, 67);
            this.tableLayoutDataFolder.TabIndex = 1;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.AutoSize = true;
            this.buttonBrowse.Location = new System.Drawing.Point(436, 36);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(99, 28);
            this.buttonBrowse.TabIndex = 4;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.BrowseClicked);
            // 
            // textBoxDataFolder
            // 
            this.textBoxDataFolder.Location = new System.Drawing.Point(3, 36);
            this.textBoxDataFolder.Name = "textBoxDataFolder";
            this.textBoxDataFolder.Size = new System.Drawing.Size(427, 26);
            this.textBoxDataFolder.TabIndex = 1;
            this.textBoxDataFolder.TextChanged += new System.EventHandler(this.DataFolderTextChanged);
            this.textBoxDataFolder.Validating += new System.ComponentModel.CancelEventHandler(this.DataFolderValidating);
            // 
            // tableLayoutPanelPort
            // 
            this.tableLayoutPanelPort.ColumnCount = 1;
            this.tableLayoutPanelPort.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelPort.Controls.Add(this.labelPort, 0, 0);
            this.tableLayoutPanelPort.Controls.Add(this.textBoxPort, 0, 1);
            this.tableLayoutPanelPort.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanelPort.Name = "tableLayoutPanelPort";
            this.tableLayoutPanelPort.RowCount = 2;
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelPort.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelPort.Size = new System.Drawing.Size(538, 64);
            this.tableLayoutPanelPort.TabIndex = 0;
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(3, 35);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(73, 26);
            this.textBoxPort.TabIndex = 1;
            this.textBoxPort.TextChanged += new System.EventHandler(this.PortTextChanged);
            this.textBoxPort.Validating += new System.ComponentModel.CancelEventHandler(this.PortValidating);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.AutoSize = true;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(500, 235);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 30);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.CancelClicked);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonSave.AutoSize = true;
            this.buttonSave.Location = new System.Drawing.Point(419, 235);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 30);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.SaveClicked);
            // 
            // errorProviderPort
            // 
            this.errorProviderPort.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProviderPort.ContainerControl = this;
            // 
            // errorProviderDataFolder
            // 
            this.errorProviderDataFolder.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProviderDataFolder.ContainerControl = this;
            // 
            // SettingsDialog
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(587, 277);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBoxServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(609, 333);
            this.Name = "SettingsDialog";
            this.Text = "Settings";
            this.groupBoxServer.ResumeLayout(false);
            this.tableLayoutPanelServer.ResumeLayout(false);
            this.tableLayoutDataFolder.ResumeLayout(false);
            this.tableLayoutDataFolder.PerformLayout();
            this.tableLayoutPanelPort.ResumeLayout(false);
            this.tableLayoutPanelPort.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderDataFolder)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxServer;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelServer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelPort;
        private System.Windows.Forms.TableLayoutPanel tableLayoutDataFolder;
        private System.Windows.Forms.TextBox textBoxDataFolder;
        private System.Windows.Forms.ErrorProvider errorProviderPort;
        private System.Windows.Forms.ErrorProvider errorProviderDataFolder;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Label labelDataFolder;
    }
}