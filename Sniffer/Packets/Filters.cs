using System;
using System.Collections.Generic;
using System.Net;

namespace Sniffer
{
  /// <summary>
  /// Класс для фильтрации полученных пакетов
  /// </summary>
  public class Filters
  {
    /// <summary>
    /// Список разрешённых протоколов
    /// </summary>
    public List<Packet.Protocols> Protocols { get; private set; }
    /// <summary>
    /// Разрешённый источник
    /// </summary>
    public IPAddress Source { get; set; }
    /// <summary>
    /// Разрешённый приёмник
    /// </summary>
    public IPAddress Destination { get; set; }

    /// <summary>
    /// Инициализация экземпляра класса
    /// </summary>
    public Filters()
    {
      Protocols = new List<Packet.Protocols>();
      foreach (var protocol in Enum.GetValues(typeof(Packet.Protocols)))
      {
        Protocols.Add((Packet.Protocols)protocol);
      }
    }

    /// <summary>
    /// Добавление протокола в список разрешённых
    /// </summary>
    /// <param name="protocol"></param>
    public void AddProtocol(Packet.Protocols protocol)
    { 
      if (!Protocols.Contains(protocol))
      {
        Protocols.Add(protocol);
      }
    }

    /// <summary>
    /// Удаление протокола из списка разрешённых
    /// </summary>
    /// <param name="protocol"></param>
    public void DeleteProtocol(Packet.Protocols protocol)
    {
      if (Protocols.Contains(protocol))
      {
        Protocols.Remove(protocol);
      }
    }

    /// <summary>
    /// Проверка пакета на соответствие фильтрам
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    public bool CheckPacket(Packet packet)
    {
      if ((!Protocols.Contains(packet.Protocol))
        || (Source != null && !packet.SourceIPAddr.Equals(Source))
        || (Destination != null && !packet.DestIPAddr.Equals(Destination)))
      {
        return false;
      }
      return true;
    }
  }
}
