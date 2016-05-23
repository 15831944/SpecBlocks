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
        [CommandMethod("ПИК", "TestSpecMonolith", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestSpecMonolith()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();                
                SpecService specService = new SpecService(new SpecMonolith());
                specService.CreateSpec();
                Inspector.Show();
            });
        }

        [CommandMethod("ПИК", "TestSpecApertures", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestSpecApertures()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();
                SpecService specService = new SpecService(new SpecAperture());
                specService.CreateSpec();
                Inspector.Show();
            });
        }

        [CommandMethod("ПИК", "TestSpecOpenings", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestOpenings()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();                
                SpecService specService = new SpecService(new SpecOpenings());
                specService.CreateSpec();
                Inspector.Show();
            });
        }

        [CommandMethod("ПИК", "TestSpecSlabOpenings", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestSpecSlabOpenings()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();
                SpecService specService = new SpecService(new SpecSlabOpenings());
                specService.CreateSpec();
                Inspector.Show();
            });
        }

        [CommandMethod("ПИК", "TestMonolithNumbering", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestMonolithNumbering()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();
                SpecService specService = new SpecService(new SpecMonolith());
                specService.Numbering();
                Inspector.Show();
            });
        }

        [CommandMethod("ПИК", "TestAperturesNumbering", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestAperturesNumbering()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();
                SpecService specService = new SpecService(new SpecAperture());
                specService.Numbering();
                Inspector.Show();
            });
        }

        [CommandMethod("ПИК", "TestOpeningsNumbering", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestOpeningsNumbering()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();
                SpecService specService = new SpecService(new SpecOpenings());
                specService.Numbering();
                Inspector.Show();
            });
        }

        [CommandMethod("ПИК", "TestSlabOpeningsNumbering", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.NoBlockEditor)]
        public void TestSlabOpeningsNumbering()
        {
            AcadLib.CommandStart.Start(doc =>
            {
                Inspector.Clear();
                SpecService specService = new SpecService(new SpecSlabOpenings());
                specService.Numbering();
                Inspector.Show();
            });
        }
    }   

    /// <summary>
    /// Спецификация монолитных блоков
    /// </summary>
    public class SpecMonolith : ISpecCustom
    {
        private const string name = "КР_Спец_Монолит";

        public string File
        {
            get
            {
                return @"c:\temp\" + name + ".xml";
            }
        }

        public SpecOptions GetDefaultOptions()
        {
            SpecOptions specOpt = new SpecOptions();

            specOpt.CheckDublicates = true;
            specOpt.Name = name;

            // Фильтр для блоков
            specOpt.BlocksFilter = new BlocksFilter();
            // Имя блока начинается с "КР_"
            specOpt.BlocksFilter.BlockNameMatch = "^КР_";
            // Обязательные атрибуты
            specOpt.BlocksFilter.AttrsMustHave = new List<string>()
            {
                "ТИП", "МАРКА", "НАИМЕНОВАНИЕ"
            };
            // Тип блока - атрибут ТИП = Монолит
            specOpt.BlocksFilter.Type = new ItemProp() { BlockPropName = "ТИП", Name = "Монолит", BlockPropType = EnumBlockProperty.Attribute };

            specOpt.GroupPropName = "ГРУППА";
            specOpt.KeyPropName = "МАРКА";

            // Свойства элемента блока
            specOpt.ItemProps = new List<ItemProp>()
            {
                new ItemProp () { Name = "Марка", BlockPropName = "МАРКА", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Обозначение", BlockPropName = "ОБОЗНАЧЕНИЕ", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Наименование", BlockPropName = "НАИМЕНОВАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Масса", BlockPropName = "МАССА", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Примечание", BlockPropName = "ПРИМЕЧАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
            };

            // Настройки Таблицы
            specOpt.TableOptions = new TableOptions();
            specOpt.TableOptions.Title = "Спецификация к схеме расположения элементов замаркированных на данном листе";
            specOpt.TableOptions.Layer = "КР_Таблицы";
            specOpt.TableOptions.Columns = new List<TableColumn>()
            {
                new TableColumn () { Name = "Марка", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Марка", Width = 15 },
                new TableColumn () { Name = "Обозначение", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Обозначение", Width = 60 },
                new TableColumn () { Name = "Наименование", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Наименование", Width = 65 },
                new TableColumn () { Name = "Кол.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Count", Width = 10 },
                new TableColumn () { Name = "Масса, ед. кг", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Масса", Width = 15 },
                new TableColumn () { Name = "Примечание", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Примечание", Width = 20 },
            };

            // Настройки нумерации
            specOpt.NumOptions = new NumberingOptions();
            specOpt.NumOptions.GroupProperties = new List<string>()
            {
                "НАИМЕНОВАНИЕ"
            };
            specOpt.NumOptions.PrefixByBlockName = new XmlSerializableDictionary<string>
            {
                { "КР_Колонна", "К-" },
                { "КР_Пилон", "П-" },
                { "КР_Балка", "Б-" },
                { "КР_Стена", "См-" }
            };

            return specOpt;
        }
    }

    /// <summary>
    /// Спецификация монолитных блоков
    /// </summary>
    public class SpecAperture : ISpecCustom
    {
        private const string name = "КР_Спец_Проемы";

        public string File
        {
            get
            {
                return @"c:\temp\" + name + ".xml";
            }
        }

        public SpecOptions GetDefaultOptions()
        {
            SpecOptions specOpt = new SpecOptions();

            specOpt.CheckDublicates = true;
            specOpt.Name = name;

            // Фильтр для блоков
            specOpt.BlocksFilter = new BlocksFilter();
            // Имя блока начинается с "КР_"
            specOpt.BlocksFilter.BlockNameMatch = "^КР_Проем";
            // Обязательные атрибуты
            specOpt.BlocksFilter.AttrsMustHave = new List<string>()
            {
                "ТИП", "МАРКА", "РАЗМЕР", "ОТМЕТКА_НИЗА", "НАЗНАЧЕНИЕ"
            };
            // Тип блока - атрибут ТИП = Монолит
            specOpt.BlocksFilter.Type = new ItemProp() { BlockPropName = "ТИП", Name = "Проем", BlockPropType = EnumBlockProperty.Attribute };

            specOpt.GroupPropName = ""; // Нет группировки
            specOpt.KeyPropName = "МАРКА";

            // Свойства элемента блока
            specOpt.ItemProps = new List<ItemProp>()
            {
                new ItemProp () { Name = "Марка", BlockPropName = "МАРКА", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Размер", BlockPropName = "РАЗМЕР", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Отметка_низа", BlockPropName = "ОТМЕТКА_НИЗА", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Назначение", BlockPropName = "НАЗНАЧЕНИЕ", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Примечание", BlockPropName = "ПРИМЕЧАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
            };

            // Настройки Таблицы
            specOpt.TableOptions = new TableOptions();
            specOpt.TableOptions.Title = "Ведомость дверных и оконных проемов";
            specOpt.TableOptions.Layer = "КР_Таблицы";
            specOpt.TableOptions.Columns = new List<TableColumn>()
            {
                new TableColumn () { Name = "Марка отв.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Марка", Width = 10 },
                new TableColumn () { Name = "Размеры, мм", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Размер", Width = 20 },
                new TableColumn () { Name = "Отм. низа проема, м", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Отметка_низа", Width = 20 },
                new TableColumn () { Name = "Назначение", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Назначение", Width = 20 },
                new TableColumn () { Name = "Кол-во, шт.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Count", Width = 15 },
                new TableColumn () { Name = "Примечание", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Примечание", Width = 30 },
            };

            // Настройки нумерации
            specOpt.NumOptions = new NumberingOptions();
            specOpt.NumOptions.GroupProperties = new List<string>()
            {
                "РАЗМЕР", "НАЗНАЧЕНИЕ"
            };
            specOpt.NumOptions.PrefixByBlockName = new XmlSerializableDictionary<string>
            {
                { "КР_Проем_Дверной-Стены", "ДП-" },
                { "КР_Проем_Оконный_Стены", "ОП-" }
            };
            specOpt.NumOptions.ExGroupNumbering = "ОТМЕТКА_НИЗА";

            return specOpt;
        }
    }

    /// <summary>
    /// Спецификация монолитных блоков
    /// </summary>
    public class SpecOpenings : ISpecCustom
    {
        private const string name = "КР_Спец_Отверстия";

        public string File
        {
            get
            {
                return @"c:\temp\" + name + ".xml";
            }
        }

        public SpecOptions GetDefaultOptions()
        {
            SpecOptions specOpt = new SpecOptions();

            specOpt.CheckDublicates = true;
            specOpt.Name = name;

            // Фильтр для блоков
            specOpt.BlocksFilter = new BlocksFilter();
            // Имя блока начинается с "КР_"
            specOpt.BlocksFilter.BlockNameMatch = "КР_Отв|КР_Гильза";
            // Обязательные атрибуты
            specOpt.BlocksFilter.AttrsMustHave = new List<string>()
            {
                "ТИП", "МАРКА", "НАЗНАЧЕНИЕ",  "РАЗМЕР", "ОТМЕТКА"
            };
            // Тип блока - атрибут ТИП = Монолит
            specOpt.BlocksFilter.Type = new ItemProp() { BlockPropName = "ТИП", Name = "Отверстие", BlockPropType = EnumBlockProperty.Attribute };

            specOpt.GroupPropName = ""; // Нет группировки
            specOpt.KeyPropName = "МАРКА";

            // Свойства элемента блока
            specOpt.ItemProps = new List<ItemProp>()
            {
                new ItemProp () { Name = "Марка", BlockPropName = "МАРКА", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Размер", BlockPropName = "РАЗМЕР", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Отметка", BlockPropName = "ОТМЕТКА", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Назначение", BlockPropName = "НАЗНАЧЕНИЕ", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Примечание", BlockPropName = "ПРИМЕЧАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
            };

            // Настройки Таблицы
            specOpt.TableOptions = new TableOptions();
            specOpt.TableOptions.Title = "Ведомость инженерных отверстий";
            specOpt.TableOptions.Layer = "КР_Таблицы";
            specOpt.TableOptions.Columns = new List<TableColumn>()
            {
                new TableColumn () { Name = "Марка отв.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Марка", Width = 10 },
                new TableColumn () { Name = "Размеры, мм", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Размер", Width = 20 },
                new TableColumn () { Name = "Отм. низа проема, м", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Отметка", Width = 20 },
                new TableColumn () { Name = "Назначение", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Назначение", Width = 20 },
                new TableColumn () { Name = "Кол-во, шт.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Count", Width = 15 },
                new TableColumn () { Name = "Примечание", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Примечание", Width = 30 },
            };

            // Префиксы для параметров
            specOpt.PrefixParam = new XmlSerializableDictionary<string>
            {
                // Префикс для Гильзы - Ось отв.
                { "КР_Гильза" + "Отметка", "ось отв. " }
            };

            // Настройки нумерации
            specOpt.NumOptions = new NumberingOptions();
            specOpt.NumOptions.GroupProperties = new List<string>()
            {
                "НАЗНАЧЕНИЕ",  "РАЗМЕР"
            };
            specOpt.NumOptions.PrefixByBlockName = new XmlSerializableDictionary<string>
            {
                { "КР_Гильза", "Г" }
            };
            specOpt.NumOptions.ExGroupNumbering = "ОТМЕТКА";

            return specOpt;
        }
    }    

    /// <summary>
    /// Спецификация монолитных блоков
    /// </summary>
    public class SpecSlabOpenings : ISpecCustom
    {
        private const string name = "КР_Спец_Отверстия в плите";

        public string File
        {
            get
            {
                return @"c:\temp\" + name + ".xml";
            }
        }

        public SpecOptions GetDefaultOptions()
        {
            SpecOptions specOpt = new SpecOptions();

            specOpt.CheckDublicates = true;
            specOpt.Name = name;

            // Фильтр для блоков
            specOpt.BlocksFilter = new BlocksFilter();
            // Имя блока начинается с "КР_"
            specOpt.BlocksFilter.BlockNameMatch = "КР_Отв в плите|КР_Гильза в плите";
            // Обязательные атрибуты
            specOpt.BlocksFilter.AttrsMustHave = new List<string>()
            {
                "ТИП", "МАРКА", "РАЗМЕР", "НАЗНАЧЕНИЕ"
            };
            // Тип блока - атрибут ТИП = Отверстие в плите
            specOpt.BlocksFilter.Type = new ItemProp() { BlockPropName = "ТИП", Name = "Отверстие в плите", BlockPropType = EnumBlockProperty.Attribute };

            specOpt.GroupPropName = ""; // Нет группировки
            specOpt.KeyPropName = "МАРКА";

            // Свойства элемента блока
            specOpt.ItemProps = new List<ItemProp>()
            {
                new ItemProp () { Name = "Марка", BlockPropName = "МАРКА", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Размер", BlockPropName = "РАЗМЕР", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Назначение", BlockPropName = "НАЗНАЧЕНИЕ", BlockPropType = EnumBlockProperty.Attribute },
                new ItemProp () { Name = "Примечание", BlockPropName = "ПРИМЕЧАНИЕ", BlockPropType = EnumBlockProperty.Attribute },
            };

            // Настройки Таблицы
            specOpt.TableOptions = new TableOptions();
            specOpt.TableOptions.Title = "Ведомость инженерных отверстий плиты";
            specOpt.TableOptions.Layer = "КР_Таблицы";
            specOpt.TableOptions.Columns = new List<TableColumn>()
            {
                new TableColumn () { Name = "Марка отв.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Марка", Width = 10 },
                new TableColumn () { Name = "Размеры, мм", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Размер", Width = 20 },
                new TableColumn () { Name = "Назначение", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Назначение", Width = 20 },
                new TableColumn () { Name = "Кол-во, шт.", Aligment = CellAlignment.MiddleCenter, ItemPropName = "Count", Width = 15 },
                new TableColumn () { Name = "Примечание", Aligment = CellAlignment.MiddleLeft, ItemPropName = "Примечание", Width = 30 },
            };

            // Настройки нумерации
            specOpt.NumOptions = new NumberingOptions();
            specOpt.NumOptions.GroupProperties = new List<string>()
            {
                "РАЗМЕР"
            };
            specOpt.NumOptions.PrefixByBlockName = new XmlSerializableDictionary<string>
            {
                { "КР_Гильза в плите", "Г" }
            };
            specOpt.NumOptions.ExGroupNumbering = "НАЗНАЧЕНИЕ";

            return specOpt;
        }
    }
}
