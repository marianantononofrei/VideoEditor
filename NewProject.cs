using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoEditor
{
    public partial class NewProject : Form
    {

        public NewProject()
        {
            InitializeComponent();
        }
        public static bool IsValidFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name.Length > 1 && name[1] == ':')
            {
                if (name.Length < 4 || name.ToLower()[0] < 'a' || name.ToLower()[0] > 'z' || name[2] != '\\') return false;
                name = name.Substring(3);
            }
            if (name.StartsWith("\\\\")) name = name.Substring(1);
            if (name.EndsWith("\\") || !name.Trim().Equals(name) || name.Contains("\\\\") ||
                name.IndexOfAny(Path.GetInvalidFileNameChars().Where(x => x != '\\').ToArray()) >= 0) return false;
            return true;
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (!IsValidFileName(textBox1.Text))
            {
                MessageBox.Show("Insert a valid file name!");
                this.DialogResult = DialogResult.No;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void NewProject_Load(object sender, EventArgs e)
        {

        }
    }
}
