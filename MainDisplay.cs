using System.IO.Ports;
using System.Text;
using System.Globalization;

namespace AT_SCC
{

    public partial class MainDisplay : Form
    {                                           // declaring all variables associated with textboxes, menu/tool strip, strings, or int
        private TextBox? textBoxCOM = new TextBox(), textBoxBAUD = new TextBox(), textBoxPARITY = new TextBox(), textBoxDATABITS = new TextBox(), textBoxSTOPBITS = new TextBox(), textBoxRTIMEOUT = new TextBox(), textBoxWTIMEOUT = new TextBox(), textBoxHANDSHAKE = new TextBox(), textBoxDELAY = new TextBox(), textBoxBTT = new TextBox(), textBoxMODEDISP = new TextBox(), textBoxSENDTYPE = new TextBox(), textBoxreceiveType = new TextBox();

        private TextBox? clock = new TextBox(), textBoxBUFFER = new TextBox(), textBoxSTATUS = new TextBox();   // sets up the textboxes

        private Panel textBoxesPanel = new Panel();
        private Panel textBoxesPanel2 = new Panel();


        private MenuStrip? menuStrip;   // sets up the menu strip and its items

        private ToolStripMenuItem? fileMenuItem, comPortMenuItem, sendMenuItem, loggingMenuItem, exitMenuItem, reloadMenuItem, modeMenuItem, currentCom, baudMenuItem, parityMenuItem, dataBitsMenuItem, stopBitsMenuItem, readTimeoutItem, writeTimeoutItem, handshakeItem, sendDelay, receivingModeMenuItem, sendingModeMenuItem;

        public CancellationTokenSource? cancellationTokenSource;   // declares the stop button functionality

        List<string>? existingPorts;

        public static string[] AvailablePorts => System.IO.Ports.SerialPort.GetPortNames(); // provides all the ports available

        string currentPort = "";

        int currentBaudint, currentdataBitsint = 8, transmitactive = 0; // declare integers and strings for default values for settings of port

        Parity currentParityvalue = Parity.None;                      // declaring variables to allow usage of dictionaries
        StopBits currentStopBitvalue = StopBits.One;
        Handshake currentHandshakevalue = Handshake.None;

        Button STRANSMIT = new Button();
        Button ETRANSMIT = new Button();

        CheckBox logging_check = new CheckBox();
        CheckBox repeat_check = new CheckBox();
        CheckBox overwrite_check = new CheckBox(); // define the checkboxes

        public string LogFilePath = Path.GetFullPath("SerialLog.txt"); // PATH TO TEXT FILE

        public const int MAX_BUFFER_SIZE = 100;       // MAX BUFFER SIZE

        public SerialPort CreateSerialPort()      // CREATES the serial port
        {
            SerialPort mySerialPort = new SerialPort(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
            {
                Handshake = currentHandshakevalue,
                ReadTimeout = int.Parse(textBoxRTIMEOUT!.Text),
                WriteTimeout = int.Parse(textBoxWTIMEOUT!.Text)
            };

            return mySerialPort;
        }

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
                TextBox textBox = new TextBox() { Location = new Point(10, 10 + (i - 1) * 20) };
                if (i <= textBoxValues.Count) textBox.Text = textBoxValues[i - 1];
                textBox.Width = 120;
                this.textBoxesPanel?.Controls.Add(textBox);
            });
        }

        private void Transmission_Click(object? sender, System.EventArgs e) // event to handle clicking the start transmission button
        {
            if (transmitactive == 1)
            {
                MessageBox.Show("Transmission In Progress. Please stop trasmission before starting another one.", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                
                textBoxesPanel2?.Controls.Clear();  // clears the output panel each time the button is clicked

                if (textBoxMODEDISP?.Text == "Receive Mode")
                {
                    var modeSwitch = textBoxreceiveType?.Text;
                    switch (modeSwitch)
                    {

                        case "Byte":
                        case "Byte Collection":
                            TX_RX_BYTE();
                            break;

                        case "String":
                            TX_RX_String();
                            break;

                        default:
                            MessageBox.Show("Misconfigured Settings. Please check settings and modify as needed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }

                else if (textBoxMODEDISP?.Text == "Send Mode")
                {
                    if ((textBoxSENDTYPE?.Text != textBoxreceiveType?.Text) && (textBoxreceiveType?.Text != "N/A - DISABLED"))
                    {
                        MessageBox.Show("Misconfigured Settings. Please check settings and modify as needed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    else
                    {

                        var modeSwitchS = textBoxSENDTYPE?.Text;
                        switch (modeSwitchS)
                        {

                            case "Byte":
                            case "Byte Collection":
                                TX_RX_BYTE();
                                break;

                            case "String":
                                TX_RX_String();
                                break;

                            case "ASCII":
                            case "ASCII-HEX":
                                TX_RX_ASCII();
                                break;

                            default:
                                MessageBox.Show("Misconfigured Settings. Please check settings and modify as needed.", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                break;

                        }
                    }

                }

            }
        }

        // DECLARE FUNCTIONS and TASKS
        private async void TX_RX_String() // sends and/or reads strings
        {
            transmitactive = 1;
            cancellationTokenSource = new CancellationTokenSource();
            int i = 0;
            var mySerialPort = CreateSerialPort();

            if (!mySerialPort.IsOpen) mySerialPort.Open();  // checks if port is open and if not opens it

            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";    // status message

            var textBoxArray = new TextBox[MAX_BUFFER_SIZE];    // sets up array for textboxes and creates them
            for (var k = 0; k < textBoxArray.Length; k++)
            {
                textBoxArray[k] = new TextBox
                {
                    Location = new Point(10, k * 20),
                    Width = 120,
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
                            await SendStringAsync(mySerialPort, textToSend, logging_check.Checked, LogFilePath);
                        }
                    }
                }

                if (textBoxreceiveType?.Text == "String")
                {
                    var receivedTextBox = textBoxArray[i];
                    ReceiveStringAsync(mySerialPort, receivedTextBox, logging_check.Checked, LogFilePath); // receives the data and sets to output panel
                    i++;

                }
                await Task.Delay(int.Parse(textBoxDELAY!.Text));
            } while (repeat_check.Checked && !cancellationTokenSource!.IsCancellationRequested && mySerialPort.IsOpen);

            transmitactive = 0;
            mySerialPort.Close(); // auto closes the port
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

        }

        private async void TX_RX_BYTE() // sends and/or reads strings
        {
            transmitactive = 1;
            cancellationTokenSource = new CancellationTokenSource();
            int i = 0;
            var mySerialPort = CreateSerialPort();

            if (!mySerialPort.IsOpen) mySerialPort.Open();  // checks if port is open and if not opens it

            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";    // status message

            var textBoxArray = new TextBox[MAX_BUFFER_SIZE];    // sets up array for textboxes and creates them
            for (var k = 0; k < textBoxArray.Length; k++)
            {
                textBoxArray[k] = new TextBox
                {
                    Location = new Point(10, k * 20),
                    Width = 120,
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

                if ((textBoxSENDTYPE?.Text == "Byte" || textBoxSENDTYPE?.Text == "Byte Collection") && mySerialPort.IsOpen)
                {
                    foreach (var control in textBoxesPanel!.Controls)
                    {
                        if (control is TextBox textBox)
                        {
                            // Send the textbox contents as a string
                            var textToSend = textBox.Text;
                            await SendBytesAsync(mySerialPort, textBoxSENDTYPE.Text, textBoxesPanel.Controls.OfType<TextBox>());
                        }
                    }
                }

                if (textBoxreceiveType?.Text == "Byte" || textBoxreceiveType?.Text == "Byte Collection")
                {

                    ReceiveBytesAsync(mySerialPort, textBoxArray, i); // receives the data and sets to output panel
                    i++;

                }
                await Task.Delay(int.Parse(textBoxDELAY!.Text));
            } while (repeat_check.Checked && !cancellationTokenSource!.IsCancellationRequested && mySerialPort.IsOpen);

            transmitactive = 0;
            mySerialPort.Close(); // auto closes the port
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";

        }

        private async void TX_RX_ASCII() // Sends and/or receives ASCII or ASCII Hex values
        {
            transmitactive = 1;
            cancellationTokenSource = new CancellationTokenSource();
            var inputValues = new List<string>();       // sets up the textboxes
            foreach (var textBox in textBoxesPanel!.Controls.OfType<TextBox>())
            {
                if (!string.IsNullOrEmpty(textBox.Text)) inputValues.AddRange(textBox.Text.Split(' '));
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

            var mySerialPort = CreateSerialPort();

            if (!mySerialPort.IsOpen) mySerialPort.Open();  // checks for if port is open and sets status message
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT OPEN";
            int textBoxLocationY = textBoxesPanel2!.Controls.Count * 20;    // sets up textbox y location
            int i = 0;

            var textBoxArray = new TextBox[MAX_BUFFER_SIZE];    // sets up array for textboxes and creates them
            for (var k = 0; k < textBoxArray.Length; k++)
            {
                textBoxArray[k] = new TextBox
                {
                    Location = new Point(10, k * 20),
                    Width = 120,
                    ReadOnly = true
                };
                textBoxesPanel2?.Controls.Add(textBoxArray[k]);
            }

            do
            {

                // Send data and check if user needs logging
                foreach (var hexByte in hexBytes)
                {
                    await SendASCIIAsync(hexBytes, mySerialPort, logging_check);

                    // If also receiving, receive what is sent and output to the panel
                    if (textBoxreceiveType?.Text == "ASCII" || textBoxreceiveType?.Text == "ASCII-HEX")
                    {
                        ReceiveASCIIAsync(mySerialPort, textBoxesPanel2!, logging_check, textBoxLocationY, textBoxArray, i);
                    }
                }

                if (i >= MAX_BUFFER_SIZE)
                {
                    i = 0;

                    if (!overwrite_check.Checked) break;

                    else textBoxesPanel2!.Controls.Clear();

                }
                textBoxLocationY += 20;
                i++;


            } while (!cancellationTokenSource!.IsCancellationRequested && repeat_check.Checked);

            transmitactive = 0;
            mySerialPort.Close();   // auto close the port
            if (textBoxSTATUS != null) textBoxSTATUS.Text = "PORT CLOSED";
        }

        private async Task SendStringAsync(SerialPort mySerialPort, string textToSend, bool loggingEnabled, string logFilePath) // task to send strings
        {
            mySerialPort.WriteLine(textToSend);

            if (loggingEnabled)
            {
                using var logFile = new StreamWriter(logFilePath, true);
                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT STRING]: {textToSend}\n");
            }

            await Task.Delay(int.Parse(textBoxDELAY!.Text));
        }

        private void ReceiveStringAsync(SerialPort mySerialPort, TextBox receivedTextBox, bool loggingEnabled, string logFilePath)    // task to receive strings
        {
            if (mySerialPort.BytesToRead > 0) {
            receivedTextBox.Text = mySerialPort.ReadLine();
            if (loggingEnabled)
            {
                using var logFile = new StreamWriter(logFilePath, true);
                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED STRING]: {receivedTextBox.Text}\n");
            }

            if (receivedTextBox.TextLength >= receivedTextBox.Width * 3)
            {
                receivedTextBox.Text = receivedTextBox.Text.Substring(receivedTextBox.TextLength - receivedTextBox.Width * 3);
            }

            }
            
        }

        private async Task SendBytesAsync(SerialPort serialPort, string textBoxType, IEnumerable<TextBox> textBoxes) // task to send bytes or byte collections
        {
            foreach (var textBox in textBoxes)
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
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

                    serialPort.Write(bytesToSend.ToArray(), 0, bytesToSend.Count);
                    await Task.Delay(int.Parse(textBoxDELAY!.Text));
                }
            }
        }

        private void ReceiveBytesAsync(SerialPort mySerialPort, TextBox[]? textBoxArray, int i)     // task to receive bytes or byte collections
        {
            if (mySerialPort.BytesToRead > 0) {
            var bytesReceived = new List<byte>();
            var receivedText = new StringBuilder();
            var receivedTextBox = textBoxArray![i];

            // read bytes from port and convert to text
            
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


            
            receivedTextBox.Text = Convert.ToString(receivedText);


            // create a new textbox to display the received bytes

            if (logging_check.Checked)
            {
                var logFilePath = LogFilePath;

                using var logFile = new StreamWriter(logFilePath, true);
                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED BYTE/BYTE COLLECTION]: {receivedText}");
            }

            }
        }

        private async Task SendASCIIAsync(byte[] hexBytes, SerialPort mySerialPort, CheckBox logging_check)     // task to send ASCII or hex values
        {
            foreach (var hexByte in hexBytes)
            {
                if (logging_check.Checked)
                {
                    var logFilePath = LogFilePath;

                    using var logFile = new StreamWriter(logFilePath, true);
                    logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT ASCII/HEX]: {hexByte.ToString("X2")}");
                }

                mySerialPort.Write(new byte[] { hexByte }, 0, 1);

                await Task.Delay(int.Parse(textBoxDELAY!.Text));
            }
        }

        private void ReceiveASCIIAsync(SerialPort mySerialPort, Panel textBoxesPanel2, CheckBox logging_check, int textBoxLocationY, TextBox[] receivedTextBox, int i)
        {

           
            receivedTextBox[i].Text = mySerialPort.ReadExisting();

            if (logging_check.Checked)
            {
                var logFilePath = LogFilePath;

                using var logFile = new StreamWriter(logFilePath, true);
                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED ASCII/HEX]: {receivedTextBox[i]!.Text}");
            }
            
        }
        // task to receive ASCII or hex values

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

        public void SetTextBox(TextBox textBox, Point location, int width, String text, Color backColor, Boolean read_only)
        { // sets up all textboxes with settings

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

        public void SetButtons(Button button, Point location, String text, Color backColor, EventHandler eventHandler)       // sets the settings for the buttons
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
            button.Click += eventHandler;
            this.Controls.Add(button);
        }

        public void SetPanels(Panel panel, Point location)  // sets up all panels with settings
        {
            panel.Location = location;
            panel.BackColor = Color.LightBlue;
            panel.Size = new Size(155, 190);
            panel.AutoScroll = true;
            this.Controls.Add(panel);
        }

        private void OnReload() // function to reload the ports/program

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
            BackgroundImage = Image.FromFile(Directory.GetCurrentDirectory() + "\\AT-SCC_GUI_Background.png");
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

            // Set up the logging menu item with lambda expressions
            loggingMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("View Logs", null, (s, e) => new Log(this).Show()),
                new ToolStripMenuItem("Delete Logs", null, (s, e) =>
                {
                    System.IO.File.WriteAllText(LogFilePath, string.Empty);
                    MessageBox.Show("Logs have been erased", "ATTENTION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                })
            });



            exitMenuItem = new ToolStripMenuItem("&Exit", null, (_, _) => Close()) { ShortcutKeys = Keys.Control | Keys.X };
            reloadMenuItem = new ToolStripMenuItem("&Reload", null, (_, _) => OnReload()) { ShortcutKeys = Keys.Control | Keys.L };
            var Help = new ToolStripMenuItem("&Info/Help", null, (_, _) => new Info(this).Show()) { ShortcutKeys = Keys.Control | Keys.H };



            modeMenuItem = new ToolStripMenuItem("&Select Mode");
            modeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { new ToolStripMenuItem("Send Mode"), new ToolStripMenuItem("Receive Mode"), new ToolStripMenuItem("Idle Mode") });
            modeMenuItem.DropDownItemClicked += (s, e) => textBoxMODEDISP!.Text = e.ClickedItem!.Text;


            fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { Help, modeMenuItem, loggingMenuItem, reloadMenuItem, exitMenuItem });


            currentCom = new ToolStripMenuItem("&COM#");
            foreach (var port in AvailablePorts)
            {
                currentCom.DropDownItems.Add(port);
            }
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
            sendDelay.DropDownItemClicked += (s, e) => textBoxDELAY!.BackColor = Color.LightGreen; ;


            sendingModeMenuItem = new ToolStripMenuItem("&TX Data Type");
            sendingModeMenuItem.DropDownItems.AddRange(PossibleTransmitModes.Select(psm => new ToolStripMenuItem(psm)).ToArray());
            sendingModeMenuItem.DropDownItemClicked += (s, e) => textBoxSENDTYPE!.Text = e.ClickedItem?.Text ?? "N/A - DISABLED";


            receivingModeMenuItem = new ToolStripMenuItem("&RX Data Type");
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

            SetButtons(STRANSMIT, new Point(410, 280), "START TRANSMISSION", Color.LightGreen, new EventHandler(Transmission_Click));

            SetButtons(ETRANSMIT, new Point(570, 280), "STOP TRANSMISSION", Color.Pink, (sender, e) => cancellationTokenSource?.Cancel());

            // DEFINE INPUT AND OUTPUT PANELS

            AddLabel("TX (Sending):", new Point(410, 45), new Font("Arial", 10));
            SetPanels(textBoxesPanel, new Point(410, 65));

            AddLabel("RX (Receiving):", new Point(568, 45), new Font("Arial", 10));
            SetPanels(textBoxesPanel2, new Point(568, 65));

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


            AddLabel("DELAY (ms):", new Point(20, 240), new Font("Arial", 8));
            SetTextBox(textBoxDELAY, new Point(20, 260), 165, "1000", Color.LightYellow, true);


            AddLabel("MODE:", new Point(250, 40), new Font("Arial", 8));
            SetTextBox(textBoxMODEDISP, new Point(250, 60), 125, "Idle Mode", Color.LightGray, true);


            AddLabel("ENABLE TX?:", new Point(250, 90), new Font("Arial", 8));
            SetTextBox(textBoxSENDTYPE, new Point(250, 110), 125, "N/A - DISABLED", Color.LightGray, true);


            AddLabel("ENABLE RX?:", new Point(250, 140), new Font("Arial", 8));
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