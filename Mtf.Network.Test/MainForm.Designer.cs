
namespace Mtf.Network.Test
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabPage1 = new TabPage();
            panel1 = new Panel();
            btnSendToClient = new Button();
            label7 = new Label();
            rtbSendToClient = new RichTextBox();
            btnStopServer = new Button();
            btnStartServer = new Button();
            label2 = new Label();
            rtbServerReceivedMessages = new RichTextBox();
            label1 = new Label();
            nudServerListeningPort = new NumericUpDown();
            tabPage2 = new TabPage();
            panel2 = new Panel();
            label6 = new Label();
            rtbClientSend = new RichTextBox();
            label5 = new Label();
            label4 = new Label();
            nudServerPort = new NumericUpDown();
            tbServerAddress = new TextBox();
            label3 = new Label();
            btnSend = new Button();
            rtbClientReceived = new RichTextBox();
            tabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudServerListeningPort).BeginInit();
            tabPage2.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudServerPort).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPage1);
            tabControl.Controls.Add(tabPage2);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(831, 408);
            tabControl.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(panel1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(823, 380);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Server";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Gray;
            panel1.Controls.Add(btnSendToClient);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(rtbSendToClient);
            panel1.Controls.Add(btnStopServer);
            panel1.Controls.Add(btnStartServer);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(rtbServerReceivedMessages);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(nudServerListeningPort);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(817, 374);
            panel1.TabIndex = 0;
            // 
            // btnSendToClient
            // 
            btnSendToClient.Location = new Point(737, 176);
            btnSendToClient.Name = "btnSendToClient";
            btnSendToClient.Size = new Size(75, 23);
            btnSendToClient.TabIndex = 8;
            btnSendToClient.Text = "Send";
            btnSendToClient.UseVisualStyleBackColor = true;
            btnSendToClient.Click += BtnSendToClient_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(14, 67);
            label7.Name = "label7";
            label7.Size = new Size(79, 15);
            label7.TabIndex = 7;
            label7.Text = "Send to client";
            // 
            // rtbSendToClient
            // 
            rtbSendToClient.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbSendToClient.BackColor = Color.Silver;
            rtbSendToClient.Location = new Point(14, 85);
            rtbSendToClient.Name = "rtbSendToClient";
            rtbSendToClient.Size = new Size(798, 85);
            rtbSendToClient.TabIndex = 6;
            rtbSendToClient.Text = "Hello from Server";
            // 
            // btnStopServer
            // 
            btnStopServer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStopServer.Location = new Point(737, 46);
            btnStopServer.Name = "btnStopServer";
            btnStopServer.Size = new Size(75, 23);
            btnStopServer.TabIndex = 5;
            btnStopServer.Text = "Stop";
            btnStopServer.UseVisualStyleBackColor = true;
            btnStopServer.Click += BtnStopServer_Click;
            // 
            // btnStartServer
            // 
            btnStartServer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStartServer.Location = new Point(737, 17);
            btnStartServer.Name = "btnStartServer";
            btnStartServer.Size = new Size(75, 23);
            btnStartServer.TabIndex = 4;
            btnStartServer.Text = "Start";
            btnStartServer.UseVisualStyleBackColor = true;
            btnStartServer.Click += BtnStartServer_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 201);
            label2.Name = "label2";
            label2.Size = new Size(108, 15);
            label2.TabIndex = 3;
            label2.Text = "Received messages";
            // 
            // rtbServerReceivedMessages
            // 
            rtbServerReceivedMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbServerReceivedMessages.BackColor = Color.Silver;
            rtbServerReceivedMessages.Location = new Point(14, 219);
            rtbServerReceivedMessages.Name = "rtbServerReceivedMessages";
            rtbServerReceivedMessages.Size = new Size(798, 150);
            rtbServerReceivedMessages.TabIndex = 2;
            rtbServerReceivedMessages.Text = "";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 17);
            label1.Name = "label1";
            label1.Size = new Size(29, 15);
            label1.TabIndex = 1;
            label1.Text = "Port";
            // 
            // nudServerListeningPort
            // 
            nudServerListeningPort.Location = new Point(14, 35);
            nudServerListeningPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            nudServerListeningPort.Name = "nudServerListeningPort";
            nudServerListeningPort.Size = new Size(77, 23);
            nudServerListeningPort.TabIndex = 0;
            nudServerListeningPort.Value = new decimal(new int[] { 4525, 0, 0, 0 });
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(panel2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(823, 380);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Client";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Gray;
            panel2.Controls.Add(label6);
            panel2.Controls.Add(rtbClientSend);
            panel2.Controls.Add(label5);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(nudServerPort);
            panel2.Controls.Add(tbServerAddress);
            panel2.Controls.Add(label3);
            panel2.Controls.Add(btnSend);
            panel2.Controls.Add(rtbClientReceived);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(3, 3);
            panel2.Name = "panel2";
            panel2.Size = new Size(817, 374);
            panel2.TabIndex = 0;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(14, 54);
            label6.Name = "label6";
            label6.Size = new Size(95, 15);
            label6.TabIndex = 8;
            label6.Text = "Message to send";
            // 
            // rtbClientSend
            // 
            rtbClientSend.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtbClientSend.BackColor = Color.Silver;
            rtbClientSend.Location = new Point(14, 72);
            rtbClientSend.Name = "rtbClientSend";
            rtbClientSend.Size = new Size(790, 102);
            rtbClientSend.TabIndex = 7;
            rtbClientSend.Text = "Hello from Client";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(14, 181);
            label5.Name = "label5";
            label5.Size = new Size(100, 15);
            label5.TabIndex = 6;
            label5.Text = "Message received";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(114, 11);
            label4.Name = "label4";
            label4.Size = new Size(29, 15);
            label4.TabIndex = 5;
            label4.Text = "Port";
            // 
            // nudServerPort
            // 
            nudServerPort.Location = new Point(114, 29);
            nudServerPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            nudServerPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudServerPort.Name = "nudServerPort";
            nudServerPort.Size = new Size(77, 23);
            nudServerPort.TabIndex = 4;
            nudServerPort.Value = new decimal(new int[] { 4525, 0, 0, 0 });
            // 
            // tbServerAddress
            // 
            tbServerAddress.Location = new Point(14, 29);
            tbServerAddress.Name = "tbServerAddress";
            tbServerAddress.Size = new Size(94, 23);
            tbServerAddress.TabIndex = 3;
            tbServerAddress.Text = "127.0.0.1";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(14, 11);
            label3.Name = "label3";
            label3.Size = new Size(39, 15);
            label3.TabIndex = 2;
            label3.Text = "Server";
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(729, 346);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 1;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += BtnSendToServer_Click;
            // 
            // rtbClientReceived
            // 
            rtbClientReceived.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbClientReceived.BackColor = Color.Silver;
            rtbClientReceived.Location = new Point(14, 199);
            rtbClientReceived.Name = "rtbClientReceived";
            rtbClientReceived.Size = new Size(790, 141);
            rtbClientReceived.TabIndex = 0;
            rtbClientReceived.Text = "";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(831, 408);
            Controls.Add(tabControl);
            Name = "MainForm";
            Text = "Mtf.Network Tst Application";
            tabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudServerListeningPort).EndInit();
            tabPage2.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudServerPort).EndInit();
            ResumeLayout(false);
        }


        #endregion

        private TabControl tabControl;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Panel panel1;
        private RichTextBox rtbServerReceivedMessages;
        private Label label1;
        private NumericUpDown nudServerListeningPort;
        private Panel panel2;
        private Button btnStopServer;
        private Button btnStartServer;
        private Label label2;
        private TextBox tbServerAddress;
        private Label label3;
        private Button btnSend;
        private RichTextBox rtbClientReceived;
        private Label label4;
        private NumericUpDown nudServerPort;
        private Label label6;
        private RichTextBox rtbClientSend;
        private Label label5;
        private Button btnSendToClient;
        private Label label7;
        private RichTextBox rtbSendToClient;
    }
}
