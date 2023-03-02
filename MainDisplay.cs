using System.IO.Ports;
using System.Text;
using System.Globalization;
using System.Diagnostics;


namespace AT_SCC
{

    public partial class MainDisplay : Form
    {                                           // declaring all variables associated with textboxes, menu/tool strip, strings, or int
        private TextBox? textBoxCOM = new TextBox(), textBoxBAUD = new TextBox(), textBoxPARITY = new TextBox(), textBoxDATABITS = new TextBox(), textBoxSTOPBITS = new TextBox(), textBoxRTIMEOUT = new TextBox(), textBoxWTIMEOUT = new TextBox(), textBoxHANDSHAKE = new TextBox(), textBoxDELAY = new TextBox(), textBoxBTT = new TextBox(), textBoxMODEDISP = new TextBox(), textBoxSENDTYPE = new TextBox(), textBoxreceiveType = new TextBox();

        private TextBox? clock = new TextBox(), textBoxBUFFER = new TextBox(), textBoxSTATUS = new TextBox();   // sets up the textboxes

        private Panel textBoxesPanel = new Panel();
        private Panel textBoxesPanel2 = new Panel();


        private MenuStrip? menuStrip;   // sets up the menu strip and its items

        private ToolStripMenuItem? fileMenuItem, comPortMenuItem, sendMenuItem, loggingMenuItem, exitMenuItem, reloadMenuItem, modeMenuItem, currentCom, baudMenuItem, parityMenuItem, dataBitsMenuItem, stopBitsMenuItem, readTimeoutItem, writeTimeoutItem, handshakeItem, sendDelay, receivingModeMenuItem, sendingModeMenuItem, portengageMenuItem;

        private CancellationTokenSource? cancellationTokenSource;   // declares the stop button functionality

        List<string>? existingPorts;

        public static string[] AvailablePorts => System.IO.Ports.SerialPort.GetPortNames(); // provides all the ports available

        string currentPort = "";

        int currentBaudint, currentdataBitsint = 8, currentDelayint = 1000, breakval = 0; // declare integers and strings for default values for settings of port


        Parity currentParityvalue = Parity.None;                      // declaring variables to allow usage of dictionaries
        StopBits currentStopBitvalue = StopBits.One;
        Handshake currentHandshakevalue = Handshake.None;

        Button STRANSMIT = new Button(), ETRANSMIT = new Button();

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

        void OnLogChange(object? sender, ToolStripItemClickedEventArgs e)       // event to view or delete logs
        {     // event to activate or deactivate the logging feature

            if (e.ClickedItem?.Text == "View Logs") new Log(this).Show();

            else if (e.ClickedItem?.Text == "Delete Logs")
            {
                System.IO.File.WriteAllText(LogFilePath, string.Empty);
                MessageBox.Show("Logs have been erased", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        void OnEngage(object sender, ToolStripItemClickedEventArgs e)       // event to open/close port
        {
            if (e.ClickedItem == null || textBoxSTATUS == null) return;

            try
            {
                using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
                {
                    Handshake = currentHandshakevalue,
                    ReadTimeout = int.Parse(textBoxRTIMEOUT!.Text),
                    WriteTimeout = int.Parse(textBoxWTIMEOUT!.Text)
                };

                textBoxSTATUS.Text = e.ClickedItem.Text == "Open Port" ? "PORT OPEN" : "PORT CLOSED";

                if (e.ClickedItem.Text == "Open Port") mySerialPort.Open();

                else mySerialPort.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TextBoxBTT_TextChanged(object sender, EventArgs e)     // event to adjust the TX BUFFER SIZE
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

        private async void TX_RX_String() // sends and/or reads strings
        {
            int i = 0;
            using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
            {
                Handshake = currentHandshakevalue,
                ReadTimeout = int.Parse(textBoxRTIMEOUT!.Text),
                WriteTimeout = int.Parse(textBoxWTIMEOUT!.Text)
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

        private async void TX_RX_BYTE() // Reads bytes or byte collections
        {
            int i = 0;
            var receivedBytes = new List<byte>();   // sets up new list of bytes

            using var mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
            {
                Handshake = currentHandshakevalue,
                ReadTimeout = int.Parse(textBoxRTIMEOUT!.Text),
                WriteTimeout = int.Parse(textBoxWTIMEOUT!.Text)
            };  // sets up serial port

            if (!mySerialPort.IsOpen) mySerialPort.Open();      // checks for if port is open and status message is set
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";

            var byteCollections = new List<List<byte>>();   // sets up the textbox array and textboxes itself
            var textBoxList = new List<TextBox>();

            // get all textboxes from input panel


            do
            {
                if (textBoxSENDTYPE?.Text == "Byte" || textBoxSENDTYPE?.Text == "Byte Collection")
                {
                    foreach (var control in textBoxesPanel!.Controls)
                    {
                        if (control is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            var inputValues = textBox.Text.Split(' ');
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

                                if (logging_check.Checked)
                                {
                                    var logFilePath = LogFilePath;

                                    using var logFile = new StreamWriter(logFilePath, true);
                                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT BYTE/BYTE COLLECTION]: {byteValue}");
                                }
                            }

                            mySerialPort.Write(bytesToSend.ToArray(), 0, bytesToSend.Count);
                            Thread.Sleep(currentDelayint);

                        }
                    }
                }

                if (textBoxreceiveType?.Text == "Byte" || textBoxreceiveType?.Text == "Byte Collection")
                {
                    if (mySerialPort.BytesToRead <= 0)
                    {
                        break;
                    }

                    var bytesReceived = new List<byte>();
                    var receivedText = new StringBuilder();

                    // read bytes from port and convert to text
                    while (mySerialPort.BytesToRead > 0)
                    {
                        var b = (byte)mySerialPort.ReadByte();
                        bytesReceived.Add(b);

                        if (textBoxreceiveType?.Text == "Byte Collection")
                        {
                            receivedText.Append(b + " ");
                        }
                        else if (textBoxreceiveType?.Text == "Byte")
                        {
                            receivedText.Append(b.ToString("X2") + " ");
                        }


                    }

                    // create a new textbox to display the received bytes
                    var receivedTextBox = new TextBox
                    {
                        Location = new Point(10, i * 20),
                        Width = 100,
                        ReadOnly = true,
                        Text = receivedText.ToString().Trim()
                    };
                    textBoxList.Add(receivedTextBox);
                    textBoxesPanel2?.Controls.Add(receivedTextBox);

                    if (logging_check.Checked)
                    {
                        var logFilePath = LogFilePath;

                        using var logFile = new StreamWriter(logFilePath, true);
                        logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE/BYTE COLLECTION]: {receivedText}");
                    }

                    i++;

                    if (i >= MAX_BUFFER_SIZE - 1)   // if buffer is hit, check if user wants overwrite
                    {
                        if (!(overwrite_check.Checked))
                        {
                            break;
                        }
                        else
                        {
                            i = -1;
                        }
                    }

                    if (!(repeat_check.Checked))
                    {
                        break;
                    }
                }

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
                ReadTimeout = int.Parse(textBoxRTIMEOUT!.Text),
                WriteTimeout = int.Parse(textBoxWTIMEOUT!.Text)
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

        private void Transmission_Click(object? sender, System.EventArgs e) // event to handle clicking the start transmission button
        {
            cancellationTokenSource = new CancellationTokenSource();    // sets up the stop transmission functionality
            textBoxesPanel2?.Controls.Clear();  // clears the output panel each time the button is clicked

            if (textBoxMODEDISP?.Text == "Receive Mode")
            {
                var modeSwitch = textBoxreceiveType?.Text;
                switch (modeSwitch)
                {

                    case "Byte":
                        TX_RX_BYTE();
                        break;

                    case "String":
                        TX_RX_String();
                        break;

                    case "Byte Collection":
                        TX_RX_BYTE();
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
                        TX_RX_BYTE();
                        break;

                    case "String":
                        TX_RX_String();
                        break;

                    case "Byte Collection":
                        TX_RX_BYTE();
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

        public void SetTextBox(TextBox textBox, Point location, int width, String text, Color backColor, Boolean read_only) { // sets up all textboxes with settings

            textBox.Location = location;
            textBox.Width = width;
            textBox.TextAlign = HorizontalAlignment.Center;
            textBox.Text = text;
            textBox.BackColor = backColor;
            textBox.ReadOnly = read_only;
            textBox.Cursor = Cursors.Arrow;
            this.Controls.Add(textBox);
        }

        public void SetCheckBoxes(CheckBox checkBox, Point location, String text)   // sets up all checkboxes with settings
        {      // sets the settings for the checkboxes

            checkBox.Width = 125;
            checkBox.Height = 20;
            checkBox.Font = new Font("Arial", 8);
            checkBox.Location = location;
            checkBox.Text = text;
            this.Controls.Add(checkBox);
        }

        public void SetButtons(Button button, Point location, String text, Color backColor)       // sets the settings for the buttons
        {
            button.Width = 155;
            button.Height = 40;
            button.AutoSize = true;
            button.DialogResult = DialogResult.OK;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#00457c");
            button.FlatAppearance.BorderSize = 2;
            button.Font = new Font("Arial", 8);
            button.Location = location;
            button.Text = text;
            button.BackColor = backColor;
            this.Controls.Add(button);
        }

        public void SetPanels(Panel panel, Point location)  // sets up all panels with settings
        {

            panel.Location = location;
            panel.BackColor = Color.LightBlue;
            panel.Size = new Size(125, 190);
            panel.AutoScroll = true;
            this.Controls.Add(panel);

        }

        void OnHelp()       // Launches to live external website for help/info
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd";
            psi.Arguments = $"/c start https://pipoat.github.io/";
            Process.Start(psi);
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

            // DEFINE THE MAIN OVERALL FORM

            existingPorts = AvailablePorts.ToList();
            Size = ClientSize;
            BackgroundImage = Image.FromFile("AT-SCC_GUI_Background.png");
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ControlBox = true;
            MaximizeBox = false;
            MinimizeBox = true;

            // DEFINE THE MENU BAR

            menuStrip = new MenuStrip();
            menuStrip.Parent = this;
            fileMenuItem = new ToolStripMenuItem("&File");
            comPortMenuItem = new ToolStripMenuItem("&Port Configurations");
            sendMenuItem = new ToolStripMenuItem("&Send/Receive Configurations");
            loggingMenuItem = new ToolStripMenuItem("&Log File");

            loggingMenuItem.DropDownItems.AddRange(new ToolStripItem[] { new ToolStripMenuItem("View Logs"), new ToolStripMenuItem("Delete Logs") });
            loggingMenuItem.DropDownItemClicked += OnLogChange;


            exitMenuItem = new ToolStripMenuItem("&Exit", null, (_, _) => Close()) { ShortcutKeys = Keys.Control | Keys.X };
            reloadMenuItem = new ToolStripMenuItem("&Reload", null, (_, _) => OnReload()) { ShortcutKeys = Keys.Control | Keys.L };
            var Help = new ToolStripMenuItem("&Info/Help", null, (_, _) => OnHelp()) { ShortcutKeys = Keys.Control | Keys.H };


            modeMenuItem = new ToolStripMenuItem("&Select Mode");
            modeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { new ToolStripMenuItem("Send Mode"), new ToolStripMenuItem("Receive Mode"), new ToolStripMenuItem("IDLE Mode") });
            modeMenuItem.DropDownItemClicked += (s, e) => textBoxMODEDISP!.Text = e.ClickedItem!.Text;

            portengageMenuItem = new ToolStripMenuItem("&Open/Close Port");
            portengageMenuItem.DropDownItems.Add("Open Port");
            portengageMenuItem.DropDownItems.Add("Close Port");
            portengageMenuItem.DropDownItemClicked += OnEngage!;


            fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { Help, modeMenuItem, loggingMenuItem, portengageMenuItem, reloadMenuItem, exitMenuItem });


            currentCom = new ToolStripMenuItem("&COM");
            foreach (var port in AvailablePorts)
            {
                currentCom.DropDownItems.Add(port);
            }
            currentPort = "COM#";
            currentCom.Text = currentPort;
            currentCom.DropDownItemClicked += (s, e) => textBoxCOM!.Text = currentPort = e.ClickedItem!.Text;
            currentCom.DropDownItemClicked += (s, e) => textBoxCOM!.BackColor = Color.LightGreen;


            baudMenuItem = new ToolStripMenuItem("&Baud");
            baudMenuItem.DropDownItems.AddRange(PossibleBauds.Select(b => new ToolStripMenuItem(b)).ToArray());
            baudMenuItem.DropDownItemClicked += (s, e) => textBoxBAUD!.Text = e.ClickedItem!.Text;
            baudMenuItem.DropDownItemClicked += (s, e) => currentBaudint = int.Parse(textBoxBAUD!.Text);
            baudMenuItem.DropDownItemClicked += (s, e) => textBoxBAUD!.BackColor = Color.LightGreen;


            parityMenuItem = new ToolStripMenuItem("&Parity");
            parityMenuItem.DropDownItemClicked += (s, e) => textBoxPARITY!.Text = e.ClickedItem!.Text;
            parityMenuItem.DropDownItemClicked += (s, e) => currentParityvalue = ParityOptions[textBoxPARITY!.Text];
            parityMenuItem.DropDownItemClicked += (s, e) => textBoxPARITY!.BackColor = Color.LightGreen;
            parityMenuItem.DropDownItems.AddRange(ParityOptions.Keys.Select(s => new ToolStripMenuItem(s)).ToArray());


            dataBitsMenuItem = new ToolStripMenuItem("&Data Bits");
            foreach (var item in Enumerable.Range(5, 4))
            {
                dataBitsMenuItem.DropDownItems.Add(item.ToString());
            }
            dataBitsMenuItem.DropDownItemClicked += (s, e) => textBoxDATABITS!.Text = e.ClickedItem!.Text;
            dataBitsMenuItem.DropDownItemClicked += (s, e) => currentdataBitsint = int.Parse(textBoxDATABITS!.Text);
            dataBitsMenuItem.DropDownItemClicked += (s, e) => textBoxDATABITS!.BackColor = Color.LightGreen; ;


            stopBitsMenuItem = new ToolStripMenuItem("&Stop Bits");
            stopBitsMenuItem.DropDownItemClicked += (s, e) => textBoxSTOPBITS!.Text = e.ClickedItem!.Text;
            stopBitsMenuItem.DropDownItemClicked += (s, e) => currentStopBitvalue = StopBitOptions[textBoxSTOPBITS!.Text];
            stopBitsMenuItem.DropDownItemClicked += (s, e) => textBoxSTOPBITS!.BackColor = Color.LightGreen; ;
            stopBitsMenuItem.DropDownItems.AddRange(StopBitOptions.Keys.Select(s => new ToolStripMenuItem(s)).ToArray());

            readTimeoutItem = new ToolStripMenuItem("&Read Timeout")
            {
                DropDownItems = { "-1", "500" }
            };
            readTimeoutItem.DropDownItemClicked += (s, e) => textBoxRTIMEOUT!.Text = e.ClickedItem!.Text;
            readTimeoutItem.DropDownItemClicked += (s, e) => textBoxRTIMEOUT!.BackColor = Color.LightGreen;


            writeTimeoutItem = new ToolStripMenuItem("&Write Timeout")
            {
                DropDownItems = { "-1", "500" }
            };
            writeTimeoutItem.DropDownItemClicked += (s, e) => textBoxWTIMEOUT!.Text = e.ClickedItem!.Text;
            writeTimeoutItem.DropDownItemClicked += (s, e) => textBoxWTIMEOUT!.BackColor = Color.LightGreen;


            handshakeItem = new ToolStripMenuItem("&Handshake");
            handshakeItem.DropDownItems.AddRange(HandShakeOptions.Keys.Select(key => new ToolStripMenuItem(key)).ToArray());
            handshakeItem.DropDownItemClicked += (s, e) => textBoxHANDSHAKE!.Text = e.ClickedItem!.Text;
            handshakeItem.DropDownItemClicked += (s, e) => currentHandshakevalue = HandShakeOptions[textBoxHANDSHAKE!.Text];
            handshakeItem.DropDownItemClicked += (s, e) => textBoxHANDSHAKE!.BackColor = Color.LightGreen;


            sendDelay = new ToolStripMenuItem("&Delay (ms)");
            sendDelay.DropDownItems.AddRange(sendDelayOptions.Select(sdelay => new ToolStripMenuItem(sdelay)).ToArray());
            sendDelay.DropDownItemClicked += (s, e) => textBoxDELAY!.Text = e.ClickedItem!.Text;
            sendDelay.DropDownItemClicked += (s, e) => currentDelayint = int.Parse(textBoxDELAY!.Text);
            sendDelay.DropDownItemClicked += (s, e) => textBoxDELAY!.BackColor = Color.LightGreen; ;


            sendingModeMenuItem = new ToolStripMenuItem("&Sending Data Type");
            sendingModeMenuItem.DropDownItems.AddRange(PossibleTransmitModes.Select(psm => new ToolStripMenuItem(psm)).ToArray());
            sendingModeMenuItem.DropDownItemClicked += (s, e) => textBoxSENDTYPE!.Text = e.ClickedItem?.Text ?? "N/A - DISABLED";


            receivingModeMenuItem = new ToolStripMenuItem("&Receiving Data Type");
            receivingModeMenuItem.DropDownItems.AddRange(PossibleTransmitModes.Select(prm => new ToolStripMenuItem(prm)).ToArray());
            receivingModeMenuItem.DropDownItemClicked += (s, e) => textBoxreceiveType!.Text = e.ClickedItem!.Text;


            comPortMenuItem.DropDownItems.AddRange(new ToolStripItem[] { currentCom, baudMenuItem, parityMenuItem, dataBitsMenuItem, stopBitsMenuItem, readTimeoutItem, writeTimeoutItem, handshakeItem, sendDelay });

            sendMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sendingModeMenuItem, receivingModeMenuItem });

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenuItem, comPortMenuItem, sendMenuItem });

            MainMenuStrip = menuStrip;

            // DEFINE CLOCK

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // Update time every 1 second
            timer.Tick += (_, _) => clock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();

            SetTextBox(clock, new Point(10, 365), 150, "", Color.LightGray, true);

            // DEFINE and DISPLAY transmit buttons

            STRANSMIT.Click += Transmission_Click;
            SetButtons(STRANSMIT, new Point(410, 280), "START TRANSMISSION", Color.LightGreen);

            ETRANSMIT.Click += (_, _) => cancellationTokenSource?.Cancel();
            SetButtons(ETRANSMIT, new Point(570, 280), "STOP TRANSMISSION", Color.Pink);


            // DEFINE INPUT AND OUTPUT PANELS

            AddLabel("TX (Sending):", new Point(410, 45), new Font("Arial", 10));
            SetPanels(textBoxesPanel, new Point(410, 65));

            AddLabel("RX (Receiving):", new Point(598, 45), new Font("Arial", 10));
            SetPanels(textBoxesPanel2, new Point(598, 65));

            // DEFINE LABELS, TEXTBOXES, BUTTONS FOR MAIN DISPLAY

            AddLabel("COM PORT:", new Point(20, 40), new Font("Arial", 8));
            SetTextBox(textBoxCOM, new Point(20, 60), 75, "", Color.Pink, true);
            

            AddLabel("BAUDRATE:", new Point(110, 40), new Font("Arial", 8));
            SetTextBox(textBoxBAUD, new Point(110, 60), 75, "", Color.Pink, true);


            AddLabel("PARITY:", new Point(20, 90), new Font("Arial", 8));
            SetTextBox(textBoxPARITY, new Point(20, 110), 75, "None", Color.LightYellow, true);


            AddLabel("DATABITS:", new Point(110, 90), new Font("Arial", 8));
            SetTextBox(textBoxDATABITS, new Point(110, 110), 75, "8", Color.LightYellow, true);


            AddLabel("STOPBITS:", new Point(20, 140), new Font("Arial", 8));
            SetTextBox(textBoxSTOPBITS, new Point(20, 160), 75, "1", Color.LightYellow, true);


            AddLabel("R T-OUT:", new Point(110, 140), new Font("Arial", 8));
            SetTextBox(textBoxRTIMEOUT, new Point(110, 160), 75, "-1", Color.LightYellow, true);


            AddLabel("W T-OUT:", new Point(20, 190), new Font("Arial", 8));
            SetTextBox(textBoxWTIMEOUT, new Point(20, 210), 75, "-1", Color.LightYellow, true);


            AddLabel("HANDSHAKE:", new Point(110, 190), new Font("Arial", 8));
            SetTextBox(textBoxHANDSHAKE, new Point(110, 210), 75, "None", Color.LightYellow, true);


            AddLabel("SEND DELAY (ms):", new Point(20, 240), new Font("Arial", 8));
            SetTextBox(textBoxDELAY, new Point(20, 260), 165, "1000", Color.LightYellow, false);
            this.textBoxDELAY.MaxLength = 4;


            AddLabel("MODE:", new Point(250, 40), new Font("Arial", 8));
            SetTextBox(textBoxMODEDISP, new Point(250, 60), 125, "Idle Mode", Color.LightGray, true);


            AddLabel("TX DATA TYPE:", new Point(250, 90), new Font("Arial", 8));
            SetTextBox(textBoxSENDTYPE, new Point(250, 110), 125, "N/A - DISABLED", Color.LightGray, true);


            AddLabel("RX DATA TYPE:", new Point(250, 140), new Font("Arial", 8));
            SetTextBox(textBoxreceiveType, new Point(250, 160), 125, "N/A - DISABLED", Color.LightGray, true);


            AddLabel("SET TX BUFER:", new Point(250, 190), new Font("Arial", 8));
            SetTextBox(textBoxBTT, new Point(250, 210), 125, "0", Color.LightBlue, false);
            this.textBoxBTT.MaxLength = 2;
            this.textBoxBTT.TextChanged += TextBoxBTT_TextChanged!; // Add event handler

            AddLabel("MAX RX BUFFER:", new Point(250, 240), new Font("Arial", 8));
            SetTextBox(textBoxBUFFER, new Point(250, 260), 125, Convert.ToString(MAX_BUFFER_SIZE), Color.LightYellow, true);
   
            // DEFINE INDICATOR TEXTBOX
            SetTextBox(textBoxSTATUS, new Point(250, 365), 125, "READY", Color.LightGray, true);

            // DEFINE CHECKBOXES
            SetCheckBoxes(repeat_check, new Point(10, 310), "Repeat Transmit");

            SetCheckBoxes(logging_check, new Point(140, 310), "Log Transmit");

            SetCheckBoxes(overwrite_check, new Point(270, 310), "Overwrite Rx");

        }
    }
}