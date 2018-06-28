using System;
using System.Net;
using System.Net.Sockets;

namespace Sniffer
{
  /// <summary>
  /// Класс для получения IP пакетов
  /// </summary>
  public class PacketListener
  {
    private object locker = new object();
    private Socket socket;
    private IPAddress ipAddress;
    private Action<Packet> afterReceive;
    private byte[] buffer;
    private int bufferSize = 65535;

    /// <summary>
    /// Инициализация экземпляра класса
    /// </summary>
    /// <param name="localIPAddress"></param>
    public PacketListener(IPAddress localIPAddress)
    {
      ipAddress = localIPAddress;
      buffer = new byte[bufferSize];
    }

    /// <summary>
    /// Запуск получения IP пакетов
    /// </summary>
    /// <param name="afterReceive">Действие при получении пакета</param>
    public void Begin(Action<Packet> afterReceive)
    {
      try
      {
        if (socket == null)
        {
          this.afterReceive = afterReceive;
          //Инициализация "сырого" сокета для получения всех входящих/исходящих IP акетов
          socket = new Socket(ipAddress.AddressFamily, SocketType.Raw, ProtocolType.IP);
          socket.Bind(new IPEndPoint(ipAddress, 0));
          socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), null);
          socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new AsyncCallback(Receive), null);
        }
        else
        {
          throw new AggregateException("Stop listening before");
        }

      }
      catch (Exception ex)
      { 
        throw ex;
      }
    }

    /// <summary>
    /// Обработка результатов асинхронной операции получения данных
    /// </summary>
    /// <param name="asyncResult"></param>
    private void Receive(IAsyncResult asyncResult)
    {
      try
      {
        lock (locker)
        {
          if (socket != null)
          {
            int count = socket.EndReceive(asyncResult);
            var packet = new Packet(buffer, count);
            afterReceive(packet);
            socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new AsyncCallback(Receive), null);
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Завершение получения IP пакетов
    /// </summary>
    public void End()
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

  }
}
