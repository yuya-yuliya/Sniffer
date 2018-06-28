using Microsoft.Win32;
using Sniffer.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace Sniffer
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    List<PacketListener> addressesPacketListeners;
    public Filters filters;

    public ObservableCollection<Packet> list;
    bool analisysStart = false;

    Communication communication;

    int port = 11000;

    public MainWindow()
    {
      InitializeComponent();
      list = new ObservableCollection<Packet>();
      filters = new Filters();
      packetLV.ItemsSource = list;
      addressesPacketListeners = new List<PacketListener>();
      IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress ipAddr in ipHostInfo.AddressList)
      {
        if (ipAddr.AddressFamily == AddressFamily.InterNetwork)
        {
          addressesPacketListeners.Add(new PacketListener(ipAddr));
        }
      }
      Actions actions = new Actions();
      actions.OnReceiveFileRequest = OnReceiveFileRequest;
      actions.OnReceiveFile = OnReceiveFile;

      communication = new Communication(ipHostInfo.AddressList.LastOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork), port, actions);
      communication.Start();

      StopBtnEnables();
    }

    private Response OnReceiveFileRequest(string fileName, long fileSize, IPAddress remoteIP)
    {
      Response response;

      if (MessageBox.Show($"Receive file \"{fileName}\" (size {fileSize} bytes) from {remoteIP}?", "Incoming file", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
      {
        response = new Response("OK");
      }
      else
      {
        response = new Response("Cancel", "Remote user rejected request");
      }

      return response;
    }

    private void OnReceiveFile(string fileName, byte[] data)
    {
      try
      {
        Application.Current.Dispatcher.Invoke(delegate ()
        {
          SaveFileDialog saveFile = new SaveFileDialog();
          saveFile.FileName = fileName;
          if (saveFile.ShowDialog() == true)
          {
            File.WriteAllBytes(saveFile.FileName, data);
          }
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void startBtn_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (list.Count > 0)
        {
          ClearListQuestion();
        }
        startPacketListeners(addressesPacketListeners);
        StartBtnEnables();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void stopBtn_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        stopPacketListeners(addressesPacketListeners);
        StopBtnEnables();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void AddInPacketList(Packet packet)
    {
      Application.Current.Dispatcher.Invoke(delegate ()
                                  {
                                    if (filters.CheckPacket(packet))
                                    {
                                      list.Add(packet);
                                      countL.Content = list.Count;
                                      packetLV.ScrollIntoView(packetLV.Items[packetLV.Items.Count - 1]);
                                    }
                                  });
    }

    private void stopPacketListeners(IEnumerable<PacketListener> PacketListeners)
    {
      foreach (PacketListener PacketListener in PacketListeners)
      {
        PacketListener.End();
      }
      analisysStart = false;
    }

    private void startPacketListeners(IEnumerable<PacketListener> PacketListeners)
    {
      foreach (PacketListener PacketListener in PacketListeners)
      {
        PacketListener.Begin(AddInPacketList);
      }
      analisysStart = true;
    }

    private void ClearListQuestion()
    {
      if (MessageBox.Show("Clear packet list?", "Clear", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
      {
        list = new ObservableCollection<Packet>();
        packetLV.ItemsSource = list;
        headerTB.Text = "";
        infoTB.Text = "";
      }
    }

    private void restartBtn_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        ClearListQuestion();
        stopPacketListeners(addressesPacketListeners);
        startPacketListeners(addressesPacketListeners);
        StartBtnEnables();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void StartBtnEnables()
    {
      stopBtn.IsEnabled = true;
      restartBtn.IsEnabled = true;
      startBtn.IsEnabled = false;
      stopMI.IsEnabled = true;
      restartMI.IsEnabled = true;
      startMI.IsEnabled = false;
      statusL.Content = "Analysis in progress";
    }

    private void StopBtnEnables()
    {
      stopBtn.IsEnabled = false;
      stopMI.IsEnabled = false;
      startBtn.IsEnabled = true;
      startMI.IsEnabled = true;
      restartBtn.IsEnabled = false;
      restartMI.IsEnabled = false;
      statusL.Content = "Analysis was stopped";
    }

    private void packetLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (packetLV.SelectedItem != null)
      {
        Packet packet = (Packet)packetLV.SelectedItem;
        headerTB.Text = Info.GetBytesInString(packet.Header);
        infoTB.Text = Info.GetPacketInfo(packet);
      }
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
      if (!analisysStart)
      {
        SaveFileDialog saveFile = new SaveFileDialog();
        saveFile.FileName = DateTime.Now.ToString("dd-MM-yyyy");
        saveFile.Filter = "Trafic analyze file|*" + Serializing.Extension + "|All files|*.*";

        if (saveFile.ShowDialog() == true)
        {
          Serializing.Serialize(list, saveFile.FileName);
        }
      }
      else
      {
        MessageBox.Show("Stop analysis before");
      }
    }

    private void MenuItem_Click_1(object sender, RoutedEventArgs e)
    {
      if (!analisysStart)
      {
        try
        {
          OpenFileDialog openFile = new OpenFileDialog();
          openFile.Filter = "Trafic analyze file|*" + Serializing.Extension + "|All files|*.*";

          if (openFile.ShowDialog() == true)
          {
            List<Packet> openList = Serializing.Deserialize(openFile.FileName);
            list = new ObservableCollection<Packet>(openList);
            packetLV.ItemsSource = list;
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Can't load data\n" + ex.Message);
        }
      }
      else
      {
        MessageBox.Show("Stop analysis before");
      }
    }

    private void MenuItem_Click_2(object sender, RoutedEventArgs e)
    {
      if (!analisysStart)
      {
        SendFileWindow sendWindow = new SendFileWindow(port);
        sendWindow.Owner = this;
        sendWindow.Show();
        IsEnabled = false;
      }
      else
      {
        MessageBox.Show("Stop analysis before");
      }
    }

    private void receiveMI_Click(object sender, RoutedEventArgs e)
    {
      if (communication != null)
      {
        if (receiveMI.IsChecked)
        {
          communication.Start();
        }
        else
        {
          communication.Stop();
        }
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (MessageBox.Show("Close window?", "Exit", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
      {
        e.Cancel = true;
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      if (communication != null)
      {
        communication.Stop();
      }
      if (addressesPacketListeners != null)
      {
        stopPacketListeners(addressesPacketListeners);
      }
    }

    private void MenuItem_Click_3(object sender, RoutedEventArgs e)
    {
      if (!analisysStart)
      {
        FiltersWindow filtersWindow = new FiltersWindow(filters);
        filtersWindow.Owner = this;
        filtersWindow.ShowDialog();
      }
      else
      {
        MessageBox.Show("Stop analysis before");
      }
    }

    private void MenuItem_Click_4(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
