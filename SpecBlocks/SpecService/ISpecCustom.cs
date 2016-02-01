using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpecBlocks.Options;

namespace SpecBlocks
{
   /// <summary>
   /// Произвольная спецификация.
   /// Должна иметь имя и уметь создавать дефолтные настройки при необхродимости
   /// </summary>
   public interface ISpecCustom
   {      
      /// <summary>
      /// Полный путь к файлу XML
      /// </summary>
      string File { get; }
      /// <summary>
      /// Метод создания дефолтных настроек, на случай отсутствия файла xml.
      /// </summary>
      /// <returns></returns>
      SpecOptions GetDefaultOptions();
   }
}
