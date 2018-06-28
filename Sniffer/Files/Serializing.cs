using System;
using System.Collections.Generic;
using System.IO;

namespace Sniffer
{
  /// <summary>
  /// Класс, предоставляющий методы для сериализации данных
  /// </summary>
  public static class Serializing
  {
    private static string separator = "|";

    /// <summary>
    /// Расширение сериализованных файлов
    /// </summary>
    public static string Extension
    {
      get
      {
        return ".taf";
      }
    }

    /// <summary>
    /// Сериализация данных
    /// </summary>
    /// <param name="packets">Перечисление пакетов</param>
    /// <param name="fileName">Путь файла для сохранения</param>
    public static void Serialize(IEnumerable<Packet> packets, string fileName)
    {
      var headers = new List<string>();
      foreach (Packet packet in packets)
      {
        var bytesStr = new string[packet.Header.Length];
        for (int i = 0; i < bytesStr.Length; i++)
        {
          bytesStr[i] = packet.Header[i].ToString("X2");
        }
        string str = string.Join(separator, bytesStr);
        headers.Add(str);
      }
      File.WriteAllLines(fileName, headers);
    }

    /// <summary>
    /// Десериализация данных
    /// </summary>
    /// <param name="fileName">Путь к файлу</param>
    /// <returns>Список пакетов</returns>
    public static List<Packet> Deserialize(string fileName)
    {
      try
      {
        string[] headers = File.ReadAllLines(fileName);
        var packets = new List<Packet>();
        foreach (string header in headers)
        {
          string[] bytesStr = header.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
          var bytes = new byte[bytesStr.Length];
          for (int i = 0; i < bytesStr.Length; i++)
          {
            bytes[i] = Convert.ToByte(bytesStr[i], 16);
          }
          packets.Add(new Packet(bytes, bytes.Length));
        }
        return packets;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }
}
