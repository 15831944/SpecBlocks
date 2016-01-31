using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpecBlocks.Options
{
   public enum EnumBlockProperty
   {
      Attribute
   }

   /// <summary>
   /// Правила отбора блоков для спецификации
   /// </summary>
   public class BlocksFilter
   {
      /// <summary>
      /// Обязательное наличие атрибутов
      /// </summary>
      public List<string> AttrsMustHave { get; set; }
      /// <summary>
      /// Имя блока должно соответствовать этому регулярному выражения Regex.IsMatch(blockName, thisPattern, ignoreCase);
      /// </summary>
      public string BlockNameMatch { get; set; }      
      /// <summary>
      /// Тип блока - свойство блока и его значение - по которому определяется тип блока.
      /// </summary>
      public ItemProp Type { get; set; }
   }   

   /// <summary>
   /// Свойство элемента блока и правила определения
   /// </summary>
   public class ItemProp
   {
      /// <summary>
      /// Имя свойства блока
      /// </summary>
      public string BlockPropName { get; set; }
      /// <summary>
      /// Тип свойства блока
      /// </summary>
      public EnumBlockProperty BlockPropType { get; set; }
      /// <summary>
      /// Имя свойства
      /// </summary>
      public string Name { get; set; }
   }

   // Настойки создания таблицы
   public class SpecOptions
   {
      /// <summary>
      /// Фильтр блоков
      /// </summary>
      public BlocksFilter BlocksFilter { get; set; }
      /// <summary>
      /// Параметр элемента по которому они будут группироваться в таблице
      /// </summary>
      public string GroupPropName { get; set; }
      /// <summary>
      /// Свойства блока и правила их определениы
      /// </summary>
      public List<ItemProp> ItemProps { get; set; }
      /// <summary>
      /// Параметр - группировки элементов по строчкам в группах.
      /// Обычно это марка элемента.
      /// Элементы с одним ключом, должны иметь одинаковые остальные параметры. - иначе будет выведено предупреждающее сообщение.
      /// </summary>
      public string KeyPropName { get; set; }
      /// <summary>
      /// Имя шаблона спецификации
      /// </summary>
      public string Name { get; set; }
      /// <summary>
      /// Настройки таблицы. Столбцы, соотв. свойства элементов
      /// </summary>
      public TableOptions TableOptions { get; set; }

      /// <summary>
      /// Загрузка настроек таблицы из файла
      /// </summary>            
      public static SpecOptions Load(string file)
      {         
         AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(file);
         return ser.DeserializeXmlFile<SpecOptions>();
      }

      /// <summary>
      /// Проверка настроек - заполнены ли важные поля, соответствуют ли имена параметров в элементе и в столбцах таблицы.
      /// NotImplementedException
      /// </summary>
      /// <returns></returns>
      public bool CheckOptions()
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Сохранение файла настроек таблицы в XML в корневой папке программы с именем Name
      /// </summary>
      public void Save(string file)
      {
         if (string.IsNullOrEmpty(Name))
         {
            Logger.Log.Error("Попытка сохранить настройки таблицы SpecOptions без имени.");
            return;
         }         
         AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(file);
         ser.SerializeList(this);
      }      
   }

   public class TableColumn
   {
      /// <summary>
      /// Выравнивание ячеек данных в столбце
      /// </summary>
      public CellAlignment Aligment { get; set; }
      /// <summary>
      /// Соответствующее свойство элемента блока
      /// </summary>
      public string ItemPropName { get; set; }
      /// <summary>
      /// Название столбца
      /// </summary>
      public string Name { get; set; }
      /// <summary>
      /// Ширина столбца
      /// </summary>
      public int Width { get; set; }
   }

   /// <summary>
   /// Настройки таблицы - столбцы и соотв им свойства элементов блоков
   /// </summary>
   public class TableOptions
   {
      /// <summary>
      /// Столбцы таблицы
      /// </summary>
      public List<TableColumn> Columns { get; set; }
      /// <summary>
      /// Слой для вставки таблицы
      /// </summary>
      public string Layer { get; set; }
      /// <summary>
      /// Наименование таблицы
      /// </summary>
      public string Title { get; set; }
   }
}