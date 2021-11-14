using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoEditor
{
    class Item
    {
        public string path { get; set; }
        public string type { get; set; }
        public string fileName { get; set; }
        public int grid { get; set; }

        public int start { get; set; }
        public int end { get; set; }

        public int startVideo { get; set; }
        public int endVideo { get; set; }
        public int duration { get; set; }
        public Button button { get; set; }

    }
}
