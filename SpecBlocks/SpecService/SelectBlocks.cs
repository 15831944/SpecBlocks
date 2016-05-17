using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace SpecBlocks
{
    // Выбор блоков монолитных конструкцийй
    public class SelectBlocks
    {
        public List<ObjectId> IdsBlRefSelected { get; private set; }

        public void Select()
        {
            // запрос выбора пользователю
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            IdsBlRefSelected = ed.SelectBlRefs("Выбор блоков для спецификации.");
            ed.WriteMessage($"\nВыбрано блоков: {IdsBlRefSelected.Count}");
        }
    }
}