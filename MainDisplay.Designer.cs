namespace AT_SCC
{

    partial class MainDisplay
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
            // DEFINE THE MAIN OVERALL FORM

            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 500);
            this.Text = "ATech Training Serial COM Communicatons";
            existingPorts = new List<string>();
            existingPorts.AddRange(AvailablePorts);

            Size = this.ClientSize;
            BackgroundImage = Image.FromFile("AT-SCC_GUI_Background.png");
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ControlBox = true; MaximizeBox = false; MinimizeBox = true;


            // DEFINE THE MENU BAR

            menuStrip = new MenuStrip();
            menuStrip.Parent = this;
            fileMenuItem = new ToolStripMenuItem("&File");
            comPortMenuItem = new ToolStripMenuItem("&Port Configurations");
            sendMenuItem = new ToolStripMenuItem("&Send/Receive Configurations");
            loggingMenuItem = new ToolStripMenuItem("&Log File");

            loggingMenuItem.DropDownItems.AddRange(new ToolStripItem[] { new ToolStripMenuItem("View Logs"), new ToolStripMenuItem("Delete Logs") });
            loggingMenuItem.DropDownItemClicked += OnLogChange;


            exitMenuItem = new ToolStripMenuItem("&Exit", null, (_, _) => OnClose()) { ShortcutKeys = Keys.Control | Keys.X };
            reloadMenuItem = new ToolStripMenuItem("&Reload", null, (_, _) => OnReload()) { ShortcutKeys = Keys.Control | Keys.L };
            var Help = new ToolStripMenuItem("&Info/Help", null, (_, _) => OnHelp()) { ShortcutKeys = Keys.Control | Keys.H };


            modeMenuItem = new ToolStripMenuItem("&Select Mode");
            modeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { new ToolStripMenuItem("Send Mode"), new ToolStripMenuItem("Receive Mode"), new ToolStripMenuItem("IDLE Mode") });
            modeMenuItem.DropDownItemClicked += OnModeChange;

            portengageMenuItem = new ToolStripMenuItem("&Open/Close Port");
            portengageMenuItem.DropDownItems.Add("Open Port");
            portengageMenuItem.DropDownItems.Add("Close Port");
            portengageMenuItem.DropDownItemClicked += OnEngage;


            fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { Help, modeMenuItem, loggingMenuItem, portengageMenuItem, reloadMenuItem, exitMenuItem });


            currentCom = new ToolStripMenuItem("&COM");
            foreach (var port in AvailablePorts)
            {
                currentCom.DropDownItems.Add(port);
            }
            currentPort = "COM#";
            currentCom.Text = currentPort;
            currentCom.DropDownItemClicked += OnCommSelected;


            baudMenuItem = new ToolStripMenuItem("&Baud");
            baudMenuItem.DropDownItems.AddRange(PossibleBauds.Select(b => new ToolStripMenuItem(b)).ToArray());
            baudMenuItem.DropDownItemClicked += OnBaudSelected;

            parityMenuItem = new ToolStripMenuItem("&Parity");
            parityMenuItem.DropDownItemClicked += OnParitySelection;
            parityMenuItem.DropDownItems.AddRange(ParityOptions.Keys.Select(s => new ToolStripMenuItem(s)).ToArray());

            dataBitsMenuItem = new ToolStripMenuItem("&Data Bits");
            for (int i = 5; i <= 9; i++)
            {
                dataBitsMenuItem.DropDownItems.Add(i.ToString());
            }
            dataBitsMenuItem.DropDownItemClicked += OndataBitsSelection;


            stopBitsMenuItem = new ToolStripMenuItem("&Stop Bits");
            stopBitsMenuItem.DropDownItemClicked += OnStopSelection;
            stopBitsMenuItem.DropDownItems.AddRange(StopBitOptions.Keys.Select(s => new ToolStripMenuItem(s)).ToArray());


            readTimeoutItem = new ToolStripMenuItem("&Read Timeout");
            readTimeoutItem.DropDownItems.Add("-1");
            readTimeoutItem.DropDownItems.Add("500");
            writeTimeoutItem = new ToolStripMenuItem("&Write Timeout");
            writeTimeoutItem.DropDownItems.Add("-1");
            writeTimeoutItem.DropDownItems.Add("500");
            readTimeoutItem.DropDownItemClicked += OnRTimeout;
            writeTimeoutItem.DropDownItemClicked += OnWTimeout;


            handshakeItem = new ToolStripMenuItem("&Handshake");
            handshakeItem.DropDownItems.AddRange(HandShakeOptions.Keys.Select(key => new ToolStripMenuItem(key)).ToArray());
            handshakeItem.DropDownItemClicked += OnHandshakeSelection;

            sendDelay = new ToolStripMenuItem("&Delay (ms)");
            sendDelay.DropDownItems.AddRange(sendDelayOptions.Select(sdelay => new ToolStripMenuItem(sdelay)).ToArray());
            sendDelay.DropDownItemClicked += OnDelaySelection;

            sendingModeMenuItem = new ToolStripMenuItem("&Sending Data Type");
            sendingModeMenuItem.DropDownItems.AddRange(PossibleTransmitModes.Select(psm => new ToolStripMenuItem(psm)).ToArray());
            sendingModeMenuItem.DropDownItemClicked += OnSModeChange;

            receivingModeMenuItem = new ToolStripMenuItem("&Receiving Data Type");
            receivingModeMenuItem.DropDownItems.AddRange(PossibleTransmitModes.Select(prm => new ToolStripMenuItem(prm)).ToArray());
            receivingModeMenuItem.DropDownItemClicked += OnRModeChange;

            comPortMenuItem.DropDownItems.AddRange(new ToolStripItem[] { currentCom, baudMenuItem, parityMenuItem, dataBitsMenuItem, stopBitsMenuItem, readTimeoutItem, writeTimeoutItem, handshakeItem, sendDelay});

            sendMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sendingModeMenuItem, receivingModeMenuItem });

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenuItem, comPortMenuItem, sendMenuItem });

            MainMenuStrip = menuStrip;

            // DEFINE CLOCK

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // Update time every 1 second
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

            this.clock = new System.Windows.Forms.TextBox();
            this.clock.Location = new Point(10, 365);
            this.clock.Width = 150;
            this.clock.TextAlign = HorizontalAlignment.Center;
            this.clock.ReadOnly = true;
            this.clock.Cursor = Cursors.Arrow;
            this.Controls.Add(this.clock);

            // DEFINE and DISPLAY transmit and receive data buttons

            string[] buttonLabels = { "TRANSMIT DATA", "RECEIVE DATA" };
            Point[] buttonLocations = { new Point(410, 280), new Point(570, 280) };
            Color[] buttonColors = { Color.LightGreen, Color.Yellow };
            EventHandler[] buttonHandlers = { new EventHandler(buttonSEND_Click), new EventHandler(buttonRECEIVE_Click) };

            for (int i = 0; i < buttonLabels.Length; i++)
            {
                Button button = new Button();
                button.Location = buttonLocations[i];
                button.Text = buttonLabels[i];
                button.Font = new Font("Arial", 12);
                button.Width = 155;
                button.Height = 40;
                button.AutoSize = true;
                button.BackColor = buttonColors[i];
                button.DialogResult = DialogResult.OK;
                button.Click += buttonHandlers[i];
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#00457c");
                button.FlatAppearance.BorderSize = 2;
                this.Controls.Add(button);
            }

            

            // DEFINE INPUT AND OUTPUT PANELS

            AddLabel("TX (Sending):", new Point(410, 45), new Font("Arial", 10));


            this.textBoxesPanel = new Panel();
            this.textBoxesPanel.Location = new Point(410, 65);
            this.textBoxesPanel.BackColor = Color.LightBlue;
            this.textBoxesPanel.Size = new Size(125, 190);
            this.Controls.Add(this.textBoxesPanel);

            // Set the minimum and maximum values for the scrollbar
            this.vScrollBar1 = new VScrollBar();
            this.vScrollBar1.Minimum = 0;
            this.vScrollBar1.Maximum = textBoxesPanel.Controls.Count - 2;
            this.vScrollBar1.Width = 30;
            this.vScrollBar1.Height = 190;
            this.vScrollBar1.Location = new Point(535, 65);
            this.vScrollBar1.LargeChange = 1;

            AddLabel("RX (Receiving):", new Point(598, 45), new Font("Arial", 10));

            this.textBoxesPanel2 = new Panel();
            this.textBoxesPanel2.Location = new Point(598, 65);
            this.textBoxesPanel2.BackColor = Color.LightBlue;
            this.textBoxesPanel2.Size = new Size(125, 190);
            this.Controls.Add(this.textBoxesPanel2);

            this.vScrollBar2 = new VScrollBar();
            this.vScrollBar2.Minimum = 0;
            this.vScrollBar2.Maximum = MAX_BUFFER_SIZE - 2;
            this.vScrollBar2.Width = 30;
            this.vScrollBar2.Height = 190;
            this.vScrollBar2.Location = new Point(568, 65);
            this.vScrollBar2.LargeChange = 1;

            // Add a scroll event handler to the scrollbar
            this.vScrollBar1.Scroll += VScrollBarSend_Scroll;
            this.Controls.Add(vScrollBar1);

            this.vScrollBar2.Scroll += VScrollBarReceive_Scroll;
            this.Controls.Add(vScrollBar2);

            

            

            // DEFINE LABELS, TEXTBOXES, BUTTONS FOR MAIN DISPLAY

            AddLabel("COM PORT:", new Point(20, 40), new Font("Arial", 8));

            this.textBoxCOM = new System.Windows.Forms.TextBox();
            this.textBoxCOM.Location = new Point(20, 60);
            this.textBoxCOM.Width = 75;
            this.textBoxCOM.TextAlign = HorizontalAlignment.Center;
            this.textBoxCOM.BackColor = Color.Pink;
            this.textBoxCOM.ReadOnly = true;
            this.textBoxCOM.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxCOM);

            AddLabel("BAUDRATE:", new Point(110, 40), new Font("Arial", 8));

            this.textBoxBAUD = new System.Windows.Forms.TextBox();
            this.textBoxBAUD.Location = new Point(110, 60);
            this.textBoxBAUD.Width = 75;
            this.textBoxBAUD.TextAlign = HorizontalAlignment.Center;
            this.textBoxBAUD.BackColor = Color.Pink;
            this.textBoxBAUD.ReadOnly = true;
            this.textBoxBAUD.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxBAUD);

            AddLabel("PARITY:", new Point(20, 90), new Font("Arial", 8));

            this.textBoxPARITY = new System.Windows.Forms.TextBox();
            this.textBoxPARITY.Location = new Point(20, 110);
            this.textBoxPARITY.Width = 75;
            this.textBoxPARITY.Text = "None";
            this.textBoxPARITY.TextAlign = HorizontalAlignment.Center;
            this.textBoxPARITY.BackColor = Color.LightYellow;
            this.textBoxPARITY.ReadOnly = true;
            this.textBoxPARITY.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxPARITY);

            AddLabel("DATABITS:", new Point(110, 90), new Font("Arial", 8));

            this.textBoxDATABITS = new System.Windows.Forms.TextBox();
            this.textBoxDATABITS.Location = new Point(110, 110);
            this.textBoxDATABITS.Width = 75;
            this.textBoxDATABITS.Text = "8";
            this.textBoxDATABITS.TextAlign = HorizontalAlignment.Center;
            this.textBoxDATABITS.BackColor = Color.LightYellow;
            this.textBoxDATABITS.ReadOnly = true;
            this.textBoxDATABITS.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxDATABITS);

            AddLabel("STOPBITS:", new Point(20, 140), new Font("Arial", 8));

            this.textBoxSTOPBITS = new System.Windows.Forms.TextBox();
            this.textBoxSTOPBITS.Location = new Point(20, 160);
            this.textBoxSTOPBITS.Width = 75;
            this.textBoxSTOPBITS.Text = "1";
            this.textBoxSTOPBITS.TextAlign = HorizontalAlignment.Center;
            this.textBoxSTOPBITS.BackColor = Color.LightYellow;
            this.textBoxSTOPBITS.ReadOnly = true;
            this.textBoxSTOPBITS.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxSTOPBITS);

            AddLabel("R T-OUT:", new Point(110, 140), new Font("Arial", 8));

            this.textBoxRTIMEOUT = new System.Windows.Forms.TextBox();
            this.textBoxRTIMEOUT.Location = new Point(110, 160);
            this.textBoxRTIMEOUT.Width = 75;
            this.textBoxRTIMEOUT.TextAlign = HorizontalAlignment.Center;
            this.textBoxRTIMEOUT.BackColor = Color.LightYellow;
            this.textBoxRTIMEOUT.ReadOnly = true;
            this.textBoxRTIMEOUT.Cursor = Cursors.Arrow;
            this.textBoxRTIMEOUT.Text = currentReadTimeout;
            this.Controls.Add(this.textBoxRTIMEOUT);

            AddLabel("W T-OUT:", new Point(20, 190), new Font("Arial", 8));

            this.textBoxWTIMEOUT = new System.Windows.Forms.TextBox();
            this.textBoxWTIMEOUT.Location = new Point(20, 210);
            this.textBoxWTIMEOUT.Width = 75;
            this.textBoxWTIMEOUT.TextAlign = HorizontalAlignment.Center;
            this.textBoxWTIMEOUT.BackColor = Color.LightYellow;
            this.textBoxWTIMEOUT.ReadOnly = true;
            this.textBoxWTIMEOUT.Cursor = Cursors.Arrow;
            this.textBoxWTIMEOUT.Text = currentWriteTimeout;
            this.Controls.Add(this.textBoxWTIMEOUT);

            AddLabel("HANDSHAKE:", new Point(110, 190), new Font("Arial", 8));

            this.textBoxHANDSHAKE = new System.Windows.Forms.TextBox();
            this.textBoxHANDSHAKE.Location = new Point(110, 210);
            this.textBoxHANDSHAKE.Width = 75;
            this.textBoxHANDSHAKE.Text = "None";
            this.textBoxHANDSHAKE.TextAlign = HorizontalAlignment.Center;
            this.textBoxHANDSHAKE.BackColor = Color.LightYellow;
            this.textBoxHANDSHAKE.ReadOnly = true;
            this.textBoxHANDSHAKE.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxHANDSHAKE);

            AddLabel("DELAY (ms):", new Point(20, 240), new Font("Arial", 8));

            this.textBoxDELAY = new System.Windows.Forms.TextBox();
            this.textBoxDELAY.Location = new Point(20, 260);
            this.textBoxDELAY.Width = 75;
            this.textBoxDELAY.Text = "1000";
            this.textBoxDELAY.TextAlign = HorizontalAlignment.Center;
            this.textBoxDELAY.BackColor = Color.LightYellow;
            this.textBoxDELAY.ReadOnly = true;
            this.textBoxDELAY.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxDELAY);

            AddLabel("STOP REPEAT:", new Point(250, 240), new Font("Arial", 8));

            Button stop = new Button();
            stop.Location = new Point(250,260);
            stop.Width = 125;
            stop.BackColor = Color.Pink;
            stop.Text = "STOP";
            stop.Click += StopButton_Click;
            this.Controls.Add(stop);

            AddLabel("MODE:", new Point(250, 40), new Font("Arial", 8));

            this.textBoxMODEDISP = new System.Windows.Forms.TextBox();
            this.textBoxMODEDISP.Location = new Point(250, 60);
            this.textBoxMODEDISP.Width = 125;
            this.textBoxMODEDISP.Text = "Idle Mode";
            this.textBoxMODEDISP.TextAlign = HorizontalAlignment.Center;
            this.textBoxMODEDISP.BackColor = Color.LightGray;
            this.textBoxMODEDISP.ReadOnly = true;
            this.textBoxMODEDISP.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxMODEDISP);


            AddLabel("SEND DATA TYPE:", new Point(250, 90), new Font("Arial", 8));

            this.textBoxSENDTYPE = new System.Windows.Forms.TextBox();
            this.textBoxSENDTYPE.Location = new Point(250, 110);
            this.textBoxSENDTYPE.Width = 125;
            this.textBoxSENDTYPE.Text = "N/A - DISABLED";
            this.textBoxSENDTYPE.TextAlign = HorizontalAlignment.Center;
            this.textBoxSENDTYPE.BackColor = Color.LightGray;
            this.textBoxSENDTYPE.ReadOnly = true;
            this.textBoxSENDTYPE.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxSENDTYPE);

            AddLabel("RECEIVE DATA TYPE:", new Point(250, 140), new Font("Arial", 8));

            this.textBoxreceiveType = new System.Windows.Forms.TextBox();
            this.textBoxreceiveType.Location = new Point(250, 160);
            this.textBoxreceiveType.Width = 125;
            this.textBoxreceiveType.Text = "N/A - DISABLED";
            this.textBoxreceiveType.TextAlign = HorizontalAlignment.Center;
            this.textBoxreceiveType.BackColor = Color.LightGray;
            this.textBoxreceiveType.ReadOnly = true;
            this.textBoxreceiveType.Cursor = Cursors.Arrow;
            this.Controls.Add(this.textBoxreceiveType);

            // Define the label and textbox controls
            AddLabel("# OF TX ENTRIES:", new Point(250, 190), new Font("Arial", 8));

            this.textBoxBTT = new System.Windows.Forms.TextBox();
            this.textBoxBTT.Location = new Point(250, 210);
            this.textBoxBTT.Text = "0";
            this.textBoxBTT.Width = 125;
            this.textBoxBTT.TextAlign = HorizontalAlignment.Center;
            this.textBoxBTT.BackColor = Color.LightBlue;
            this.textBoxBTT.TextChanged += TextBoxBTT_TextChanged; // Add event handler
            this.Controls.Add(this.textBoxBTT);

            AddLabel("MAX RX BUFFER:", new Point(110, 240), new Font("Arial", 8));

            this.textBoxBUFFER = new System.Windows.Forms.TextBox();
            this.textBoxBUFFER.Location = new Point(110, 260);
            this.textBoxBUFFER.Text = Convert.ToString(MAX_BUFFER_SIZE);
            this.textBoxBUFFER.Width = 75;
            this.textBoxBUFFER.TextAlign = HorizontalAlignment.Center;
            this.textBoxBUFFER.BackColor = Color.LightYellow;
            this.textBoxBUFFER.ReadOnly = true;
            this.Controls.Add(this.textBoxBUFFER);

            // DEFINE INDICATOR PANEL
            this.textBoxSTATUS = new TextBox();
            this.textBoxSTATUS.Location = new Point(250, 365);
            this.textBoxSTATUS.ReadOnly = true;
            this.textBoxSTATUS.Text = "READY";
            this.textBoxSTATUS.TextAlign = HorizontalAlignment.Center;
            this.textBoxSTATUS.BackColor = Color.LightGray;
            this.textBoxSTATUS.Width = 125;
            this.Controls.Add(this.textBoxSTATUS);

            // DEFINE CHECKBOXES

            receive_check.Location = new Point(110, 310);
            receive_check.Width = 175;
            receive_check.Height = 20;
            receive_check.Text = "Send and Receive Mode";
            this.Controls.Add(receive_check);

            repeat_check.Location = new Point(300, 310);
            repeat_check.Width = 75;
            repeat_check.Height = 20;
            repeat_check.Text = "Repeat";
            this.Controls.Add(repeat_check);


            logging_check.Location = new Point(20, 310);
            logging_check.Width = 50;
            logging_check.Height = 20;
            logging_check.Text = "Log";
            this.Controls.Add(logging_check);

        }
    }

    #endregion
}
