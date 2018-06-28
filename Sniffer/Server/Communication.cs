using System;
using System.Net;
using System.Net.Sockets;

namespace Sniffer
{
  /// <summary>
  /// Класс для организации работы сервера
  /// </summary>
  public class Communication
  {
    private Socket socket;
    private IPAddress bindIP;
    private int bindPort;
    private object locker = new object();
    private Actions connectionActions;

    /// <summary>
    /// Инициализация экзкмпляра класса
    /// </summary>
    /// <param name="localIP"></param>
    /// <param name="port"></param>
    /// <param name="connectionActions">Обработчики полученных данных</param>
    public Communication(IPAddress localIP, int port, Actions connectionActions)
    {
      bindIP = localIP;
      bindPort = port;
      this.connectionActions = connectionActions;
    }

    /// <summary>
    /// Запуск работы сервера
    /// </summary>
    public void Start()
    {
      if (socket == null)
      {
        socket = new Socket(bindIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(bindIP, bindPort));
        socket.Listen(10);
        socket.BeginAccept(new AsyncCallback(IncomingConnection), null);
      }
      else
      {
        throw new AggregateException("Stop communication socket first");
      }
    }

    /// <summary>
    /// Завершение работы сервера
    /// </summary>
    public void Stop()
    {
      try
      {
        lock (locker)
        {
          if (socket != null)
          {
            socket.Close();
            socket = null;
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Обработка асинхронной операции входящего подключения к серверу
    /// </summary>
    /// <param name="ar"></param>
    private void IncomingConnection(IAsyncResult ar)
    {
      try
      {
        lock (locker)
        {
          if (socket != null)
          {
            Socket handle = socket.EndAccept(ar);
            socket.BeginAccept(new AsyncCallback(IncomingConnection), null);
            //Создание нового подключения
            var connection = new Connection(handle, connectionActions);
            connection.Listen();
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }
}
