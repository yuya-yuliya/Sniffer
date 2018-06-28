using Sniffer.Commands;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Sniffer
{
  /// <summary>
  /// Класс для работы с подключением к серверу
  /// </summary>
  public class Connection
  {
    private Actions actions;
    private Socket socket;
    private byte[] buffer;
    private int bufferSize = 1024;
    private string FileName;
    private long FileSize;
    private string FileHash;
    private Timer timer;
    private int timeout = 5 * 60 * 1000;

    /// <summary>
    /// Инициализация экземпляра класса
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="connectionActions">Обработчики полученных данных</param>
    public Connection(Socket socket, Actions connectionActions)
    {
      this.socket = socket;
      actions = connectionActions;
      //Создание таймера для автоматического отключения в случае долгого невзаимодействия
      timer = new Timer(new TimerCallback(DisconnectDueTimer), null, timeout, Timeout.Infinite);
    }

    /// <summary>
    /// Прослушивание подключения
    /// </summary>
    public void Listen()
    {
      try
      {
        buffer = new byte[bufferSize];
        while (socket != null)
        {
          StringBuilder builder = new StringBuilder();
          int count = 0;
          char lastChar = '\0';
          while (socket.Available == 0) { }
          do
          {
            count = socket.Receive(buffer);
            builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            lastChar = builder[builder.Length - 1];
          }
          while (lastChar != '\n');
          //Обработка полученных данных
          Process(builder.ToString());
          //Перезапуск таймера
          timer.Change(timeout, Timeout.Infinite);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Обработка полученного сообщения
    /// </summary>
    /// <param name="msg"></param>
    private void Process(string msg)
    {
      msg = msg.Trim(new char[] { '\n', '\r' });
      //Выделение слова-команды
      string request = Regex.Match(msg, @"^([a-z]+)\b", RegexOptions.IgnoreCase).Groups[1].Value;
      switch (request.ToUpper())
      {
        //Запрос на отправку данных на сервер
        case Request.Send:
          //Получение подробных данных о файле
          Match match = Regex.Match(msg, @"^SEND\s([\w.\-_]+)\s([0-9]+)\s(\w+)", RegexOptions.IgnoreCase);
          if (match.Length != 0)
          {
            FileSize = long.Parse(match.Groups[2].Value);
            FileName = match.Groups[1].Value;
            FileHash = match.Groups[3].Value;
            IPEndPoint remoteEP = socket.RemoteEndPoint as IPEndPoint;
            if (remoteEP != null)
            {
              //Обработка полученных данных
              if (actions.OnReceiveFileRequest != null)
              {
                Response response = actions.OnReceiveFileRequest(FileName, FileSize, remoteEP.Address);
                Message.Send(socket, response.Current + (response.Info == "" ? "" : (" " + response.Info)));
              }
              else
              {
                Message.Send(socket, Response.Ok);
              }
            }
          }
          else
          {
            SendError("Invalid command format");
          }
          break;
        // Пустая команда
        case Request.Noop:
          break;
        //команда отключения
        case Request.Quit:
          Message.Send(socket, Response.Ok);
          Disconnect();
          break;
        //Передача данных
        case Request.Data:
          //Проверка последовательности команд
          if (FileName != "")
          {
            msg = msg.TrimEnd(new char[] { '\r', '\n' });
            string fileDataBase64 = Regex.Match(msg, @"^DATA\s([a-z0-9=]+)", RegexOptions.IgnoreCase).Groups[1].Value;
            //Получение данных
            byte[] fileData = Base64.GetFileData(fileDataBase64);
            //Проверка хэш-кода полученных данных
            if (FileHash == Hash.GetHashSha1(fileData))
            {
              Message.Send(socket, Response.Ok);
              actions.OnReceiveFile?.BeginInvoke(FileName, fileData, null, null);
            }
            else
            {
              SendError("Hash codes does't match");
            }
          }
          else
          {
            SendError("Bad sequence of commands");
          }

          break;
        default:
          SendError("No such command");
          break;
      }
    }

    /// <summary>
    /// Отправка сообщения об ошибке
    /// </summary>
    /// <param name="msg"></param>
    private void SendError(string msg)
    {
      Message.Send(socket, Response.Error + " " + msg);
    }

    /// <summary>
    /// Отключение при срабатывании таймера
    /// </summary>
    /// <param name="obj"></param>
    private void DisconnectDueTimer(object obj)
    {
      Disconnect();
    }

    /// <summary>
    /// Отключение
    /// </summary>
    private void Disconnect()
    {
      try
      {
        timer.Change(Timeout.Infinite, Timeout.Infinite);
        socket.Close();
        socket = null;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }
}
