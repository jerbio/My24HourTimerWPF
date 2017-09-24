using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public interface IUndoable
    {
        bool FirstInstantiation { get; }
        void undoUpdate(Undo undo);
        void undo(string undoId);
        void redo(string undoId);
        string UndoId { get; }
    }
}
