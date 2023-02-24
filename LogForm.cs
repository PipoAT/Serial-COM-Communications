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

        this.ClientSize = new System.Drawing.Size(800, 500);
        this.Text = "Logs";

        RichTextBox LOGS = new RichTextBox();
        LOGS.Dock = DockStyle.Fill;
        LOGS.ReadOnly = true;

        LOGS.LoadFile("T:\\APIPO\\Dev\\C#\\AT-SCC\\SerialLog.txt", RichTextBoxStreamType.PlainText); // FIX THIS
        LOGS.Font = new Font("Times New Roman", 12);

        this.Controls.Add(LOGS);

    }


}


