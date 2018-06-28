using Sniffer.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Sniffer
{
  /// <summary>
  /// Класс для взаимодействия с сервером для передачи на него файл
  /// </summary>
  public class SendInteraction
  {
    private Socket socket;
    private IPAddress localIP;
    private IPAddress remoteIP;
    private int port;
    private string _tempFileName;
    private string tempFileName
    {
      get
      {
        int ind = 0;
        string filePath = _tempFileName;
        if ((ind = filePath.LastIndexOf('\\')) != -1 || (ind = filePath.LastIndexOf('/')) != -1)
        {
          filePath = filePath.Substring(ind + 1);
        }
        return filePath; ;
      }
      set
      {
        _tempFileName = value;
      }
    }
    private string fileHash;
    private bool IsTemp = false;

    /// <summary>
    /// Инициализация экземпляра класса
    /// </summary>
    /// <param name="localIP">Локальный IP адрес</param>
    /// <param name="packets">Перечисление пакетов</param>
    /// <param name="extension">Расширение файла для сохранения</param>
    public SendInteraction(IPAddress localIP, IEnumerable<Packet> packets, string extension)
    {
      this.localIP = localIP;
      tempFileName = SaveTempFile(packets, extension);
      fileHash = Hash.GetHashSha1(File.ReadAllBytes(_tempFileName));
    }

    /// <summary>
    /// Инициализация экземпляра класса
    /// </summary>
    /// <param name="localIP">Локальный IP адрес</param>
    /// <param name="fileName">Путь файла для передачи</param>
    public SendInteraction(IPAddress localIP, string fileName)
    {
      this.localIP = localIP;
      tempFileName = fileName;
      fileHash = Hash.GetHashSha1(File.ReadAllBytes(fileName));
    }

    /// <summary>
    /// Сохранение данных во временный файл
    /// </summary>
    /// <param name="packets">Преречисление пакетов</param>
    /// <param name="extension">Расширение файла</param>
    /// <returns></returns>
    private string SaveTempFile(IEnumerable<Packet> packets, string extension)
    {
      IsTemp = true;
      string path = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("dd-MM-yyyy") + "." + extension);
      Serializing.Serialize(packets, path);
      return path;
    }

    /// <summary>
    /// Отправка файла на сервер
    /// </summary>
    /// <param name="remoteIP">Удалённый IP адрес</param>
    /// <param name="port">Порт сервера</param>
    /// <returns></returns>
    public bool SendFile(IPAddress remoteIP, int port)
    {
      try
      {
        this.remoteIP = remoteIP;
        this.port = port;
        if (socket == null)
        {
          socket = new Socket(localIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
          socket.Bind(new IPEndPoint(localIP, 0));
        }
        socket.Connect(new IPEndPoint(remoteIP, port));

        //Отправка запроса на отправку файла на сервер
        string msg = Message.SendAndReceive(socket, Request.Send + $" {tempFileName} {new FileInfo(_tempFileName).Length} {fileHash}");
        //Оценка ответа сервера
        if (CheckResponse(ParseMsg(msg)))
        {
          //Отправка файла на сервер
          msg = Message.SendAndReceive(socket, Request.Data + " " + Base64.GetBase64FromFile(_tempFileName));
          CloseConnection();
          if (!CheckResponse(ParseMsg(msg)))
          {
            return false;
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        CloseConnection();
        throw ex;
      }
    }

    /// <summary>
    /// Закрытие соединения с сервером
    /// </summary>
    private void CloseConnection()
    {
      Message.SendAndReceive(socket, Request.Quit);
      Disconnect(socket);
      if (IsTemp)
      {
        File.Delete(_tempFileName);
      }
    }

    /// <summary>
    /// Оценка ответа сервера
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private bool CheckResponse(Response response)
    {
      switch (response.Current)
      {
        case Response.Ok:
          return true;
        case Response.Cancel:
        case Response.Error:
          throw new Exception(response.Info);
        default:
          return false;
      }
    }

    /// <summary>
    /// Разбор сообщения
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private Response ParseMsg(string msg)
    {
      msg = msg.Trim(new char[] { '\r', '\n' });
      //Выделение слова-команды
      string response = Regex.Match(msg, @"^([a-z]+)\b", RegexOptions.IgnoreCase).Groups[1].Value;
      string info = "";
      if (response != "")
      {
        switch (response)
        {
          case Response.Ok:
            break;
          case Response.Cancel:
          case Response.Error:
            //Выделение информации об ошибке
            info = Regex.Match(msg, @"^[a-z]+\s(.+)$", RegexOptions.IgnoreCase).Groups[1].Value;
            break;
          default:
            throw new ArgumentException("Invalid responce");
        }
        return new Response(response, info);
      }
      else
      {
        throw new ArgumentException("Invalid response");
      }
    }

    /// <summary>
    /// Отключение сокета, используемого для связи с сервером
    /// </summary>
    /// <param name="socket"></param>
    private void Disconnect(Socket socket)
    {
      try
      {
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
