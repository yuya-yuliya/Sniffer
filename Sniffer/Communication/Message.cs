using System.Net.Sockets;
using System.Text;

namespace Sniffer
{
  /// <summary>
  /// Класс, предоставляющий методы для взаимодействия между узлами
  /// </summary>
  public class Message
  {
    private static string EndLn = "\r\n";

    /// <summary>
    /// Отправить сообщение
    /// </summary>
    /// <param name="socket">Сокет соединения</param>
    /// <param name="msg">Сообщение</param>
    public static void Send(Socket socket, string msg)
    {
      socket.Send(Encoding.UTF8.GetBytes(msg + EndLn));
    }

    /// <summary>
    /// Отправить сообщение и получить ответ
    /// </summary>
    /// <param name="socket">Сокет соединения</param>
    /// <param name="msg">Сообщение</param>
    /// <returns>Ответ удалённого узла</returns>
    public static string SendAndReceive(Socket socket, string msg)
    {
      Send(socket, msg);
      return Receive(socket);
    }

    /// <summary>
    /// Получить ответ удалённого узла
    /// </summary>
    /// <param name="socket">Сокет соединения</param>
    /// <returns>Ответ удалённого узла</returns>
    public static string Receive(Socket socket)
    {
      byte[] Buffer = new byte[1024];
      int CountBytes = 0;
      StringBuilder builder = new StringBuilder();

      do
      {
        CountBytes = socket.Receive(Buffer);
        builder.Append(Encoding.UTF8.GetString(Buffer, 0, CountBytes));
      }
      while (socket.Available > 0);

      return builder.ToString();
    }
  }
}
