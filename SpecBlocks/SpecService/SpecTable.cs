using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AcadLib.Errors;
using AcadLib.Jigs;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SpecBlocks.Options;

namespace SpecBlocks
{
   internal class SpecTable
   {
      internal Document Doc { get; private set; } = Application.DocumentManager.MdiActiveDocument;
      internal List<SpecGroup> Groups { get; private set; } = new List<SpecGroup>();
      internal List<SpecItem> Items { get; private set; } = new List<SpecItem>();
      internal SelectBlocks SelBlocks { get; private set; } = new SelectBlocks();
      internal SpecOptions SpecOptions { get; private set; }

      public SpecTable(SpecOptions options)
      {
         SpecOptions = options;
      }

      /// <summary>
      /// Создание таблицы спецификации блоков, с запросом выбора блоков у пользователя.
      /// Таблица будет вставлена в указанное место пользователем в текущем пространстве.
      /// </summary>
      public void CreateTable()
      {
         // Выбор блоков
         SelBlocks.Select();

         using (var t = Doc.TransactionManager.StartTransaction())
         {
            try
            {
               // Фильтрация блоков
               Items = SpecItem.FilterSpecItems(this);
               // Группировка элементов
               Groups = SpecGroup.Grouping(this);

               // Создание таблицы
               Table table = getTable();
               // Вставка таблицы
               insertTable(table);
            }
            catch (Exception ex)
            {
               Logger.Log.Error(ex, "SpecTable.CreateTable().");
               Inspector.AddError(ex.Message, icon: SystemIcons.Error);
            }

            t.Commit();
         }
      }

      private Table getTable()
      {
         Table table = new Table();
         table.SetDatabaseDefaults(Doc.Database);
         table.TableStyle = Doc.Database.GetTableStylePIK(); // если нет стиля ПИк в этом чертеже, то он скопируетс из шаблона, если он найдется
         if (!string.IsNullOrEmpty(SpecOptions.TableOptions.Layer))
         {
            table.LayerId = AcadLib.Layers.LayerExt.GetLayerOrCreateNew(new AcadLib.Layers.LayerInfo(SpecOptions.TableOptions.Layer));
         }         

         int rows = 2 + Groups.Count + Groups.Sum(g => g.Records.Count);
         table.SetSize(rows, SpecOptions.TableOptions.Columns.Count);

         for (int i = 0; i < table.Columns.Count; i++)
         {
            var specCol = SpecOptions.TableOptions.Columns[i];
            var col = table.Columns[i];
            col.Alignment = specCol.Aligment;
            col.Width = specCol.Width;
            col.Name = specCol.Name;

            var cellColName = table.Cells[1, i];
            cellColName.TextString = specCol.Name;
            cellColName.Alignment = CellAlignment.MiddleCenter;
         }

         // Название таблицы
         var rowTitle = table.Cells[0, 0];
         rowTitle.Alignment = CellAlignment.MiddleCenter;
         rowTitle.TextHeight = 5;
         rowTitle.TextString = SpecOptions.TableOptions.Title;

         // Строка заголовков столбцов
         var rowHeaders = table.Rows[1];
         rowHeaders.Height = 15;
         var lwBold = rowHeaders.Borders.Top.LineWeight;
         rowHeaders.Borders.Bottom.LineWeight = lwBold;

         int row = 2;
         foreach (var group in Groups)
         {
            string groupName = group.Name;
            if (string.IsNullOrEmpty(group.Name))
            {
               if (Groups.Count == 1)
               {
                  // Если кол групп = 1, и она пустая, то удаление строки группы
                  table.DeleteRows(row, 1);
                  row--;               
               }
               else
               {
                  // Если кол групп > 1, и она пустая, то название для пустой группы - Разное
                  groupName = "Разное";
               }               
            }
            else
            {
               table.Cells[row, 2].TextString = "{0}{1}{2}".f("{\\L", groupName, "}");
               table.Cells[row, 2].Alignment = CellAlignment.MiddleCenter;
            }            

            row++;
            foreach (var rec in group.Records)
            {
               for (int i = 0; i < table.Columns.Count; i++)
               {
                  var colVal = rec.ColumnsValue[i];
                  table.Cells[row, i].TextString = colVal.Value;
               }
               row++;
            }
         }
         var lastRow = table.Rows.Last();
         lastRow.Borders.Bottom.LineWeight = lwBold;

         table.GenerateLayout();
         return table;
      }

      private void insertTable(Table table)
      {
         Database db = Doc.Database;
         Editor ed = Doc.Editor;

         TableJig jigTable = new TableJig(table, 1 / db.Cannoscale.Scale, "Вставка таблицы");
         if (ed.Drag(jigTable).Status == PromptStatus.OK)
         {
            var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
            cs.AppendEntity(table);
            db.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(table, true);
         }
      }
   }
}