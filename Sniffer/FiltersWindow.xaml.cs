using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Sniffer
{
  /// <summary>
  /// Логика взаимодействия для FiltersWindow.xaml
  /// </summary>
  public partial class FiltersWindow : Window
  {
    List<CheckBox> protocolsCheckBoxes;

    public FiltersWindow(Filters filters)
    {
      InitializeComponent();
      protocolsCheckBoxes = new List<CheckBox>();
      foreach (var protocol in Enum.GetValues(typeof(Packet.Protocols)))
      {
        CheckBox checkBox = new CheckBox();
        checkBox.Content = ((Packet.Protocols)protocol);
        checkBox.Tag = (int)protocol;
        if (filters.Protocols.Contains((Packet.Protocols)protocol))
        {
          checkBox.IsChecked = true;
        }
        else
        {
          checkBox.IsChecked = false;
        }
        checkBox.Width = 100;
        protocolsCheckBoxes.Add(checkBox);
        protocolsWP.Children.Add(checkBox);
      }
      if (filters.Destination != null)
      {
        toTB.Text = filters.Destination.ToString();
      }
      if (filters.Source != null)
      {
        fromTB.Text = filters.Source.ToString();
      }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (Owner is MainWindow)
        {
          Filters filters = ((MainWindow)Owner).filters;
          foreach (var checkBox in protocolsCheckBoxes)
          {
            if (!(bool)checkBox.IsChecked)
            {
              filters.DeleteProtocol((Packet.Protocols)checkBox.Tag);
            }
            else
            {
              filters.AddProtocol((Packet.Protocols)checkBox.Tag);
            }
          }
          IPAddress temp = null;
          if (fromTB.Text != "")
          {
            if (!IPAddress.TryParse(fromTB.Text, out temp))
            {
              throw new Exception("Wrong \"from IP\" format");
            }
            fromTB.Text = temp.ToString();
          }
          filters.Source = temp;
          temp = null;
          if (toTB.Text != "")
          {
            if (!IPAddress.TryParse(toTB.Text, out temp))
            {
              throw new Exception("Wrong \"to IP\" format");
            }
            toTB.Text = temp.ToString();
          }
          filters.Destination = temp;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error");
      }
    }
  }
}
