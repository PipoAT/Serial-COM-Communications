namespace AT_SCC;

public partial class Log : Form
{           // establishes the logging form and sets parent as main

    private MainDisplay MainDisplay;

    public Log(MainDisplay parentForm)
    {
        LogConfig();
        MainDisplay = parentForm;
    }

    private void LogConfig()
    {
        ClientSize = new Size(800, 500);
        Text = "Logs";
        RichTextBox LOGS = new() { Dock = DockStyle.Fill,ReadOnly = true};   // establishes the textbox in the form and sets the file to be the contents
        LOGS.LoadFile(Path.GetFullPath("SerialLog.txt"), RichTextBoxStreamType.PlainText);  // finds the file and opens up
        LOGS.Font = new Font("Times New Roman", 12);
        Controls.Add(LOGS);
    }
}