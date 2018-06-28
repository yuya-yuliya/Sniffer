using System;
using System.Net;

namespace Sniffer
{
  /// <summary>
  /// Класс для работы с IP пакетами
  /// </summary>
  public class Packet
  {
    public byte[] Header { get; private set; }
    public byte Version { get; set; }
    public byte HeaderLength { get; set; }
    public byte ServiceType { get; set; }
    public short TotalLength { get; set; }
    public short SegmentID { get; set; }
    public short FlagsAndOffset { get; set; }
    public byte TTL { get; set; }

    public enum Protocols
    {
      Unknown = 0,
      ICMP = 1,
      IGMP = 2,
      GGP = 3,
      TCP = 6,
      EGP = 8,
      UDP = 17,
      RDP = 27,
      IRTP = 28,
      ISO_TP4 = 29,
      ISO_IP = 80,
      OSPF = 89
    }

    public Protocols Protocol { get; set; }
    public short HeaderChecksum { get; set; }
    public IPAddress SourceIPAddr { get; set; }
    public IPAddress DestIPAddr { get; set; }

    /// <summary>
    /// Инициализация экземпляра класса
    /// </summary>
    /// <param name="data">Данные IP пакета</param>
    /// <param name="length">Длина данных</param>
    public Packet(byte[] data, int length)
    {
      if (length >= 20)
      {
        Version = (byte)((data[0] & 0xF0) >> 4);
        HeaderLength = (byte)(data[0] & 0x0F);
        Header = new byte[HeaderLength * 4];
        Array.Copy(data, Header, HeaderLength * 4);
        ServiceType = data[1];
        TotalLength = (short)((data[2] << 8) | data[3]);
        SegmentID = (short)((data[4] << 8) | data[5]);
        FlagsAndOffset = (short)((data[6] << 8) | data[7]);
        TTL = data[8];
        if (Enum.IsDefined(typeof(Protocols), (int)data[9]))
        {
          Protocol = (Protocols)data[9];
        }
        else
        {
          Protocol = Protocols.Unknown;
        }
        HeaderChecksum = (short)((data[10] << 8) | data[11]);
        byte[] tmpArr = new byte[4];
        Array.Copy(data, 12, tmpArr, 0, 4);
        SourceIPAddr = new IPAddress(tmpArr);
        Array.Copy(data, 16, tmpArr, 0, 4);
        DestIPAddr = new IPAddress(tmpArr);
      }
      else
      {
        throw new ArgumentException("Message must contains 20 bytes header");
      }
    }
  }
}
