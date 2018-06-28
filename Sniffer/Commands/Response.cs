using System;

namespace Sniffer.Commands
{
  /// <summary>
  /// Класс, представляющий возможные ответы сервера
  /// </summary>
  public class Response
  {
    /// <summary>
    /// Подтверждение выполнения команды
    /// </summary>
    public const string Ok = "OK";
    /// <summary>
    /// Отмена выполнения команды
    /// </summary>
    public const string Cancel = "CANCEL";
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public const string Error = "ERROR";

    /// <summary>
    /// Ответ сервера
    /// </summary>
    public readonly string Current;
    /// <summary>
    /// Дополнительная информация об ответе
    /// </summary>
    public readonly string Info;
    /// <summary>
    /// Инициализация экземпляра класса
    /// </summary>
    /// <param name="responce">Ответ сервера</param>
    /// <param name="info">Информация</param>
    public Response(string responce, string info = "")
    {
      string s = responce.ToUpper();
      if (s == Ok || s == Cancel || s == Error)
      {
        Current = s;
        Info = info;
      }
      else
      {
        throw new ArgumentException("No such responce");
      }
    }
  }
}
