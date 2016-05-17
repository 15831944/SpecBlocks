using SpecBlocks.Options;

namespace SpecBlocks
{
   /// <summary>
   /// Значение столбца
   /// </summary>
   class ColumnValue
   {
      public TableColumn ColumnSpec { get; private set; }
      public string Value { get; set; }

      public ColumnValue(TableColumn column)
      {
         ColumnSpec = column;
      }
   }
}