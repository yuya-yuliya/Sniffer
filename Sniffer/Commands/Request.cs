namespace Sniffer.Commands
{
  /// <summary>
  /// Класс, представляющий возможные слова-команды клиента
  /// </summary>
  public class Request
  {
    /// <summary>
    /// Команда "Отправить"
    /// </summary>
    public const string Send = "SEND";
    /// <summary>
    /// Команда "Пустая операция"
    /// </summary>
    public const string Noop = "NOOP";
    /// <summary>
    /// Команда "Выход"
    /// </summary>
    public const string Quit = "QUIT";
    /// <summary>
    /// Команда "Данные"
    /// </summary>
    public const string Data = "DATA";
  }
}
