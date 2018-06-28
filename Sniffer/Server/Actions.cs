using Sniffer.Commands;
using System;
using System.Net;

namespace Sniffer
{
  /// <summary>
  /// Класс для взаимодействия с приложением при получении запросов на сервере
  /// </summary>
  public class Actions
  {
    /// <summary>
    /// Взаимодействие при получении запроса на отправку файла
    /// </summary>
    public Func<string, long, IPAddress, Response> OnReceiveFileRequest;
    /// <summary>
    /// Взаимодействие при получении файла
    /// </summary>
    public Action<string, byte[]> OnReceiveFile;
  }
}
