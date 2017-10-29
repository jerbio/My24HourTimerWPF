﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class Undo
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string userId { get; set; }
        [ForeignKey("userId")]
        TilerUser user { get; set; }
        public string activeId { get; set; }
        public string lastUndoId { get; set; }
        public DateTimeOffset creationTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset lastUndoTime { get; set; }
        public bool LastAction { get; set; } = false; // if true then last action is re undo else it was redo
    }
}
