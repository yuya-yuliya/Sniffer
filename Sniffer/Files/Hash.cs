using System.Security.Cryptography;
using System.Text;

namespace Sniffer
{
  /// <summary>
  /// Класс для расчёта хэш-кода для данных
  /// </summary>
  public class Hash
  {
    /// <summary>
    /// Расчёт хэш-кода по алгоритму SHA-1
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GetHashSha1(byte[] data)
    {
      using (SHA1 sha1 = SHA1.Create())
      {
        byte[] hashBytes = sha1.ComputeHash(data);
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
          builder.Append(hashBytes[i].ToString("x2"));
        }

        return builder.ToString();
      }
    }
  }
}
