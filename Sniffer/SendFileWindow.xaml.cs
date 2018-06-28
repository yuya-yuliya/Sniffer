using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Sniffer
{
  /// <summary>
  /// Логика взаимодействия для SendFileWindow.xaml
  /// </summary>
  public partial class SendFileWindow : Window
  {
    public int port;

    public SendFileWindow(int port)
    {
      InitializeComponent();
      this.port = port;
    }

    private void sendBtn_Click(object sender, RoutedEventArgs e)
    {
      IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
      IPAddress localIP = ipHostInfo.AddressList.LastOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
      SendInteraction sendInteraction = null;
      
      if ((bool)currentRB.IsChecked)
      {
        if (Owner is MainWindow)
        {
          sendInteraction = new SendInteraction(localIP, ((MainWindow)Owner).list, "taf");
        }
        else
        {
          throw new AggregateException("Send file window has incorrect owner");
        }
      }
      else if ((bool)chooseFileRB.IsChecked)
      {
        if (File.Exists(fileNameTB.Text))
        {
          sendInteraction = new SendInteraction(localIP, fileNameTB.Text);
        }
        else
        {
          MessageBox.Show("Choosen file doesn't exist", "Error");
        }
      }

      IPAddress IPAddr;
      if (!IPAddress.TryParse(reciverIPTB.Text, out IPAddr))
      {
        MessageBox.Show("Invalid reciver IP address", "Error");
        return;
      }
      if (sendInteraction != null)
      {
        try
        {
          if (sendInteraction.SendFile(IPAddr, port))
          {
            MessageBox.Show("File was send", "Success");
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message, "Error");
        }
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      if (Owner is MainWindow && !Owner.IsEnabled)
      {
        Owner.IsEnabled = true;
      }
    }

    private void chooseFileRB_Checked(object sender, RoutedEventArgs e)
    {
      if ((bool)chooseFileRB.IsChecked)
      {
        chooseFilePanel.IsEnabled = true;
      }
      else
      {
        chooseFilePanel.IsEnabled = false;
      }
    }

    private void chooseFileBtn_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog openFile = new OpenFileDialog();
      openFile.Filter = "Trafic analyze file (*.taf)|*.taf";
      if (openFile.ShowDialog() == true)
      {
        fileNameTB.Text = openFile.FileName;
      }
    }
  }
}
