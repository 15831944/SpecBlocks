using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using SpecBlocks;
using SpecBlocks.Options;

[assembly: CommandClass(typeof(TestSpecBlocks.TestCommands))]

namespace TestSpecBlocks
{
    public class TestCommands
    {
        [CommandMethod("TestCreateTable")]
        public void TestCreateTable()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (var t = db.TransactionManager.StartTransaction())
            {
                SpecService specService = new SpecService(new SpecTest());
                var groups = specService.SelectAndGroupBlocks();

                int index = 1;
                foreach (var group in groups)
                {
                    foreach (var item in group)
                    {
                        if (item.AtrKey != null)
                        {
                            item.AtrKey.UpgradeOpen();
                            item.AtrKey.TextString = index.ToString();
                            item.Key = index.ToString();
                            Inspector.AddError($"{item.BlName} {SpecBlocks.SpecService.Optinons.KeyPropName}={item.Key}", item.IdBlRef,
                                    icon: System.Drawing.SystemIcons.Information);
                        }                        
                    }
                    index++;
                }
                t.Commit();
            }

        }
    }

    class SpecTest : ISpecCustom
    {
        private const string name = "КР_Спец_Отверстия в плите";

        public string File
        {
            get
            {
                return Path.Combine(@"\\dsk2.picompany.ru\project\CAD_Settings\AutoCAD_server\ShareSettings\КР-МН\Спецификации\КР-МН\Спецификации\" + name + ".xml");
            }
        }

        public SpecOptions GetDefaultOptions()
        {
            SpecOptions specHoleOpt = new SpecOptions();

            specHoleOpt.CheckDublicates = true;
            specHoleOpt.Name = name;

            // Фильтр для блоков
            specHoleOpt.BlocksFilter = new BlocksFilter();
            // Имя блока начинается с "КР_"
            specHoleOpt.BlocksFilter.BlockNameMatch = "КР_Отв в плите";
            // Обязательные атрибуты
            specHoleOpt.BlocksFilter.AttrsMustHave = new List<string>()
         {
            "ТИП", "МАРКА", "НАЗНАЧЕНИЕ", "РАЗМЕР"
         };
            // Тип блока - атрибут ТИП = Отверстие в плите
            specHoleOpt.BlocksFilter.Type = new ItemProp() { BlockPropName = "ТИП", Name = "Отверстие в плите", BlockPropType = EnumBlockProperty.Attribute };

            specHoleOpt.GroupPropName = ""; // Нет группировки
            specHoleOpt.KeyPropName = "МАРКА";

            // Свойства элемента блока
            specHoleOpt.ItemProps = new List<ItemProp>()
         {
            new ItemProp () { Name = "Марка", BlockPropName = "МАРКА", BlockPropType = EnumBlockProperty.Attribute },
            new ItemProp () { Name = "Размер", BlockPropName = "РАЗМЕР", BlockPropType = EnumBlockProperty.Attribute },
            new ItemProp () { Name = "Назначение", BlockPropName = "НАЗНАЧЕНИЕ", BlockPropType = EnumBlockProperty.Attribute },
            new ItemProp () { Name = "Примечание", BlockPropName = "ПРИМЕЧАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
         };

            // Настройки Таблицы
            specHoleOpt.TableOptions = new TableOptions();
            specHoleOpt.TableOptions.Title = "Ведомость инженерных отверстий плиты";
            specHoleOpt.TableOptions.Layer = "КР_Таблицы";
            specHoleOpt.TableOptions.Columns = new List<TableColumn>()
         {
            new TableColumn () { Name = "Марка отв.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Марка", Width = 10 },
            new TableColumn () { Name = "Размеры, мм", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Размер", Width = 20 },
            new TableColumn () { Name = "Назначение", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Назначение", Width = 20 },
            new TableColumn () { Name = "Кол-во, шт.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Count", Width = 15 },
            new TableColumn () { Name = "Примечание", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Примечание", Width = 30 },
         };

            return specHoleOpt;
        }
    }

}
