using System;

namespace Sniffer
{
  /// <summary>
  /// Класс для предоставления в текстовом виде информации о пакетах
  /// </summary>
  public static class Info
  {
    /// <summary>
    /// Предоставление информации о пакете в текстовом виде
    /// </summary>
    /// <param name="packet"></param>
    /// <returns>Информация о пакете в текстовом виде</returns>
    public static string GetPacketInfo(Packet packet)
    {
      string str = "";

      str += "Version: " + packet.Version;
      switch (packet.Version)
      {
        case 4:
          str += "(IPv4)\n";
          break;
        case 6:
          str += "(IPv6)\n";
          break;
      }

      str += $"Header length: {packet.HeaderLength * 4} bytes({packet.HeaderLength} words)\n";
      str += ParseServiceType(packet.ServiceType);
      str += $"Total length: {packet.TotalLength}\n";
      str += "Identificator: 0x" + packet.SegmentID.ToString("X4") + "\n";
      str += ParseFlagsAndOffset(packet.FlagsAndOffset);
      str += $"Time to live: {packet.TTL}\n";
      str += $"Protocol: {packet.Protocol}\n";
      str += "Header checksum: 0x" + packet.HeaderChecksum.ToString("X4") + "\n";
      str += $"Source address: {packet.SourceIPAddr}\n";
      str += $"Destination address: {packet.DestIPAddr}\n";

      return str;
    }

    /// <summary>
    /// Получение подробной информации о типе обслуживания
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    private static string ParseServiceType(byte serviceType)
    {
      string str = "Service type: 0b" + Convert.ToString(serviceType, 2).PadLeft(8, '0') + "\n";
      str += "\tPrecedence: " + (byte)(serviceType & 0xE0 >> 5) + "\n";
      str += "\tDelay: " + ((serviceType >> 4 & 1) == 0 ? "Normal" : "Low") + "\n";
      str += "\tThroughput: " + ((serviceType >> 3 & 1) == 0 ? "Low" : "High") + "\n";
      str += "\tReliability: " + ((serviceType >> 2 & 1) == 0 ? "Normal" : "High") + "\n";

      return str;
    }

    /// <summary>
    /// Получение подробной информации о флагах и смещении фрагмента
    /// </summary>
    /// <param name="flagsAndOffset"></param>
    /// <returns></returns>
    private static string ParseFlagsAndOffset(short flagsAndOffset)
    {
      string str = "Flags and offset: \n";
      str += "\tDont't fragment: " + ((flagsAndOffset >> 14 & 1) == 0 ? "No" : "Yes") + "\n";
      str += "\tMore fragments: " + ((flagsAndOffset >> 13 & 1) == 0 ? "No" : "Yes") + "\n";
      str += "\tOffset: " + ((flagsAndOffset & 0x1FFF) * 8) + " bytes" + "\n";

      return str;
    }

    /// <summary>
    /// Представление данных каждого байта в 16-ой системе счисления
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GetBytesInString(byte[] data)
    {
      string str = "";
      for (int i = 0; i < data.Length; i++)
      {
        if (i != 0 && i % 4 == 0)
        {
          str += "\n";
        }
        str += data[i].ToString("X2") + " ";
      }
      return str;
    }
  }
}
