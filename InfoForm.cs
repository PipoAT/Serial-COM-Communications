namespace AT_SCC;

public partial class Info : Form
{  // establishes the help form and sets main as parent

    private MainDisplay MainDisplay;

    public Info(MainDisplay parentForm)
    {

        InfoConfig();
        MainDisplay = parentForm;

    }

    private void InfoConfig()
    {
        this.ClientSize = new System.Drawing.Size(800, 950);
        this.Text = "Information/Help Guide";

        string info = "ATech Training Serial COM Communications (AT-SCC)\nv1.0.0\n\nDeveloped by Andrew Pipo\n\nINTERNAL USAGE ONLY";
        AddLabel(info, 0, 0);

        string sendInfo = "TO SEND:\nUnder 'File' -> 'Select Mode' -> 'Send Mode'\nUnder 'Port Configurations', select desired settings from drop down menu(s)\nUnder 'Send/Receive Configurations', select desired type of data to send\nWithin the light blue panel, type in desired data to send\nSelect the log checkbox if you want to log the transfer\nSelect Send and Receive Mode checkbox if you want to send and receive the data\nSelect the repeat checkbox if you want to send multiple times\nWhen ready, hit the GREEN 'TRANSMIT DATA' button";
        AddLabel(sendInfo, 0, 150);

        string recvInfo = "TO RECEIVE:\nUnder 'File' -> 'Select Mode' -> 'Receive Mode'\nUnder 'Port Configurations', select desired settings from drop down menu(s)\nUnder 'Send/Receive Configurations', select desired type of data to receive (ASCII/HEX will not work in this mode)\nWithin the light blue panel, type in desired data to send\nSelect the log checkbox if you want to log the transfer\nSelect the repeat checkbox if you want to receive multiple times\nWhen ready, hit the RED 'RECEIVE DATA' button";
        AddLabel(recvInfo, 0, 350);

        string logInfo = "TO VIEW/DELETE LOG:\nUnder 'File' -> 'Log File', select VIEW LOGS to open the log file, or select DELETE LOGS to delete the log file";
        AddLabel(logInfo, 0, 550);

        string ReloadInfo = "TO RELOAD PORTS/PROGRAM:\nUnder 'File' -> 'Reload'";
        AddLabel(ReloadInfo, 0, 600);

        // ADD MORE INSTRUCTIONS HERE
    }

    private void AddLabel(string text, int x, int y)
    {
        Label label = new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            Font = new Font("Times New Roman", 12),
            ForeColor = Color.Black
        };
        this.Controls.Add(label);
    }

}