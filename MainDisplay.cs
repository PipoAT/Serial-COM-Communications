using System.IO.Ports;
using System.Text;
using System.Globalization;


namespace AT_SCC
{

    public partial class MainDisplay : Form
    {                                           // declaring all variables associated with textboxes, menu/tool strip, strings, or int
        private TextBox? textBoxCOM, textBoxBAUD, textBoxPARITY, textBoxDATABITS, textBoxSTOPBITS, textBoxRTIMEOUT, textBoxWTIMEOUT, textBoxHANDSHAKE, textBoxDELAY, textBoxBTT, textBoxMODEDISP, textBoxSENDTYPE, textBoxreceiveType;

        private TextBox? clock, textBoxBUFFER, textBoxSTATUS;

        private Panel? textBoxesPanel, textBoxesPanel2;

        private VScrollBar? vScrollBar1, vScrollBar2;

        private MenuStrip? menuStrip;

        private ToolStripMenuItem? fileMenuItem, comPortMenuItem, sendMenuItem, loggingMenuItem, exitMenuItem, reloadMenuItem, modeMenuItem, currentCom, baudMenuItem, parityMenuItem, dataBitsMenuItem, stopBitsMenuItem, readTimeoutItem, writeTimeoutItem, handshakeItem, sendDelay, receivingModeMenuItem, sendingModeMenuItem, portengageMenuItem;

        private CancellationTokenSource? cancellationTokenSource;

        List<string>? existingPorts;

        public static string[] AvailablePorts => System.IO.Ports.SerialPort.GetPortNames();

        string currentPort = "", currentBaud = "", currentParity = "None", stopBitOption = "", currentReadTimeout = "-1", currentWriteTimeout = "-1", currentHandshakeOption = "None", dataBitoption = "", currentDelayOption = "1000", currentMode = "", currentLogMode = "", currentRTOption = "", currentSTOption = "";

        int currentBaudint, currentdataBitsint = 8, currentDelayint = 1000, currentRepeatint = 0, checklimit = 0;


        Parity currentParityvalue = Parity.None;                      // declaring variables to allow usage of dictionaries
        StopBits currentStopBitvalue = StopBits.One;

        Handshake currentHandshakevalue = Handshake.None;


        CheckBox logging_check = new CheckBox(), repeat_check = new CheckBox(), receive_check = new CheckBox();

        private string LogFilePath = Path.GetFullPath("SerialLog.txt"); // PATH TO TEXT FILE
        public int MAX_BUFFER_SIZE = 100;       // MAX BUFFER SIZE


        // dictionaries/list of strings for user selection and input
        public readonly string[] PossibleBauds = new string[]
        {
            "75","110","134","150","300","600","1200","1800","2400","4800","7200","9600","14400","19200","38400","57600","115200","128000"
        };

        public readonly string[] PossibleTransmitModes = new string[]
        {
            "Byte", "String", "Byte Collection", "ASCII (SEND ONLY)", "ASCII-HEX (SEND ONLY)"
        };

        public readonly Dictionary<string, Parity> ParityOptions = new Dictionary<string, Parity>()
        {
            ["None"] = Parity.None,
            ["Even"] = Parity.Even,
            ["Odd"] = Parity.Odd,
            ["Mark"] = Parity.Mark,
            ["Space"] = Parity.Space
        };

        public readonly Dictionary<string, StopBits> StopBitOptions = new Dictionary<string, StopBits>()
        {
            ["1"] = StopBits.One,
            ["1.5"] = StopBits.OnePointFive,
            ["2"] = StopBits.Two
        };

        public readonly Dictionary<string, Handshake> HandShakeOptions = new Dictionary<string, Handshake>()
        {
            ["None"] = Handshake.None,
            ["XOnXOff"] = Handshake.XOnXOff,
            ["Send (Rq)"] = Handshake.RequestToSend,
            ["XOnXOff (Rq)"] = Handshake.RequestToSendXOnXOff
        };


        public readonly string[] sendDelayOptions = new string[]
        {
            "100","500","1000","2000","3000","4000","5000"
        };


        // DECLARING EVENTS


        void OnCommSelected(object? sender, ToolStripItemClickedEventArgs e)        // event to select COM PORT based on user input 
        {
            if (e.ClickedItem != null && textBoxCOM != null)
            {
                textBoxCOM.Text = currentPort = e.ClickedItem.Text;
                textBoxCOM.BackColor = Color.LightGreen;
            }
        }

        void OnBaudSelected(object? sender, ToolStripItemClickedEventArgs e)        // event to select BAUD rate based on user input
        {

            if (e.ClickedItem != null && textBoxBAUD != null)
            {
                textBoxBAUD.Text = currentBaud = e.ClickedItem.Text;
                currentBaudint = int.Parse(currentBaud);
                textBoxBAUD.BackColor = Color.LightGreen;
            }

        }

        void OnModeChange(object? sender, ToolStripItemClickedEventArgs e)      // event to select mode based on user input
        {
            if (e.ClickedItem != null && textBoxMODEDISP != null) textBoxMODEDISP.Text = currentMode = e.ClickedItem.Text;
        }

        void OnLogChange(object? sender, ToolStripItemClickedEventArgs e)       // event to view or delete logs
        {     // event to activate or deactivate the logging feature

            if (e.ClickedItem != null) currentLogMode = e.ClickedItem.Text;
            if (currentLogMode == "View Logs") new Log(this).Show();

            else if (currentLogMode == "Delete Logs")
            {
                System.IO.File.WriteAllText(LogFilePath, string.Empty);
                MessageBox.Show("Logs have been erased", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        void OnEngage(object? sender, ToolStripItemClickedEventArgs e)          // event to open or close the port
        {
            try {
            if (e.ClickedItem != null && textBoxSTATUS != null)
            {
                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                {
                    Handshake = currentHandshakevalue,
                    ReadTimeout = int.Parse(currentReadTimeout),
                    WriteTimeout = int.Parse(currentWriteTimeout)

                };

                if (e.ClickedItem.Text == "Open Port")
                {

                    textBoxSTATUS.Text = "PORT OPEN";
                    mySerialPort.Open();

                }
                else if (e.ClickedItem.Text == "Close Port")
                {

                    textBoxSTATUS.Text = "PORT CLOSED";
                    mySerialPort.Close();
                }
            }
            }
            catch (Exception ex) {

                MessageBox.Show(Convert.ToString(ex),"WARNING",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            
            }


        }

        void OnParitySelection(object? sender, ToolStripItemClickedEventArgs e)     // event to select parity based on user input
        {
            if (e.ClickedItem != null && textBoxPARITY != null)
            {
                textBoxPARITY.Text = currentParity = e.ClickedItem.Text;
                currentParityvalue = ParityOptions[currentParity];
                textBoxPARITY.BackColor = Color.LightGreen;
            }

        }

        void OnStopSelection(object? sender, ToolStripItemClickedEventArgs e)   // event to select stopbit based on user input
        {
            if (e.ClickedItem != null && textBoxSTOPBITS != null)
            {
                textBoxSTOPBITS.Text = stopBitOption = e.ClickedItem.Text;
                currentStopBitvalue = StopBitOptions[stopBitOption];
                textBoxSTOPBITS.BackColor = Color.LightGreen;
            }
        }

        void OndataBitsSelection(object? sender, ToolStripItemClickedEventArgs e)   // event to select databit based on user input
        {         // event to select databits based on user input

            if (e.ClickedItem != null && textBoxDATABITS != null)
            {
                textBoxDATABITS.Text = dataBitoption = e.ClickedItem.Text;
                currentdataBitsint = int.Parse(dataBitoption);
                textBoxDATABITS.BackColor = Color.LightGreen;
            }
        }

        void OnHandshakeSelection(object? sender, ToolStripItemClickedEventArgs e)      // event to select handshake based on user input
        {
            if (e.ClickedItem != null && textBoxHANDSHAKE != null)
            {
                textBoxHANDSHAKE.Text = currentHandshakeOption = e.ClickedItem.Text;
                currentHandshakevalue = HandShakeOptions[currentHandshakeOption];
                textBoxHANDSHAKE.BackColor = Color.LightGreen;
            }
        }

        void OnDelaySelection(object? sender, ToolStripItemClickedEventArgs e)      // evet to select the delay timings based on user input
        {
            if (e.ClickedItem != null && textBoxDELAY != null)
            {
                textBoxDELAY.Text = currentDelayOption = e.ClickedItem.Text;
                currentDelayint = int.Parse(currentDelayOption);
                textBoxDELAY.BackColor = Color.LightGreen;
            }
        }

        void OnRModeChange(object? sender, ToolStripItemClickedEventArgs e)     // event to select the mode of the tool
        {
            if ((e.ClickedItem == null || currentMode != "Receive Mode") && textBoxreceiveType != null)
            {
                MessageBox.Show("Receive Mode Not Active", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBoxreceiveType.Text = "N/A - DISABLED";
                return;
            }

            else if (e.ClickedItem != null && textBoxreceiveType != null && textBoxSENDTYPE != null && currentMode == "Receive Mode")
            {
                textBoxreceiveType.Text = currentRTOption = e.ClickedItem.Text;
                textBoxSENDTYPE.Text = "N/A - DISABLED";
            }
        }

        void OnSModeChange(object? sender, ToolStripItemClickedEventArgs e)         // event to select the mode of the tool
        {
            if (currentMode == "Send Mode" && textBoxreceiveType != null && textBoxSENDTYPE != null)
            {
                textBoxSENDTYPE.Text = currentSTOption = e.ClickedItem?.Text ?? "N/A - DISABLED";
                textBoxreceiveType.Text = "N/A - DISABLED";
            }
            else if (currentMode != "Send Mode" && textBoxSENDTYPE != null)
            {
                textBoxSENDTYPE.Text = "N/A - DISABLED";
                MessageBox.Show("Send Mode Not Active", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OnRTimeout(object? sender, ToolStripItemClickedEventArgs e)            // event to select the read timeout
        {

            if (e.ClickedItem != null && textBoxRTIMEOUT != null)
            {

                textBoxRTIMEOUT.Text = currentReadTimeout = e.ClickedItem.Text;
                textBoxRTIMEOUT.BackColor = Color.LightGreen;
            }
        }

        void OnWTimeout(object? sender, ToolStripItemClickedEventArgs e)        // event to sleect the write timeout
        {

            if (e.ClickedItem != null && textBoxWTIMEOUT != null)
            {

                textBoxWTIMEOUT.Text = currentReadTimeout = e.ClickedItem.Text;
                textBoxWTIMEOUT.BackColor = Color.LightGreen;

            }

        }

        private void TextBoxBTT_TextChanged(object? sender, EventArgs e)             // Define the event handler for the textbox text changed event
        {
            try
            {
                if (textBoxBTT != null)
                {
                    if (int.Parse(textBoxBTT.Text) > 100)       //FIX THIS
                    {

                        MessageBox.Show("Desired transfer size exceeds buffer.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxesPanel?.Controls.Clear();
                        textBoxesPanel2?.Controls.Clear();

                    }

                    else if (int.Parse(textBoxBTT.Text) < 1)
                    {

                        MessageBox.Show("Desired transfer size needs to be positve value.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        textBoxesPanel?.Controls.Clear();
                        textBoxesPanel2?.Controls.Clear();
                    }

                    else
                    {

                        int btt = 1;
                        int.TryParse(textBoxBTT.Text, out btt);
                        this.textBoxesPanel?.Controls.Clear();
                        Enumerable.Range(1, btt).ToList().ForEach(i =>
                        {
                            TextBox textBox = new TextBox();
                            textBox.Location = new Point(10, 10 + (i - 1) * 20);
                            this.textBoxesPanel?.Controls.Add(textBox);
                        });
                        if (textBoxesPanel != null && vScrollBar1 != null)
                        {
                            this.vScrollBar1.Maximum = this.textBoxesPanel.Controls.Count - 2;
                        }
                    }
                }
            }
            catch (SystemException ex)
            {

                MessageBox.Show(ex.Message, "WARNING:", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }

        private void Timer_Tick(object? sender, EventArgs e)        // event to display the time and date
        {
            if (clock != null)
            {
                clock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Display current time in textbox
            }
        }

        private void VScrollBarSend_Scroll(object? sender, ScrollEventArgs e)       // events to handle the scroll bars
        {
            // Get the index of the first visible textbox
            int index = e.NewValue;


            // Determine which textboxes should be shown or hidden based on the scrollbar value
            for (int i = 0; i < textBoxesPanel?.Controls.Count; i++)
            {
                if (i < index || i > index + 11) textBoxesPanel?.Controls[i].Hide();
                else
                {
                    textBoxesPanel.Controls[i].Show();
                    textBoxesPanel.Controls[i].Top = (i - index) * textBoxesPanel.Controls[i].Height;

                }
            }


        }

        private void VScrollBarReceive_Scroll(object? sender, ScrollEventArgs e)            // events to handle the scroll bars
        {

            int index2 = e.NewValue;

            for (int j = 0; j < textBoxesPanel2?.Controls.Count; j++)
            {
                if (j < index2 || j > index2 + 11) textBoxesPanel2.Controls[j].Hide();
                else
                {
                    textBoxesPanel2.Controls[j].Show();
                    textBoxesPanel2.Controls[j].Top = (j - index2) * textBoxesPanel2.Controls[j].Height;
                }
            }



        }

        private void StopButton_Click(object sender, EventArgs e)           // event for button to stop the repeat
        {
            // Cancel the loop
            cancellationTokenSource?.Cancel();
        }


        private async void buttonRECEIVE_Click(object? sender, EventArgs e)     // event to engage the receive functionality
        {
            cancellationTokenSource = new CancellationTokenSource();

            try
            {
                if (textBoxesPanel2 != null)
                {
                    textBoxesPanel2.Controls.Clear();
                    if (currentMode == "Receive Mode" && currentRTOption == "Byte")
                    {
                        int i = 0;
                        var receivedBytes = new List<byte>();

                        using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                        {
                            Handshake = currentHandshakevalue,
                            ReadTimeout = int.Parse(currentReadTimeout),
                            WriteTimeout = int.Parse(currentWriteTimeout)

                        };

                        if (!mySerialPort.IsOpen) mySerialPort.Open();
                        if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                        var textBoxArray = new TextBox[MAX_BUFFER_SIZE];
                        for (var k = 0; k < textBoxArray.Length; k++)
                        {
                            textBoxArray[k] = new TextBox
                            {
                                Location = new Point(10, k * 20),
                                Width = 100,
                                ReadOnly = true
                            };
                            textBoxesPanel2.Controls.Add(textBoxArray[k]);
                        }

                        if (!(repeat_check.Checked))
                        {

                            var receivedByte = (byte)mySerialPort.ReadByte();
                            receivedBytes.Add(receivedByte);

                            if (logging_check.Checked)
                            {
                                var logFilePath = LogFilePath;

                                using var logFile = new StreamWriter(logFilePath, true);
                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE]: {receivedByte}");
                            }

                            if (textBoxArray.Length > 0)
                            {

                                var receivedTextBox = textBoxArray[i];
                                receivedTextBox.Text += $"{receivedByte:X2} ";

                                if (receivedTextBox.TextLength >= receivedTextBox.Width * 3)
                                {
                                    receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);
                                }
                                i++;
                            }

                            if (receivedBytes.Count >= MAX_BUFFER_SIZE)
                            {
                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            }



                        }

                        while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                        {


                            var receivedByte = (byte)mySerialPort.ReadByte();
                            receivedBytes.Add(receivedByte);

                            if (logging_check.Checked)
                            {
                                var logFilePath = LogFilePath;

                                using var logFile = new StreamWriter(logFilePath, true);
                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE]: {receivedByte}");
                            }

                            if (textBoxArray.Length > 0)
                            {
                                var receivedTextBox = textBoxArray[i];
                                receivedTextBox.Text += $"{receivedByte:X2} ";

                                if (receivedTextBox.TextLength >= receivedTextBox.Width * 3)
                                {
                                    receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);
                                }
                            }

                            if (receivedBytes.Count >= MAX_BUFFER_SIZE)
                            {
                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                i = -1;
                                receivedBytes = new List<byte>();

                            }
                            i++;

                            await Task.Delay(500);
                        }

                        mySerialPort.Close();
                        if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";


                    }

                    else if (currentMode == "Receive Mode" && currentRTOption == "String")
                    {

                        int i = 0;
                        var receivedString = new StringBuilder();

                        using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                        {
                            Handshake = currentHandshakevalue,
                            ReadTimeout = int.Parse(currentReadTimeout),
                            WriteTimeout = int.Parse(currentWriteTimeout)
                        };

                        if (!mySerialPort.IsOpen) mySerialPort.Open();
                        if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                        var textBoxArray = new TextBox[MAX_BUFFER_SIZE];
                        for (var k = 0; k < textBoxArray.Length; k++)
                        {
                            textBoxArray[k] = new TextBox
                            {
                                Location = new Point(10, k * 20),
                                Width = 100,
                                ReadOnly = true
                            };
                            textBoxesPanel2.Controls.Add(textBoxArray[k]);
                        }

                        if (!(repeat_check.Checked))
                        {

                            if (textBoxArray.Length > 0)
                            {

                                var receivedTextBox = textBoxArray[i];
                                receivedTextBox.Text = mySerialPort.ReadLine();

                                if (logging_check.Checked)
                                {
                                    var logFilePath = LogFilePath;
                                    using var logFile = new StreamWriter(logFilePath, true);
                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED STRING]: {receivedTextBox.Text}\n");
                                }

                                if (receivedTextBox.TextLength >= receivedTextBox.Width * 3)
                                {
                                    receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);
                                }
                                i++;
                            }

                            if (receivedString.Length >= MAX_BUFFER_SIZE)
                            {
                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }



                        }

                        while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                        {

                            if (textBoxArray.Length > 0)
                            {

                                var receivedTextBox = textBoxArray[i];
                                receivedTextBox.Text = mySerialPort.ReadLine();

                                if (logging_check.Checked)
                                {
                                    var logFilePath = LogFilePath;
                                    using var logFile = new StreamWriter(logFilePath, true);
                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED STRING]: {receivedTextBox.Text}\n");
                                }

                                if (receivedTextBox.TextLength >= receivedTextBox.Width * 3)
                                {
                                    receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);
                                }

                            }

                            if (i >= MAX_BUFFER_SIZE - 1)
                            {
                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                i = -1;
                            }
                            i++;

                            await Task.Delay(500);
                        }

                        mySerialPort.Close();
                        if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

                    }

                    else if (currentMode == "Receive Mode" && currentRTOption == "Byte Collection")
                    {

                        int i = 0;
                        var receivedBytes = new List<byte>();

                        using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                        {
                            Handshake = currentHandshakevalue,
                            ReadTimeout = int.Parse(currentReadTimeout),
                            WriteTimeout = int.Parse(currentWriteTimeout)
                        };

                        if (!mySerialPort.IsOpen) mySerialPort.Open();
                        if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                        var byteCollections = new List<List<byte>>();
                        var textBoxArray = new TextBox[MAX_BUFFER_SIZE];
                        for (var k = 0; k < textBoxArray.Length; k++)
                        {
                            textBoxArray[k] = new TextBox
                            {
                                Location = new Point(10, k * 20),
                                Width = 100,
                                ReadOnly = true
                            };
                            textBoxesPanel2.Controls.Add(textBoxArray[k]);
                        }

                        if (!(repeat_check.Checked))
                        {
                            if (textBoxArray.Length > 0)
                            {
                                var receivedTextBox = textBoxArray[i];
                                receivedTextBox.Text = Convert.ToString(mySerialPort.ReadByte());

                                if (receivedTextBox.TextLength >= receivedTextBox.Width * 3)
                                {
                                    receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);
                                }

                                if (logging_check.Checked)
                                {
                                    var logFilePath = LogFilePath;

                                    using var logFile = new StreamWriter(logFilePath, true);
                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE COLLECTION]: {receivedTextBox.Text}");
                                }
                                i++;
                            }

                            if (receivedBytes.Count >= MAX_BUFFER_SIZE)
                            {
                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            }
                        }

                        while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                        {

                            if (textBoxArray.Length > 0)
                            {
                                var receivedTextBox = textBoxArray[i];
                                receivedTextBox.Text = Convert.ToString(mySerialPort.ReadByte());

                                if (receivedTextBox.TextLength >= receivedTextBox.Width * 3)
                                {
                                    receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);
                                }

                                if (logging_check.Checked)
                                {
                                    var logFilePath = LogFilePath;

                                    using var logFile = new StreamWriter(logFilePath, true);
                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE COLLECTION]: {receivedTextBox.Text}");
                                }

                            }

                            if (i >= MAX_BUFFER_SIZE - 1)
                            {
                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                i = -1;

                            }
                            i++;

                            await Task.Delay(500);
                        }

                        mySerialPort.Close();
                        if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";
                    }

                    else
                    {
                        MessageBox.Show("Either Receive Mode not active OR Invalid Receive Data Type", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private async void buttonSEND_Click(object? sender, System.EventArgs e) // event to engage the send functionality
        {

            cancellationTokenSource = new CancellationTokenSource();
            if (textBoxBTT != null)
            {
                int buffcheck = currentRepeatint * int.Parse(textBoxBTT.Text);
                if (buffcheck > MAX_BUFFER_SIZE)
                {
                    MessageBox.Show("RX will receive more than allowed buffer", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    try
                    {
                        if (textBoxesPanel2 != null && textBoxesPanel != null)
                        {
                            textBoxesPanel2.Controls.Clear();
                            if (currentMode == "Send Mode" && currentSTOption == "String")
                            {

                                var inputValues = new List<string>();
                                foreach (var textBox in textBoxesPanel.Controls.OfType<TextBox>())
                                {
                                    if (!string.IsNullOrEmpty(textBox.Text))
                                    {
                                        inputValues.Add(textBox.Text);
                                    }
                                }

                                if (!inputValues.Any())
                                {
                                    MessageBox.Show("Please input strings to send\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                var delay = TimeSpan.FromMilliseconds(currentDelayint);

                                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                                {
                                    Handshake = currentHandshakevalue,
                                    ReadTimeout = int.Parse(currentReadTimeout),
                                    WriteTimeout = int.Parse(currentWriteTimeout)
                                };

                                if (!mySerialPort.IsOpen) mySerialPort.Open();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                                var textBoxArray = new TextBox[MAX_BUFFER_SIZE];
                                var textBoxesPanelIndex = 0; // keeps track of the index in the textboxesPanel2.Controls list

                                if (!repeat_check.Checked)
                                {

                                    for (var j = 0; j < Math.Min(inputValues.Count, MAX_BUFFER_SIZE); j++)
                                    {
                                        var textBox = new TextBox
                                        {
                                            Location = new Point(10, (textBoxesPanelIndex + j) * 20),
                                            Width = 100,
                                            ReadOnly = true
                                        };
                                        textBoxesPanel2.Controls.Add(textBox);
                                        textBoxArray[j] = textBox;
                                    }

                                    int currentTextBoxIndex = 0; // add this line to declare a variable to keep track of the current textbox index

                                    foreach (var value in inputValues)
                                    {
                                        Console.WriteLine("Sending string: {0}", value);

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT STRING]: {value}");
                                        }

                                        mySerialPort.Write(value);
                                        Thread.Sleep(currentDelayint);

                                        if (receive_check.Checked)
                                        {
                                            if (textBoxArray.Length > 0)
                                            {
                                                var receivedTextBox = textBoxArray[currentTextBoxIndex];
                                                var buffer = new byte[mySerialPort.ReadBufferSize];

                                                int bytesRead = 0;
                                                var sb = new StringBuilder();

                                                while (mySerialPort.BytesToRead > 0)
                                                {
                                                    bytesRead = mySerialPort.Read(buffer, 0, buffer.Length);
                                                    sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                                                }

                                                receivedTextBox.Text = sb.ToString();

                                                if (logging_check.Checked)
                                                {
                                                    var logFilePath = LogFilePath;
                                                    using var logFile = new StreamWriter(logFilePath, true);
                                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED STRING]: {receivedTextBox.Text}\n");
                                                }

                                                currentTextBoxIndex++; // increment the index of the current textbox
                                            }
                                            else
                                            {
                                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            }
                                        }

                                    }

                                    textBoxesPanelIndex += Math.Min(inputValues.Count, MAX_BUFFER_SIZE); // increment the index for the next iteration

                                }


                                while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                                {
                                    // create textboxes for the current iteration and add them to the panel
                                    if (textBoxesPanel2.Controls.Count <= MAX_BUFFER_SIZE)
                                    {
                                        for (var j = 0; j < Math.Min(inputValues.Count, MAX_BUFFER_SIZE); j++)
                                        {
                                            var textBox = new TextBox
                                            {
                                                Location = new Point(10, (textBoxesPanelIndex + j) * 20),
                                                Width = 100,
                                                ReadOnly = true
                                            };
                                            textBoxesPanel2.Controls.Add(textBox);
                                            textBoxArray[j] = textBox;
                                        }
                                    }

                                    int currentTextBoxIndex = 0; // add this line to declare a variable to keep track of the current textbox index

                                    foreach (var value in inputValues)
                                    {
                                        Console.WriteLine("Sending string: {0}", value);

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT STRING]: {value}");
                                        }

                                        mySerialPort.Write(value);
                                        Thread.Sleep(currentDelayint);

                                        if (receive_check.Checked)
                                        {
                                            if (textBoxArray.Length > 0 && checklimit < MAX_BUFFER_SIZE)
                                            {
                                                var receivedTextBox = textBoxArray[currentTextBoxIndex];
                                                var buffer = new byte[mySerialPort.ReadBufferSize];

                                                int bytesRead = 0;
                                                var sb = new StringBuilder();

                                                while (mySerialPort.BytesToRead > 0)
                                                {
                                                    bytesRead = mySerialPort.Read(buffer, 0, buffer.Length);
                                                    sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                                                }

                                                receivedTextBox.Text = sb.ToString();

                                                if (logging_check.Checked)
                                                {
                                                    var logFilePath = LogFilePath;
                                                    using var logFile = new StreamWriter(logFilePath, true);
                                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED STRING]: {receivedTextBox.Text}\n");
                                                }

                                                currentTextBoxIndex++; // increment the index of the current textbox
                                                checklimit++;
                                            }
                                            else
                                            {
                                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                textBoxesPanel2.Controls.Clear();
                                                checklimit = 0;
                                                currentTextBoxIndex = 0;
                                            }
                                        }

                                        await Task.Delay(500);

                                    }

                                    textBoxesPanelIndex += Math.Min(inputValues.Count, MAX_BUFFER_SIZE); // increment the index for the next iteration

                                }



                                mySerialPort.Close();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";
                            }

                            else if (currentMode == "Send Mode" && currentSTOption == "Byte")
                            {

                                var inputValues = new List<string>();
                                foreach (var textBox in textBoxesPanel.Controls.OfType<TextBox>())
                                {
                                    if (!string.IsNullOrEmpty(textBox.Text))
                                    {
                                        inputValues.AddRange(textBox.Text.Split(' '));
                                    }
                                }

                                if (!inputValues.Any())
                                {
                                    MessageBox.Show("Please input byte values\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                var bytesToSend = new List<byte>();
                                foreach (var value in inputValues)
                                {
                                    if (byte.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var byteValue))
                                    {
                                        bytesToSend.Add(byteValue);
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Error: Unable to parse byte value '{value}'", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }

                                var delay = TimeSpan.FromMilliseconds(currentDelayint);

                                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                                {
                                    Handshake = currentHandshakevalue,
                                    ReadTimeout = int.Parse(currentReadTimeout),
                                    WriteTimeout = int.Parse(currentWriteTimeout)
                                };

                                if (!mySerialPort.IsOpen) mySerialPort.Open();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                                if (!repeat_check.Checked)
                                {
                                    int i = 0;
                                    var textBoxArray = new TextBox[Math.Min(bytesToSend.Count, MAX_BUFFER_SIZE)];
                                    for (var j = 0; j < textBoxArray.Length; j++)
                                    {
                                        textBoxArray[j] = new TextBox
                                        {
                                            Location = new Point(10, j * 20 + i * 200),         // ADJUST THIS CODE LINE
                                            Width = 100,
                                            ReadOnly = true
                                        };
                                        textBoxesPanel2.Controls.Add(textBoxArray[j]);
                                    }

                                    var index = 0;

                                    foreach (var byteValue in bytesToSend)
                                    {
                                        Console.WriteLine("Byte value: {0}", byteValue);

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE]: {byteValue}");
                                        }

                                        mySerialPort.Write(new[] { byteValue }, 0, 1);

                                        Thread.Sleep(currentDelayint);
                                        if (receive_check.Checked)
                                        {
                                            if (index < textBoxArray.Length)
                                            {
                                                var receivedTextBox = textBoxArray[index];
                                                receivedTextBox.Text = Convert.ToString(mySerialPort.ReadByte());

                                                if (logging_check.Checked)
                                                {
                                                    var logFilePath = LogFilePath;

                                                    using var logFile = new StreamWriter(logFilePath, true);
                                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE]: {receivedTextBox.Text}");
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                            }

                                            index++;
                                        }
                                    }
                                    i++;


                                }

                                while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                                {
                                    int i = 0;
                                    var textBoxArray = new TextBox[Math.Min(bytesToSend.Count, MAX_BUFFER_SIZE)];
                                    for (var j = 0; j < textBoxArray.Length; j++)
                                    {
                                        textBoxArray[j] = new TextBox
                                        {
                                            Location = new Point(10, j * 20 + i * 200),         // ADJUST THIS CODE LINE
                                            Width = 100,
                                            ReadOnly = true
                                        };
                                        textBoxesPanel2.Controls.Add(textBoxArray[j]);

                                        textBoxArray[j].Show();
                                    }

                                    var index = 0;

                                    foreach (var byteValue in bytesToSend)
                                    {
                                        Console.WriteLine("Byte value: {0}", byteValue);

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE]: {byteValue}");
                                        }

                                        mySerialPort.Write(new[] { byteValue }, 0, 1);

                                        Thread.Sleep(currentDelayint);
                                        if (receive_check.Checked)
                                        {
                                            if (index < textBoxArray.Length && textBoxesPanel2.Controls.Count < MAX_BUFFER_SIZE)
                                            {
                                                var receivedTextBox = textBoxArray[index];
                                                receivedTextBox.Text = Convert.ToString(mySerialPort.ReadByte());

                                                if (logging_check.Checked)
                                                {
                                                    var logFilePath = LogFilePath;

                                                    using var logFile = new StreamWriter(logFilePath, true);
                                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE]: {receivedTextBox.Text}");
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                textBoxesPanel2.Controls.Clear();
                                                index = -1;
                                            }

                                            index++;
                                        }
                                        await Task.Delay(500);
                                    }
                                    i++;


                                }

                                mySerialPort.Close();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";
                            }

                            else if (currentMode == "Send Mode" && currentSTOption == "Byte Collection")
                            {

                                var byteCollections = new List<byte[]>();
                                foreach (var textBox in textBoxesPanel.Controls.OfType<TextBox>())
                                {
                                    if (!string.IsNullOrEmpty(textBox.Text))
                                    {
                                        var byteValues = textBox.Text.Split(' ')
                                            .Where(x => !string.IsNullOrEmpty(x))
                                            .Select(x => byte.TryParse(x, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result) ? result : (byte)0)
                                            .ToArray();

                                        if (byteValues.Length > 0)
                                        {
                                            byteCollections.Add(byteValues);
                                        }
                                    }
                                }

                                if (!byteCollections.Any())
                                {
                                    MessageBox.Show("Please input byte values\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                var delay = TimeSpan.FromMilliseconds(currentDelayint);

                                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                                {
                                    Handshake = currentHandshakevalue,
                                    ReadTimeout = int.Parse(currentReadTimeout),
                                    WriteTimeout = int.Parse(currentWriteTimeout)
                                };

                                if (!mySerialPort.IsOpen) mySerialPort.Open();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                                var textBoxList = new List<TextBox>();
                                var textBoxIndex = 0;

                                if (!repeat_check.Checked)
                                {

                                    var textBoxArray = new TextBox[Math.Min(byteCollections.Count, MAX_BUFFER_SIZE)];
                                    for (var j = 0; j < textBoxArray.Length; j++)
                                    {
                                        var textBox = new TextBox
                                        {
                                            Location = new Point(10, (textBoxIndex + j) * 20),
                                            Width = 100,
                                            ReadOnly = true
                                        };
                                        textBoxList.Add(textBox);
                                        textBoxesPanel2.Controls.Add(textBox);
                                    }

                                    var index = 0;

                                    foreach (var byteCollection in byteCollections)
                                    {
                                        Console.WriteLine("Byte collection: {0}", BitConverter.ToString(byteCollection));

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE COLLECTION]: {BitConverter.ToString(byteCollection)}");
                                        }

                                        mySerialPort.Write(byteCollection, 0, byteCollection.Length);

                                        Thread.Sleep(currentDelayint);
                                        if (receive_check.Checked)
                                        {
                                            if (index < textBoxArray.Length)
                                            {
                                                var receivedTextBox = textBoxList[textBoxIndex + index];
                                                receivedTextBox.Text = mySerialPort.ReadExisting();

                                                if (logging_check.Checked)
                                                {
                                                    var logFilePath = LogFilePath;

                                                    using var logFile = new StreamWriter(logFilePath, true);
                                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE COLLECTION]: {receivedTextBox.Text}");
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            }

                                            index++;
                                        }
                                    }

                                    textBoxIndex += textBoxArray.Length;

                                }

                                while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                                {
                                    var textBoxArray = new TextBox[Math.Min(byteCollections.Count, MAX_BUFFER_SIZE)];
                                    for (var j = 0; j < textBoxArray.Length; j++)
                                    {
                                        var textBox = new TextBox
                                        {
                                            Location = new Point(10, (textBoxIndex + j) * 20),
                                            Width = 100,
                                            ReadOnly = true
                                        };
                                        textBoxList.Add(textBox);
                                        textBoxesPanel2.Controls.Add(textBox);
                                    }

                                    var index = 0;

                                    foreach (var byteCollection in byteCollections)
                                    {
                                        Console.WriteLine("Byte collection: {0}", BitConverter.ToString(byteCollection));

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE COLLECTION]: {BitConverter.ToString(byteCollection)}");
                                        }

                                        mySerialPort.Write(byteCollection, 0, byteCollection.Length);

                                        Thread.Sleep(currentDelayint);
                                        if (receive_check.Checked)
                                        {
                                            if (index < textBoxArray.Length && textBoxesPanel2.Controls.Count <= MAX_BUFFER_SIZE)
                                            {
                                                var receivedTextBox = textBoxList[textBoxIndex + index];
                                                receivedTextBox.Text = mySerialPort.ReadExisting();

                                                if (logging_check.Checked)
                                                {
                                                    var logFilePath = LogFilePath;

                                                    using var logFile = new StreamWriter(logFilePath, true);
                                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE COLLECTION]: {receivedTextBox.Text}");
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                index = -1;
                                                textBoxesPanel2.Controls.Clear();
                                            }

                                            index++;
                                        }

                                        await Task.Delay(500);
                                    }

                                    textBoxIndex += textBoxArray.Length;

                                }

                                mySerialPort.Close();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";
                            }

                            else if (currentMode == "Send Mode" && currentSTOption == "ASCII (SEND ONLY)")
                            {

                                var inputValues = new List<string>();
                                foreach (var textBox in textBoxesPanel.Controls.OfType<TextBox>())
                                {
                                    if (!string.IsNullOrEmpty(textBox.Text))
                                    {
                                        inputValues.AddRange(textBox.Text.Split(' '));
                                    }
                                }

                                if (!inputValues.Any())
                                {
                                    MessageBox.Show("Please input ASCII values\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                var asciiBytes = new List<byte>();
                                foreach (var value in inputValues)
                                {
                                    if (byte.TryParse(value, out var asciiByte))
                                    {
                                        asciiBytes.Add(asciiByte);
                                    }
                                }

                                var hexBytes = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(asciiBytes.ToArray()));


                                var delay = TimeSpan.FromMilliseconds(currentDelayint);

                                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                                {
                                    Handshake = currentHandshakevalue,
                                    ReadTimeout = int.Parse(currentReadTimeout),
                                    WriteTimeout = int.Parse(currentWriteTimeout)
                                };

                                if (!mySerialPort.IsOpen) mySerialPort.Open();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                                int textBoxLocationY = textBoxesPanel2.Controls.Count * 20;

                                if (!repeat_check.Checked)
                                {

                                    int index = 0;

                                    foreach (var hexByte in hexBytes)
                                    {
                                        Console.WriteLine("ASCII value: {0} represents: {1}", hexByte, Convert.ToChar(hexByte));

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT ASCII]: {hexByte.ToString("X2")}");
                                        }

                                        mySerialPort.Write(new byte[] { hexByte }, 0, 1);

                                        Thread.Sleep(currentDelayint);

                                        if (receive_check.Checked)
                                        {

                                            var receivedTextBox = new TextBox
                                            {
                                                Location = new Point(10, textBoxLocationY),
                                                Width = 100,
                                                ReadOnly = true,
                                                Text = mySerialPort.ReadExisting()
                                            };

                                            textBoxesPanel2.Controls.Add(receivedTextBox);

                                            if (logging_check.Checked)
                                            {
                                                var logFilePath = LogFilePath;

                                                using var logFile = new StreamWriter(logFilePath, true);
                                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED ASCII]: {receivedTextBox.Text}");
                                            }

                                            textBoxLocationY += 20;
                                            index++;

                                        }
                                    }

                                }

                                int index2 = 0;
                                while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                                {
                                    byte[] hexBytesArray = hexBytes.ToArray();

                                    for (int i = 0; i < hexBytesArray.Length; i++)
                                    {
                                        var hexByte = hexBytesArray[i];
                                        Console.WriteLine("ASCII value: {0} represents: {1}", hexByte, Convert.ToChar(hexByte));

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT ASCII]: {hexByte.ToString("X2")}");
                                        }

                                        mySerialPort.Write(new byte[] { hexByte }, 0, 1);

                                        Thread.Sleep(currentDelayint);

                                        if (receive_check.Checked)
                                        {
                                            var receivedBytes = new byte[1024];
                                            int bytesRead = mySerialPort.Read(receivedBytes, 0, receivedBytes.Length);
                                            var receivedText = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);

                                            var receivedTextBox = new TextBox
                                            {
                                                Location = new Point(10, 20 + (index2 % MAX_BUFFER_SIZE) * 20),
                                                Width = 100,
                                                ReadOnly = true,
                                                Text = receivedText
                                            };

                                            if (index2 >= MAX_BUFFER_SIZE)
                                            {
                                                textBoxesPanel2.Controls.RemoveAt(0);
                                            }

                                            textBoxesPanel2.Controls.Add(receivedTextBox);

                                            if (logging_check.Checked)
                                            {
                                                var logFilePath = LogFilePath;

                                                using var logFile = new StreamWriter(logFilePath, true);
                                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED ASCII]: {receivedText}");
                                            }

                                            index2++;
                                        }

                                        if (index2 >= MAX_BUFFER_SIZE)
                                        {
                                            MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            index2 = 0;
                                            textBoxesPanel2.Controls.Clear();
                                        }

                                    }


                                }


                                mySerialPort.Close();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

                            }

                            else if (currentMode == "Send Mode" && currentSTOption == "ASCII-HEX (SEND ONLY)")
                            {


                                var inputValues = new List<string>();
                                foreach (var textBox in textBoxesPanel.Controls.OfType<TextBox>())
                                {
                                    if (!string.IsNullOrEmpty(textBox.Text))
                                    {
                                        inputValues.AddRange(textBox.Text.Split(' '));
                                    }
                                }

                                if (!inputValues.Any())
                                {
                                    MessageBox.Show("Please input HEX values\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                var hexBytes = new List<byte>();
                                foreach (var value in inputValues)
                                {
                                    if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var hexByte))
                                    {
                                        hexBytes.Add(hexByte);
                                    }
                                }


                                var delay = TimeSpan.FromMilliseconds(currentDelayint);

                                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                                {
                                    Handshake = currentHandshakevalue,
                                    ReadTimeout = int.Parse(currentReadTimeout),
                                    WriteTimeout = int.Parse(currentWriteTimeout)
                                };

                                if (!mySerialPort.IsOpen) mySerialPort.Open();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                                int textBoxLocationY = textBoxesPanel2.Controls.Count * 20;

                                if (!repeat_check.Checked)
                                {

                                    int index = 0;

                                    foreach (var hexByte in hexBytes)
                                    {
                                        Console.WriteLine("HEX value: {0} represents the byte: {1}", hexByte.ToString("X2"), hexByte);

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT HEX]: {hexByte.ToString("X2")}");
                                        }

                                        mySerialPort.Write(new byte[] { hexByte }, 0, 1);

                                        Thread.Sleep(currentDelayint);

                                        if (receive_check.Checked)
                                        {

                                            var receivedTextBox = new TextBox
                                            {
                                                Location = new Point(10, textBoxLocationY),
                                                Width = 100,
                                                ReadOnly = true,
                                                Text = mySerialPort.ReadExisting()
                                            };

                                            textBoxesPanel2.Controls.Add(receivedTextBox);

                                            if (logging_check.Checked)
                                            {
                                                var logFilePath = LogFilePath;

                                                using var logFile = new StreamWriter(logFilePath, true);
                                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED HEX]: {receivedTextBox.Text}");
                                            }

                                            textBoxLocationY += 20;
                                            index++;

                                        }
                                    }

                                }

                                int index2 = 0;
                                while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)
                                {
                                    byte[] hexBytesArray = hexBytes.ToArray();

                                    for (int i = 0; i < hexBytesArray.Length; i++)
                                    {
                                        var hexByte = hexBytesArray[i];
                                        Console.WriteLine("HEX value: {0} represents the byte: {1}", hexByte.ToString("X2"), hexByte);

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT HEX]: {hexByte.ToString("X2")}");
                                        }

                                        mySerialPort.Write(new byte[] { hexByte }, 0, 1);

                                        Thread.Sleep(currentDelayint);

                                        if (receive_check.Checked)
                                        {
                                            var receivedBytes = new byte[1024];
                                            int bytesRead = mySerialPort.Read(receivedBytes, 0, receivedBytes.Length);
                                            var receivedText = Encoding.ASCII.GetString(receivedBytes, 0, bytesRead);

                                            var receivedTextBox = new TextBox
                                            {
                                                Location = new Point(10, 20 + (index2 % MAX_BUFFER_SIZE) * 20),
                                                Width = 100,
                                                ReadOnly = true,
                                                Text = receivedText
                                            };

                                            if (index2 >= MAX_BUFFER_SIZE)
                                            {
                                                textBoxesPanel2.Controls.RemoveAt(0);
                                            }

                                            textBoxesPanel2.Controls.Add(receivedTextBox);

                                            if (logging_check.Checked)
                                            {
                                                var logFilePath = LogFilePath;

                                                using var logFile = new StreamWriter(logFilePath, true);
                                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED HEX]: {receivedText}");
                                            }

                                            index2++;
                                        }

                                        if (index2 >= MAX_BUFFER_SIZE)
                                        {
                                            MessageBox.Show($"Maximum buffer of {MAX_BUFFER_SIZE} exceeded", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            index2 = 0;
                                            textBoxesPanel2.Controls.Clear();
                                        }

                                        await Task.Delay(500);
                                    }


                                }


                                mySerialPort.Close();
                                if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

                            }

                            else
                            {
                                MessageBox.Show("Either Send Mode not active OR Invalid Send Data Type", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                        }
                    }
                    catch (SystemException ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // DECLARE FUNCTIONS
        private void AddLabel(string labelText, Point location, Font font)      // creates the labels on the MainDisplay form
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = location;
            label.AutoSize = true;
            label.Font = font;
            label.ForeColor = Color.Black;
            this.Controls.Add(label);
        }

        void OnHelp()                                                           // function to open the info/help form
        {
            new Info(this).Show();

        }

        void OnClose()  // function to close the program
        {
            Close();
        }

        void OnReload() // function to reload the ports/program

        {
            existingPorts = AvailablePorts.ToList();
            currentCom?.DropDownItems.Clear();
            existingPorts.ForEach(port => currentCom?.DropDownItems.Add(port));

            TextBox? textBox = textBoxesPanel2?.Controls.OfType<TextBox>().FirstOrDefault();
            if (textBox != null) textBox.Text = "";
            textBoxesPanel2?.Controls.Clear();
        }

        public MainDisplay()  // main form with design components
        {
            InitializeComponent();


        }
    }
}