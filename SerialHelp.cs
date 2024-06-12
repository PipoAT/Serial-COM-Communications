using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace AT_SCC;

public class SerialHelp {

    // this class contains the functions and tasks for USART/UART/Serial Communications
    public async Task SendStringAsync(SerialPort mySerialPort, string textToSend, bool loggingEnabled, string logFilePath, TextBox textBox) // task to send strings
    {
        mySerialPort.WriteLine(textToSend);

        if (loggingEnabled)
        {
            using var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT STRING]: {textToSend}\n");
        }

        await Task.Delay(int.Parse(textBox!.Text));
    }

    public void ReceiveStringAsync(SerialPort mySerialPort, TextBox receivedTextBox, bool loggingEnabled, string logFilePath)    // task to receive strings
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

    public async Task SendASCIIAsync(byte[] hexBytes, SerialPort mySerialPort, CheckBox logging_check, TextBox textBox, string LogFilePath)     // task to send ASCII or hex values
    {
        foreach (var hexByte in hexBytes)
        {
            if (logging_check.Checked)
            {
                var logFilePath = LogFilePath;

                using var logFile = new StreamWriter(logFilePath, true);
                logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [SENT ASCII/HEX]: {hexByte:X2}");
            }

            mySerialPort.Write(new byte[] { hexByte }, 0, 1);

            await Task.Delay(int.Parse(textBox!.Text));
        }
    }

    public void ReceiveASCIIAsync(SerialPort mySerialPort, CheckBox logging_check, TextBox[] receivedTextBox, int i, string LogFilePath)
    {

        
        receivedTextBox[i].Text = mySerialPort.ReadExisting();

        if (logging_check.Checked)
        {
            var logFilePath = LogFilePath;

            using var logFile = new StreamWriter(logFilePath, true);
            logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: [RECEIVED ASCII/HEX]: {receivedTextBox[i]!.Text}");
        }
        
    }

    public async Task SendBytesAsync(SerialPort serialPort, IEnumerable<TextBox> textBoxes, CheckBox logging_check, string LogFilePath, TextBox textBoxDelay) // task to send bytes or byte collections
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
                await Task.Delay(int.Parse(textBoxDelay!.Text));
            }
        }
    }

    public void ReceiveBytesAsync(SerialPort mySerialPort, TextBox[]? textBoxArray, int i, CheckBox logging_check, string LogFilePath)     // task to receive bytes or byte collections
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



}