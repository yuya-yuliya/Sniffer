using System;
using System.IO;

namespace Sniffer
{
  /// <summary>
  /// Класс для работы с кодировкой Base 64
  /// </summary>
  public static class Base64
  {
    /// <summary>
    /// Преобразование данных в строку в кодировка Base 64
    /// </summary>
    /// <param name="path">Путь к файлу</param>
    /// <returns></returns>
    public static string GetBase64FromFile(string path)
    {
      byte[] data = File.ReadAllBytes(path);
      return Convert.ToBase64String(data);
    }

    /// <summary>
    /// Получение данных из строки в кодировке Base 64
    /// </summary>
    /// <param name="strBase64"></param>
    /// <returns></returns>
    public static byte[] GetFileData(string strBase64)
    {
      byte[] data = Convert.FromBase64String(strBase64);
      return data;
    }
  }
}
