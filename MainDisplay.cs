using System.IO.Ports;
using System.Text;
using System.Globalization;

namespace AT_SCC
{

    public partial class MainDisplay : Form
    {                                        
        
        private readonly DisplayHelp _displayHelp;
        
           // declaring all variables associated with textboxes, menu/tool strip, strings, or int
        private readonly TextBox? textBoxCOM = new(), textBoxBAUD = new(), textBoxPARITY = new(), textBoxDATABITS = new(), textBoxSTOPBITS = new(), textBoxRTIMEOUT = new(), textBoxWTIMEOUT = new(), textBoxHANDSHAKE = new(), textBoxDELAY = new(), textBoxBTT = new(), textBoxMODEDISP = new(), textBoxSENDTYPE = new(), textBoxreceiveType = new();

        private readonly TextBox? clock = new(), textBoxBUFFER = new(), textBoxSTATUS = new();   // sets up the textboxes

        private readonly Panel textBoxesPanel = new();
        private readonly Panel textBoxesPanel2 = new();


        private readonly MenuStrip? menuStrip;   // sets up the menu strip and its items

        private readonly ToolStripMenuItem? fileMenuItem, comPortMenuItem, sendMenuItem, loggingMenuItem, exitMenuItem, reloadMenuItem, modeMenuItem, currentCom, baudMenuItem, parityMenuItem, dataBitsMenuItem, stopBitsMenuItem, readTimeoutItem, writeTimeoutItem, handshakeItem, sendDelay, receivingModeMenuItem, sendingModeMenuItem;

        public CancellationTokenSource? cancellationTokenSource;   // declares the stop button functionality

        List<string>? existingPorts;

        public static string[] AvailablePorts => SerialPort.GetPortNames(); // provides all the ports available

        string currentPort = "";

        int currentBaudint, currentdataBitsint = 8, transmitactive = 0; // declare integers and strings for default values for settings of port

        Parity currentParityvalue = Parity.None;                      // declaring variables to allow usage of dictionaries
        StopBits currentStopBitvalue = StopBits.One;
        Handshake currentHandshakevalue = Handshake.None;

        readonly Button STRANSMIT = new();
        readonly Button ETRANSMIT = new();

        readonly CheckBox logging_check = new();
        readonly CheckBox repeat_check = new();
        readonly CheckBox overwrite_check = new(); // define the checkboxes

        public string LogFilePath = Path.GetFullPath("SerialLog.txt"); // PATH TO TEXT FILE

        public const int MAX_BUFFER_SIZE = 100;       // MAX BUFFER SIZE

        public SerialPort CreateSerialPort()      // CREATES the serial port
        {
            SerialPort mySerialPort = new(currentPort, currentBaudint, currentParityvalue, currentdataBitsint, currentStopBitvalue)
            {
                Handshake = currentHandshakevalue,
                ReadTimeout = int.Parse(textBoxRTIMEOUT!.Text),
                WriteTimeout = int.Parse(textBoxWTIMEOUT!.Text)
            };

            return mySerialPort;
        }

        // dictionaries/list of strings for user selection and input
        public readonly string[] PossibleBauds =
        [
            "75","110","134","150","300","600","1200","1800","2400","4800","7200","9600","14400","19200","38400","57600","115200","128000"
        ];

        public readonly string[] PossibleTransmitModes =
        [
            "N/A - DISABLED", "Byte/Byte Collection", "String", "ASCII", "ASCII-HEX"
        ];

        public readonly Dictionary<string, Parity> ParityOptions = new()
        {
            ["None"] = Parity.None,
            ["Even"] = Parity.Even,
            ["Odd"] = Parity.Odd,
            ["Mark"] = Parity.Mark,
            ["Space"] = Parity.Space
        };

        public readonly Dictionary<string, StopBits> StopBitOptions = new()
        {
            ["1"] = StopBits.One,
            ["1.5"] = StopBits.OnePointFive,
            ["2"] = StopBits.Two
        };

        public readonly Dictionary<string, Handshake> HandShakeOptions = new()
        {
            ["None"] = Handshake.None,
            ["XOnXOff"] = Handshake.XOnXOff,
            ["Send (Rq)"] = Handshake.RequestToSend,
            ["XOnXOff (Rq)"] = Handshake.RequestToSendXOnXOff
        };

        public readonly string[] sendDelayOptions =
        [
            "100","500","1000","2000","3000","4000","5000"
        ];

        // DECLARING EVENTS
        private void TextBoxBTT_TextChanged(object sender, EventArgs e)     // event to adjust the TX BUFFER SIZE
        {
            List<string> textBoxValues = [];

            if (!int.TryParse(textBoxBTT?.Text, out int btt) || btt < 0)    // if invalid input for TX buffer that does not exceed buffer
            {
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
                TextBox textBox = new() { Location = new Point(10, 10 + (i - 1) * 20) };
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
                        case "Byte/Byte Collection":
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

                else if (textBoxMODEDISP?.Text == "Send Mode" || textBoxMODEDISP?.Text == "Send and Receive")
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

                            case "Byte/Byte Collection":
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

                if (textBoxSENDTYPE?.Text == "String" && mySerialPort.IsOpen && (textBoxMODEDISP?.Text == "Send Mode" || textBoxMODEDISP?.Text == "Send and Receive"))
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

                if (textBoxreceiveType?.Text == "String" && (textBoxMODEDISP?.Text == "Receive Mode" || textBoxMODEDISP?.Text == "Send and Receive"))
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

                if (textBoxSENDTYPE?.Text == "Byte/Byte Collection" && mySerialPort.IsOpen && (textBoxMODEDISP?.Text == "Send Mode" || textBoxMODEDISP?.Text == "Send and Receive"))
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

                if (textBoxreceiveType?.Text == "Byte/Byte Collection" && (textBoxMODEDISP?.Text == "Receive Mode"  || textBoxMODEDISP?.Text == "Send and Receive"))
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

            byte[] hexBytes = []; // declare the variable as a byte array

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

        private static void ReceiveStringAsync(SerialPort mySerialPort, TextBox receivedTextBox, bool loggingEnabled, string logFilePath)    // task to receive strings
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
                receivedTextBox.Text = receivedTextBox.Text[(receivedTextBox.TextLength - receivedTextBox.Width * 3)..];
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
            receivedText.Append(b + " ");
            
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

        private void OnReload() // function to reload the ports/program

        {
            existingPorts = [.. AvailablePorts];
            currentCom?.DropDownItems.Clear();
            existingPorts.ForEach(port => currentCom?.DropDownItems.Add(port));

            TextBox? textBox = textBoxesPanel2?.Controls.OfType<TextBox>().FirstOrDefault();
            if (textBox != null) textBox.Text = "";
            textBoxesPanel2?.Controls.Clear();
        }

        public MainDisplay()  // main form with design components
        {
            InitializeComponent(); // calls the components fuction in the MainDisplay.Designer.cs file
            _displayHelp = new DisplayHelp();
            // DEFINE THE MAIN OVERALL FORM

            existingPorts = AvailablePorts.ToList();
            Size = ClientSize;
            BackgroundImage = Image.FromFile(Directory.GetCurrentDirectory() + "\\AT-SCC_GUI_Background.png");
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ControlBox = true;
            MaximizeBox = false;
            MinimizeBox = true;

            // DEFINE THE MENU BAR

            menuStrip = new MenuStrip{Parent = this};
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
            modeMenuItem.DropDownItems.AddRange(new ToolStripItem[] 
            { 
                new ToolStripMenuItem("Send Mode"), 
                new ToolStripMenuItem("Receive Mode"), 
                new ToolStripMenuItem("Send and Receive"),
                new ToolStripMenuItem("Idle Mode")
            });
            modeMenuItem.DropDownItemClicked += (s, e) => textBoxMODEDISP!.Text = e.ClickedItem!.Text;


            fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { Help, modeMenuItem, loggingMenuItem, reloadMenuItem, exitMenuItem });


            currentCom = new ToolStripMenuItem("&COM#");
            foreach (var port in AvailablePorts)
            {
                currentCom.DropDownItems.Add(port);
            }
            currentCom.DropDownItemClicked += (s, e) => textBoxCOM!.Text = currentPort = e.ClickedItem!.Text!;
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
            sendDelay.DropDownItemClicked += (s, e) => textBoxDELAY!.BackColor = Color.LightGreen;


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

            System.Windows.Forms.Timer timer = new(){ Interval = 1000 };
            timer.Tick += (_, _) => clock.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();

            _displayHelp.SetTextBox(clock, new Point(10, 365), 150, "", Color.LightGray, true, this);

            // DEFINE and DISPLAY transmit buttons

            _displayHelp.SetButtons(STRANSMIT, new Point(410, 280), "START TRANSMISSION", Color.LightGreen, new EventHandler(Transmission_Click), this);

            _displayHelp.SetButtons(ETRANSMIT, new Point(570, 280), "STOP TRANSMISSION", Color.Pink, (sender, e) => cancellationTokenSource?.Cancel(), this);

            // DEFINE INPUT AND OUTPUT PANELS

            _displayHelp.AddLabel("TX (Sending):", new Point(410, 45), new Font("Arial", 10), this);
            _displayHelp.SetPanels(textBoxesPanel, new Point(410, 65), this);

            _displayHelp.AddLabel("RX (Receiving):", new Point(568, 45), new Font("Arial", 10), this);
            _displayHelp.SetPanels(textBoxesPanel2, new Point(568, 65), this);

            // DEFINE LABELS, TEXTBOXES, BUTTONS FOR MAIN DISPLAY

            _displayHelp.AddLabel("COM PORT:", new Point(20, 40), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxCOM, new Point(20, 60), 75, "", Color.Pink, true, this);


            _displayHelp.AddLabel("BAUDRATE:", new Point(110, 40), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxBAUD, new Point(110, 60), 75, "", Color.Pink, true, this);


            _displayHelp.AddLabel("PARITY:", new Point(20, 90), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxPARITY, new Point(20, 110), 75, "None", Color.LightYellow, true, this);


            _displayHelp.AddLabel("DATABITS:", new Point(110, 90), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxDATABITS, new Point(110, 110), 75, "8", Color.LightYellow, true, this);


            _displayHelp.AddLabel("STOPBITS:", new Point(20, 140), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxSTOPBITS, new Point(20, 160), 75, "1", Color.LightYellow, true, this);


            _displayHelp.AddLabel("R T-OUT:", new Point(110, 140), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxRTIMEOUT, new Point(110, 160), 75, "-1", Color.LightYellow, true, this);


            _displayHelp.AddLabel("W T-OUT:", new Point(20, 190), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxWTIMEOUT, new Point(20, 210), 75, "-1", Color.LightYellow, true, this);


            _displayHelp.AddLabel("HANDSHAKE:", new Point(110, 190), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxHANDSHAKE, new Point(110, 210), 75, "None", Color.LightYellow, true, this);


            _displayHelp.AddLabel("DELAY (ms):", new Point(20, 240), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxDELAY, new Point(20, 260), 165, "1000", Color.LightYellow, true, this);


            _displayHelp.AddLabel("MODE:", new Point(250, 40), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxMODEDISP, new Point(250, 60), 125, "Idle Mode", Color.LightGray, true, this);


            _displayHelp.AddLabel("ENABLE TX?:", new Point(250, 90), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxSENDTYPE, new Point(250, 110), 125, "N/A - DISABLED", Color.LightGray, true, this);


            _displayHelp.AddLabel("ENABLE RX?:", new Point(250, 140), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxreceiveType, new Point(250, 160), 125, "N/A - DISABLED", Color.LightGray, true, this);


            _displayHelp.AddLabel("SET TX BUFER:", new Point(250, 190), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxBTT, new Point(250, 210), 125, "0", Color.LightBlue, false, this);
            textBoxBTT.MaxLength = 2;
            textBoxBTT.TextChanged += TextBoxBTT_TextChanged!; // Add event handler

            _displayHelp.AddLabel("MAX RX BUFFER:", new Point(250, 240), new Font("Arial", 8), this);
            _displayHelp.SetTextBox(textBoxBUFFER, new Point(250, 260), 125, Convert.ToString(MAX_BUFFER_SIZE), Color.LightYellow, true, this);

            // DEFINE INDICATOR TEXTBOX
            _displayHelp.SetTextBox(textBoxSTATUS, new Point(250, 365), 125, "READY", Color.LightGray, true, this);

            // DEFINE CHECKBOXES
            _displayHelp.SetCheckBoxes(repeat_check, new Point(10, 310), "Repeat Transmit", this);

            _displayHelp.SetCheckBoxes(logging_check, new Point(140, 310), "Log Transmit", this);

            _displayHelp.SetCheckBoxes(overwrite_check, new Point(270, 310), "Overwrite Rx", this);

        }
    }
}