using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class Undo
    {
        public string id { get; set; }
        [ForeignKey("id")]
        TilerUser user { get; set; }
        public string activeId { get; set; }
        public string lastUndoId { get; set; }
        public DateTimeOffset lastUndoTIme { get; set; }
    }
}
