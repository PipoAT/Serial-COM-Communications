using System.IO.Ports;
using System.Text;
using System.Globalization;
using System.Diagnostics;


namespace AT_SCC
{

    public partial class MainDisplay : Form
    {                                           // declaring all variables associated with textboxes, menu/tool strip, strings, or int
        private TextBox? textBoxCOM, textBoxBAUD, textBoxPARITY, textBoxDATABITS, textBoxSTOPBITS, textBoxRTIMEOUT, textBoxWTIMEOUT, textBoxHANDSHAKE, textBoxDELAY, textBoxBTT, textBoxMODEDISP, textBoxSENDTYPE, textBoxreceiveType;

        private TextBox? clock, textBoxBUFFER, textBoxSTATUS;   // sets up the textboxes

        private Panel? textBoxesPanel, textBoxesPanel2; // sets up the tx and rx panels

        private MenuStrip? menuStrip;   // sets up the menu strip and its items

        private ToolStripMenuItem? fileMenuItem, comPortMenuItem, sendMenuItem, loggingMenuItem, exitMenuItem, reloadMenuItem, modeMenuItem, currentCom, baudMenuItem, parityMenuItem, dataBitsMenuItem, stopBitsMenuItem, readTimeoutItem, writeTimeoutItem, handshakeItem, sendDelay, receivingModeMenuItem, sendingModeMenuItem, portengageMenuItem;

        private CancellationTokenSource? cancellationTokenSource;   // declares the stop button functionality

        List<string>? existingPorts;

        public static string[] AvailablePorts => System.IO.Ports.SerialPort.GetPortNames(); // provides all the ports available

        string currentPort = "", currentBaud = "", currentParity = "None", stopBitOption = "", currentReadTimeout = "-1", currentWriteTimeout = "-1", currentHandshakeOption = "None", dataBitoption = "", currentDelayOption = "1000", currentMode = "", currentLogMode = "", currentRTOption = "", currentSTOption = "";

        int currentBaudint, currentdataBitsint = 8, currentDelayint = 1000, breakval = 0; // declare integers and strings for default values for settings of port


        Parity currentParityvalue = Parity.None;                      // declaring variables to allow usage of dictionaries
        StopBits currentStopBitvalue = StopBits.One;

        Handshake currentHandshakevalue = Handshake.None;


        CheckBox logging_check = new CheckBox(), repeat_check = new CheckBox(), overwrite_check = new CheckBox(); // define the checkboxes

        public string LogFilePath = Path.GetFullPath("SerialLog.txt"); // PATH TO TEXT FILE
        public int MAX_BUFFER_SIZE = 100;       // MAX BUFFER SIZE


        // dictionaries/list of strings for user selection and input
        public readonly string[] PossibleBauds = new string[]
        {
            "75","110","134","150","300","600","1200","1800","2400","4800","7200","9600","14400","19200","38400","57600","115200","128000"
        };

        public readonly string[] PossibleTransmitModes = new string[]
        {
            "N/A - DISABLED", "Byte", "String", "Byte Collection", "ASCII", "ASCII-HEX"
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
            if (e.ClickedItem != null && textBoxMODEDISP != null) { textBoxMODEDISP.Text = currentMode = e.ClickedItem.Text; }
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

        void OnEngage(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null || textBoxSTATUS == null)
            {
                return;
            }

            try
            {
                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                {
                    Handshake = currentHandshakevalue,
                    ReadTimeout = int.Parse(currentReadTimeout),
                    WriteTimeout = int.Parse(currentWriteTimeout)
                };

                textBoxSTATUS.Text = e.ClickedItem.Text == "Open Port" ? "PORT OPEN" : "PORT CLOSED";
                if (e.ClickedItem.Text == "Open Port")
                {
                    mySerialPort.Open();
                }
                else
                {
                    mySerialPort.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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


            if (e.ClickedItem != null && textBoxreceiveType != null && textBoxSENDTYPE != null)
            {
                textBoxreceiveType.Text = currentRTOption = e.ClickedItem.Text;

            }
        }

        void OnSModeChange(object? sender, ToolStripItemClickedEventArgs e)         // event to select the mode of the tool
        {
            if (textBoxreceiveType != null && textBoxSENDTYPE != null)
            {
                textBoxSENDTYPE.Text = currentSTOption = e.ClickedItem?.Text ?? "N/A - DISABLED";

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

        private void TextBoxBTT_TextChanged(object sender, EventArgs e)
        {
            List<string> textBoxValues = new List<string>();

            if (!int.TryParse(textBoxBTT?.Text, out int btt) || btt < 0)    // if invalid input for TX buffer that does not exceed buffer
            {
                MessageBox.Show("Desired transfer size needs to be a positive integer.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxesPanel?.Controls.Clear();
                textBoxesPanel2?.Controls.Clear();
                textBoxBTT!.Text = "0";
                return;
            }

            textBoxValues.Clear();
            foreach (var control in textBoxesPanel!.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBoxValues.Add(textBox.Text);
                }
            }

            this.textBoxesPanel?.Controls.Clear();
            Enumerable.Range(1, btt).ToList().ForEach(i =>
            {
                TextBox textBox = new TextBox() { Location = new Point(5, 10 + (i - 1) * 20) };
                if (i <= textBoxValues.Count) textBox.Text = textBoxValues[i - 1];
                this.textBoxesPanel?.Controls.Add(textBox);
            });
        }

        private void Timer_Tick(object? sender, EventArgs e)        // event to display the time and date
        {
            if (clock != null) clock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Display current time in textbox

        }

        private void StopButton_Click(object sender, EventArgs e)           // event for button to stop the repeat
        {
            // Cancel the loop
            cancellationTokenSource?.Cancel();
        }

        private async void TX_RX_String() // sends and/or reads strings
        {
            int i = 0;
            using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
            {
                Handshake = currentHandshakevalue,
                ReadTimeout = int.Parse(currentReadTimeout),
                WriteTimeout = int.Parse(currentWriteTimeout)
            };  // sets up serial port

            if (!mySerialPort.IsOpen) mySerialPort.Open();  // checks if port is open and if not opens it

            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";    // status message

            var textBoxArray = new TextBox[MAX_BUFFER_SIZE];    // sets up array for textboxes and creates them
            for (var k = 0; k < textBoxArray.Length; k++)
            {
                textBoxArray[k] = new TextBox
                {
                    Location = new Point(10, k * 20),
                    Width = 100,
                    ReadOnly = true
                };
                textBoxesPanel2?.Controls.Add(textBoxArray[k]);
            }


            do
            {   // if buffer hits, check if user wants overwrite 
                if (i >= MAX_BUFFER_SIZE)
                {
                    if (!overwrite_check.Checked) break;

                    else i = 0;
                }

                if (textBoxSENDTYPE?.Text == "String" && mySerialPort.IsOpen)
                {
                    foreach (var control in textBoxesPanel!.Controls)
                    {
                        if (control is TextBox textBox)
                        {
                            // Send the textbox contents as a string
                            var textToSend = textBox.Text;
                            mySerialPort.WriteLine(textToSend);

                            if (logging_check.Checked)
                            {
                                var logFilePath = LogFilePath;
                                using var logFile = new StreamWriter(logFilePath, true);
                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT STRING]: {textToSend}\n");
                            }

                            await Task.Delay(500);
                        }
                    }
                }

                if (textBoxreceiveType?.Text == "String")
                {
                    var receivedTextBox = textBoxArray[i];
                    receivedTextBox.Text = mySerialPort.ReadLine(); // reads the string and checks if logging is needed and outputs to panel

                    if (logging_check.Checked)
                    {
                        var logFilePath = LogFilePath;
                        using var logFile = new StreamWriter(logFilePath, true);
                        logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED STRING]: {receivedTextBox.Text}\n");
                    }

                    if (receivedTextBox.TextLength >= receivedTextBox.Width * 3) receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);

                    i++;

                }
                await Task.Delay(500);
            } while (repeat_check.Checked && !cancellationTokenSource!.IsCancellationRequested && mySerialPort.IsOpen);


            mySerialPort.Close(); // auto closes the port
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

        }

        private async void RX_BYTE() // Reads bytes or byte collections
        {
            int i = 0;
            var receivedBytes = new List<byte>();   // sets up new list of bytes

            using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
            {
                Handshake = currentHandshakevalue,
                ReadTimeout = int.Parse(currentReadTimeout),
                WriteTimeout = int.Parse(currentWriteTimeout)
            };  // sets up serial port

            if (!mySerialPort.IsOpen) mySerialPort.Open();      // checks for if port is open and status message is set
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

            var byteCollections = new List<List<byte>>();   // sets up the textbox array and textboxes itself
            var textBoxArray = new TextBox[MAX_BUFFER_SIZE];
            for (var k = 0; k < textBoxArray.Length; k++)
            {
                textBoxArray[k] = new TextBox
                {
                    Location = new Point(10, k * 20),
                    Width = 100,
                    ReadOnly = true
                };
                textBoxesPanel2?.Controls.Add(textBoxArray[k]);
            }

            do
            {
                if (mySerialPort.BytesToRead <= 0)
                {
                    break;
                }
                // reads the bytes and outputs to the panel, also checks to make sure it can do so
                if (textBoxArray.Length > 0)
                {
                    var receivedTextBox = textBoxArray[i];
                    if (textBoxreceiveType?.Text == "Byte Collection") receivedTextBox.Text = Convert.ToString(mySerialPort.ReadByte());
                    else if (textBoxreceiveType?.Text == "Byte") receivedTextBox.Text += $"{mySerialPort.ReadByte():X2} ";

                    if (receivedTextBox.TextLength >= receivedTextBox.Width * 3) receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);


                    if (logging_check.Checked)
                    {
                        var logFilePath = LogFilePath;

                        using var logFile = new StreamWriter(logFilePath, true);
                        logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE/BYTE COLLECTION]: {receivedTextBox.Text}");
                    }
                }

                if (i >= MAX_BUFFER_SIZE - 1)   // if buffer is hit, check if user wants overwrite
                {
                    if (!(overwrite_check.Checked)) break;
                    else i = -1;

                }

                i++;

                if (!(repeat_check.Checked)) break;

                await Task.Delay(500);
            } while (!cancellationTokenSource!.IsCancellationRequested);

            mySerialPort.Close();   // auto closes the port
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";
        }

        private async void TX_RX_ASCII() // Sends and receives ASCII or ASCII Hex values
        {

            var inputValues = new List<string>();       // sets up the textboxes
            foreach (var textBox in textBoxesPanel!.Controls.OfType<TextBox>())
            {
                if (!string.IsNullOrEmpty(textBox.Text)) inputValues.AddRange(textBox.Text.Split(' '));
            }

            if (!inputValues.Any()) // checks for any data
            {
                MessageBox.Show("Please input ASCII/HEX values\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            byte[] hexBytes = new byte[0]; // declare the variable as a byte array

            if (textBoxSENDTYPE?.Text == "ASCII")
            {
                var asciiBytes = new List<byte>();
                foreach (var value in inputValues)
                {
                    if (byte.TryParse(value, out var asciiByte))
                    {
                        asciiBytes.Add(asciiByte);
                    }
                }
                hexBytes = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(asciiBytes.ToArray())); // assign a value to hexBytes in this branch
            }
            else if (textBoxSENDTYPE?.Text == "ASCII-HEX")
            {
                var hexBytesList = new List<byte>();
                foreach (var value in inputValues)
                {
                    if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var hexByte))
                    {
                        hexBytesList.Add(hexByte);
                    }
                }
                hexBytes = hexBytesList.ToArray(); // convert the List<byte> to a byte array and assign it to hexBytes
            }

            // now you can use the variable hexBytes outside of the if statemen


            var delay = TimeSpan.FromMilliseconds(currentDelayint); // delay for transmission

            using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
            {
                Handshake = currentHandshakevalue,
                ReadTimeout = int.Parse(currentReadTimeout),
                WriteTimeout = int.Parse(currentWriteTimeout)
            };  // sets up the serial port

            if (!mySerialPort.IsOpen) mySerialPort.Open();  // checks for if port is open and sets status message
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";
            int textBoxLocationY = textBoxesPanel2!.Controls.Count * 20;    // sets up textbox y location

            if (!repeat_check.Checked)  // if only running code once // ensure correct configurations
            {
                if (textBoxreceiveType?.Text != "N/A - DISABLED" && (textBoxreceiveType?.Text != "ASCII" && textBoxreceiveType?.Text != "ASCII-HEX")) MessageBox.Show("Misconfiguration Warning. Please adjust settings and try again", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                else
                {

                    int index = 0;

                    foreach (var hexByte in hexBytes)   // sends the data and checks if user needs logging
                    {

                        if (logging_check.Checked)
                        {
                            var logFilePath = LogFilePath;

                            using var logFile = new StreamWriter(logFilePath, true);
                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT ASCII]: {hexByte.ToString("X2")}");
                        }

                        mySerialPort.Write(new byte[] { hexByte }, 0, 1);

                        Thread.Sleep(currentDelayint);

                        if (textBoxreceiveType?.Text == "ASCII" || textBoxreceiveType?.Text == "ASCII-HEX")    // if also receiving, it will receive what is sent and outputs to the panel
                        {

                            var receivedTextBox = new TextBox   // creates the textboxes
                            {
                                Location = new Point(10, textBoxLocationY),
                                Width = 100,
                                ReadOnly = true,
                                Text = mySerialPort.ReadExisting()
                            };

                            textBoxesPanel2.Controls.Add(receivedTextBox);

                            if (logging_check.Checked)  // logging if user needs to
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

            }

            int index2 = 0;
            while (!cancellationTokenSource!.IsCancellationRequested && repeat_check.Checked)    // if repeat is enabled
            {
                if (textBoxreceiveType?.Text != "N/A - DISABLED" && (textBoxreceiveType?.Text != "ASCII" && textBoxreceiveType?.Text != "ASCII-HEX"))    // configuration correctness
                {
                    MessageBox.Show("Misconfiguration Warning. Please adjust settings and try again", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }

                byte[] hexBytesArray = hexBytes.ToArray();  // sets up data and sends data with ability to see if user wanta logging

                for (int i = 0; i < hexBytesArray.Length; i++)
                {
                    var hexByte = hexBytesArray[i];

                    if (logging_check.Checked)
                    {
                        var logFilePath = LogFilePath;

                        using var logFile = new StreamWriter(logFilePath, true);
                        logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT ASCII]: {hexByte.ToString("X2")}");
                    }

                    mySerialPort.Write(new byte[] { hexByte }, 0, 1);

                    Thread.Sleep(currentDelayint);

                    if (textBoxreceiveType?.Text == "ASCII" || textBoxreceiveType?.Text == "ASCII-HEX")    // if also receiving, it will receives the sent data and outputs to panel
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

                        textBoxesPanel2.Controls.Add(receivedTextBox);

                        if (logging_check.Checked) // checks logging needs from user
                        {
                            var logFilePath = LogFilePath;

                            using var logFile = new StreamWriter(logFilePath, true);
                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED ASCII]: {receivedText}");
                        }

                        index2++;
                    }

                    if (index2 >= MAX_BUFFER_SIZE)  // if buffer is hit, see if overwriting data is wanted by user
                    {
                        index2 = 0;

                        if (!overwrite_check.Checked) breakval = 1;
                        else textBoxesPanel2.Controls.Clear();

                    }

                    await Task.Delay(500);

                }

                if (breakval == 1)
                {    // if no overwriting, exit the loop and end process
                    breakval = 0;
                    break;
                }


            }

            mySerialPort.Close();   // auto close the port
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

        }

        private async void Transmission_Click(object? sender, System.EventArgs e) // event to handle clicking the start transmission button
        {
            if (textBoxesPanel != null && textBoxesPanel2 != null)      // checks to ensure that the panels are not null, if they exist
            {
                cancellationTokenSource = new CancellationTokenSource();    // sets up the stop transmission functionality
                textBoxesPanel2?.Controls.Clear();  // clears the output panel each time the button is clicked


                if (textBoxMODEDISP?.Text == "Receive Mode")
                {
                    var modeSwitch = textBoxreceiveType?.Text;
                    switch (modeSwitch)
                    {

                        case "Byte":
                            RX_BYTE();
                            break;

                        case "String":
                            TX_RX_String();
                            break;

                        case "Byte Collection":
                            RX_BYTE();
                            break;

                        default:
                            MessageBox.Show("Misconfigured Settings. Please check settings and modify as needed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }

                else if (textBoxMODEDISP?.Text == "Send Mode")
                {

                    var modeSwitchS = textBoxSENDTYPE?.Text;
                    switch (modeSwitchS)
                    {

                        case "Byte":
                            break;

                        case "String":
                            TX_RX_String();
                            break;

                        case "Byte Collection":
                            break;

                        case "ASCII":
                            TX_RX_ASCII();
                            break;

                        case "ASCII-HEX":
                            TX_RX_ASCII();
                            break;

                        default:
                            MessageBox.Show("Misconfigured Settings. Please check settings and modify as needed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                    }
                }

                if (textBoxMODEDISP?.Text == "Send Mode" && textBoxSENDTYPE?.Text == "Byte")   // checks if it is sending a byte
                {

                    var inputValues = new List<string>();
                    foreach (var textBox in textBoxesPanel.Controls.OfType<TextBox>())  // sets up the textboxes and list of bytes to send
                    {
                        if (!string.IsNullOrEmpty(textBox.Text)) inputValues.AddRange(textBox.Text.Split(' '));
                    }

                    if (!inputValues.Any()) // checks for if any data exists
                    {
                        MessageBox.Show("Please input byte values\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var bytesToSend = new List<byte>(); // adds data to the list to send
                    foreach (var value in inputValues)
                    {
                        if (byte.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var byteValue))
                        {
                            bytesToSend.Add(byteValue);
                        }
                        else    // if data is unavailable or not correct type, throw error
                        {
                            MessageBox.Show($"Error: Unable to parse byte value '{value}'", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    var delay = TimeSpan.FromMilliseconds(currentDelayint); // delay

                    using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                    {
                        Handshake = currentHandshakevalue,
                        ReadTimeout = int.Parse(currentReadTimeout),
                        WriteTimeout = int.Parse(currentWriteTimeout)
                    };  // sets up serial port

                    if (!mySerialPort.IsOpen) mySerialPort.Open();  // checks if port is open and sets status message
                    if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                    if (!repeat_check.Checked)  // code runs if repeat is not on
                    {
                        if (textBoxreceiveType?.Text != "N/A - DISABLED" && textBoxreceiveType?.Text != "Byte") MessageBox.Show("Misconfiguration Warning. Please adjust settings and try again", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        else
                        {
                            int i = 0;
                            var textBoxArray = new TextBox[Math.Min(bytesToSend.Count, MAX_BUFFER_SIZE)]; // creates the textboxes and sets them up
                            for (var j = 0; j < textBoxArray.Length; j++)
                            {
                                textBoxArray[j] = new TextBox
                                {
                                    Location = new Point(10, j * 20 + i * 200),
                                    Width = 100,
                                    ReadOnly = true
                                };
                                textBoxesPanel2?.Controls.Add(textBoxArray[j]);
                            }

                            var index = 0;  // writes the bytes and checks for if logging is needed

                            foreach (var byteValue in bytesToSend)
                            {

                                if (logging_check.Checked)
                                {
                                    var logFilePath = LogFilePath;

                                    using var logFile = new StreamWriter(logFilePath, true);
                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE]: {byteValue}");
                                }

                                mySerialPort.Write(new[] { byteValue }, 0, 1);

                                Thread.Sleep(currentDelayint);
                                if (textBoxreceiveType?.Text == "Byte") // if receiving as well as sending, it will read that data and output onto the panel
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

                                    } // if buffer is hit, throw warning

                                    index++;
                                }

                            }
                            i++;
                        }


                    }

                    while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)    // for if the sending data is repeating
                    {

                        if (textBoxreceiveType?.Text != "N/A - DISABLED" && textBoxreceiveType?.Text != "Byte") // ensures configuration is correct
                        {

                            MessageBox.Show("Misconfiguration Warning. Please adjust settings and try again", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        }


                        int i = 0;  // sets up the textboxes
                        var textBoxArray = new TextBox[Math.Min(bytesToSend.Count, MAX_BUFFER_SIZE)];
                        for (var j = 0; j < textBoxArray.Length; j++)
                        {
                            textBoxArray[j] = new TextBox
                            {
                                Location = new Point(10, j * 20 + i * 200),
                                Width = 100,
                                ReadOnly = true
                            };
                            textBoxesPanel2?.Controls.Add(textBoxArray[j]);

                            textBoxArray[j].Show();
                        }

                        var index = 0;

                        foreach (var byteValue in bytesToSend)
                        {

                            if (logging_check.Checked)
                            {
                                var logFilePath = LogFilePath;

                                using var logFile = new StreamWriter(logFilePath, true);
                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE]: {byteValue}");
                            }

                            mySerialPort.Write(new[] { byteValue }, 0, 1);      // writes the data and also checks for logging needs

                            Thread.Sleep(currentDelayint);
                            if (textBoxreceiveType?.Text == "Byte") // if receiving, it will receive the data and output to panel
                            {
                                if (index < textBoxArray.Length && textBoxesPanel2?.Controls.Count < MAX_BUFFER_SIZE)
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
                                else // if buffer is hit, check for is user wants overwriting
                                {
                                    index = -1;

                                    if (!(overwrite_check.Checked)) breakval = 1;

                                    else textBoxesPanel2?.Controls.Clear();

                                }

                                index++;
                            }

                            await Task.Delay(500);
                        }
                        i++;

                        if (breakval == 1)
                        {        // if user does not want to overwrite, it will exit the while loop and end the sending process
                            breakval = 0;
                            break;
                        }




                    }

                    mySerialPort.Close();       // auto close the port
                    if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

                }

                else if (textBoxMODEDISP?.Text == "Send Mode" && textBoxSENDTYPE?.Text == "Byte Collection") // checks if it is sending a collection of bytes
                {
                    var byteCollections = new List<byte[]>();       // sets up the textboxes and lists for data
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

                    if (!byteCollections.Any()) // check for any data
                    {
                        MessageBox.Show("Please input byte values\nIf you are unable to input, please change the desired transfer size", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var delay = TimeSpan.FromMilliseconds(currentDelayint); // delay for transmission

                    using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                    {
                        Handshake = currentHandshakevalue,
                        ReadTimeout = int.Parse(currentReadTimeout),
                        WriteTimeout = int.Parse(currentWriteTimeout)
                    };      // sets up the serial port

                    if (!mySerialPort.IsOpen) mySerialPort.Open();  // checks if the port is open and sets status message
                    if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

                    var textBoxList = new List<TextBox>();
                    var textBoxIndex = 0;   // sets up the textboxes

                    if (!repeat_check.Checked)      // if code is running only once
                    {       // ensures configuration is correct
                        if (textBoxreceiveType?.Text != "N/A - DISABLED" && textBoxreceiveType?.Text != "Byte Collection") MessageBox.Show("Misconfiguration Warning. Please adjust settings and try again", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        else
                        {
                            var textBoxArray = new TextBox[Math.Min(byteCollections.Count, MAX_BUFFER_SIZE)];
                            for (var j = 0; j < textBoxArray.Length; j++)
                            {
                                var textBox = new TextBox   // creates the textboxes
                                {
                                    Location = new Point(10, (textBoxIndex + j) * 20),
                                    Width = 100,
                                    ReadOnly = true
                                };
                                textBoxList.Add(textBox);
                                textBoxesPanel2?.Controls.Add(textBox);
                            }

                            var index = 0;

                            foreach (var byteCollection in byteCollections) // checks for logging needs and sends the data to port
                            {

                                if (logging_check.Checked)
                                {
                                    var logFilePath = LogFilePath;

                                    using var logFile = new StreamWriter(logFilePath, true);
                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE COLLECTION]: {BitConverter.ToString(byteCollection)}");
                                }

                                mySerialPort.Write(byteCollection, 0, byteCollection.Length);

                                Thread.Sleep(currentDelayint);
                                if (textBoxreceiveType?.Text == "Byte Collection")  // if also receiving, it will read the sent data and output to panel
                                {
                                    if (index < textBoxArray.Length)
                                    {
                                        var receivedTextBox = textBoxList[textBoxIndex + index];
                                        receivedTextBox.Text = mySerialPort.ReadExisting();

                                        if (logging_check.Checked)
                                        {
                                            var logFilePath = LogFilePath;

                                            using var logFile = new StreamWriter(logFilePath, true);        // checks for logging
                                            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE COLLECTION]: {receivedTextBox.Text}");
                                        }
                                    }   // if buffer is hit, throw warning

                                    index++;
                                }

                            }

                            textBoxIndex += textBoxArray.Length;
                        }

                    }

                    while (!cancellationTokenSource.IsCancellationRequested && repeat_check.Checked)    // if repeating is enabled
                    {

                        if (textBoxreceiveType?.Text != "N/A - DISABLED" && textBoxreceiveType?.Text != "Byte Collection")  // ensure correct configurations
                        {

                            MessageBox.Show("Misconfiguration Warning. Please adjust settings and try again", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;

                        }

                        var textBoxArray = new TextBox[Math.Min(byteCollections.Count, MAX_BUFFER_SIZE)];   // sets up textboxes
                        for (var j = 0; j < textBoxArray.Length; j++)
                        {
                            var textBox = new TextBox
                            {
                                Location = new Point(10, (textBoxIndex + j) * 20),
                                Width = 100,
                                ReadOnly = true
                            };
                            textBoxList.Add(textBox);
                            textBoxesPanel2?.Controls.Add(textBox);
                        }

                        var index = 0;

                        foreach (var byteCollection in byteCollections) // checks if logging is needed and sends the data to port
                        {

                            if (logging_check.Checked)
                            {
                                var logFilePath = LogFilePath;

                                using var logFile = new StreamWriter(logFilePath, true);
                                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE COLLECTION]: {BitConverter.ToString(byteCollection)}");
                            }

                            mySerialPort.Write(byteCollection, 0, byteCollection.Length);

                            Thread.Sleep(currentDelayint);
                            if (textBoxreceiveType?.Text == "Byte Collection")  // if also receiving, it will read data its sent and output to panel
                            {
                                if (index < textBoxArray.Length && textBoxesPanel2?.Controls.Count <= MAX_BUFFER_SIZE)
                                {
                                    var receivedTextBox = textBoxList[textBoxIndex + index];
                                    receivedTextBox.Text = mySerialPort.ReadExisting();

                                    if (logging_check.Checked)  // if logging is wanted, it will log
                                    {
                                        var logFilePath = LogFilePath;

                                        using var logFile = new StreamWriter(logFilePath, true);
                                        logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE COLLECTION]: {receivedTextBox.Text}");
                                    }
                                }
                                else
                                { // if buffer is hit, see if user wants overwrite of data
                                    index = -1;
                                    if (!overwrite_check.Checked) breakval = 1;

                                    else textBoxesPanel2?.Controls.Clear();
                                }

                                index++;
                            }


                            await Task.Delay(500);
                        }

                        textBoxIndex += textBoxArray.Length;

                        if (breakval == 1)
                        {    // if no overwriting is wanted, then it exits the loop and ends the process
                            breakval = 0;
                            break;
                        }



                    }

                    mySerialPort.Close();
                    if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";
                }

            }

        }

        // DECLARE FUNCTIONS
        public void AddLabel(string labelText, Point location, Font font)      // creates the labels on the MainDisplay form
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = location;
            label.AutoSize = true;
            label.Font = font;
            label.ForeColor = Color.Black;
            this.Controls.Add(label);
        }

        void OnHelp()       // Launches to live external website for help/info
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd";
            psi.Arguments = $"/c start https://pipoat.github.io/";
            Process.Start(psi);
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
            InitializeComponent(); // calls the components fuction in the MainDisplay.Designer.cs file

        }
    }
}