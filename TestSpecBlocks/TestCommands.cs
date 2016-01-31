using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using SpecBlocks.Options;

[assembly: CommandClass(typeof(TestSpecBlocks.TestCommands))]

namespace TestSpecBlocks
{
   public class TestCommands
   {
      [CommandMethod("TestCreateTable")]
      public void TestCreateTable()
      {
         var specOpt = getTestSpecOptions();         
         var specTable = new SpecBlocks.SpecTable(specOpt);         
         specTable.CreateTable();         
      }

      private SpecOptions getTestSpecOptions()
      {
         SpecOptions specMonolOpt = new SpecOptions();

         specMonolOpt.Name = "Test";

         // Фильтр для блоков
         specMonolOpt.BlocksFilter = new BlocksFilter();
         // Имя блока начинается с "КР_"
         specMonolOpt.BlocksFilter.BlockNameMatch = "^КР_";
         // Обязательные атрибуты
         specMonolOpt.BlocksFilter.AttrsMustHave = new List<string>()
         {
            "ТИП", "МАРКА", "НАИМЕНОВАНИЕ"
         };

         specMonolOpt.GroupPropName = "ГРУППА";
         specMonolOpt.KeyPropName = "МАРКА";

         // Свойства элемента блока
         specMonolOpt.ItemProps = new List<ItemProp>()
         {
            new ItemProp () { Name = "Марка", BlockPropName = "МАРКА", BlockPropType = EnumBlockProperty.Attribute },
            new ItemProp () { Name = "Обозначение", BlockPropName = "ОБОЗНАЧЕНИЕ", BlockPropType = EnumBlockProperty.Attribute },
            new ItemProp () { Name = "Наименование", BlockPropName = "НАИМЕНОВАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
            new ItemProp () { Name = "Масса", BlockPropName = "МАССА", BlockPropType = EnumBlockProperty.Attribute },
            new ItemProp () { Name = "Примечание", BlockPropName = "ПРИМЕЧАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
         };

         // Настройки Таблицы
         specMonolOpt.TableOptions = new TableOptions();
         specMonolOpt.TableOptions.Title = "Спецификация к схеме расположения элементов замаркированных на данном листе";
         specMonolOpt.TableOptions.Layer = "КР_Таблицы";
         specMonolOpt.TableOptions.Columns = new List<TableColumn>()
         {
            new TableColumn () { Name = "Марка", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Марка", Width = 15 },
            new TableColumn () { Name = "Обозначение", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Обозначение", Width = 60 },
            new TableColumn () { Name = "Наименование", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Наименование", Width = 65 },
            new TableColumn () { Name = "Кол.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Count", Width = 10 },
            new TableColumn () { Name = "Масса, ед. кг", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Масса", Width = 15 },
            new TableColumn () { Name = "Примечание", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Примечание", Width = 20 },
         };

         return specMonolOpt;
      }
   }
}
