using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using WMPLib;
using System.IO;

namespace VideoEditor
{
    public partial class FormEditor : Form
    {
        int heightColumn = 50;
        int nrLines = 8;
        Graphics editGraphic;
        int mouseDown = 0;
        int mouseDownCursor = 0;
        public FormEditor()
        {
            InitializeComponent();
            pnVideoEditing.AutoScroll = false;
            pnVideoEditing.HorizontalScroll.Enabled = false;
            pnVideoEditing.HorizontalScroll.Visible = false;
            pnVideoEditing.HorizontalScroll.Maximum = 0;
            pnVideoEditing.AutoScroll = true;
            pnVideoEditing.AutoSize = true;
            pnVideoEditing.Update();
            var fullHeight = nrLines * heightColumn;
            pnVideoEditing.AutoScrollMinSize = new Size(0, fullHeight);
            pnVideoEditing.AutoScrollPosition = new Point(0, 0);
            axWindowsMediaPlayer1.PlayStateChange += AxWindowsMediaPlayer1_PlayStateChange;
        }

        private void AxWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 9 || e.newState == 3)
            {
                timer1.Start();
            }
            else
            {
                if (e.newState == 2)
                {
                    timer1.Stop();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        void DrawVideoEditingLines()
        {
            editGraphic = pnVideoEditing.CreateGraphics();
            Brush lineColor = Brushes.Black;
            for (int i = 0; i < nrLines; i++)
            {
                editGraphic.DrawLine(new Pen(lineColor), new Point(0, i * heightColumn), new Point(pnVideoEditing.Width, i * heightColumn));
            }
            // Vertical Lines
            editGraphic.DrawLine(new Pen(lineColor), new Point(heightColumn, 0), new Point(heightColumn, pnVideoEditing.Height));
            pnVideoEditing.Location = new Point(0, this.Height - pnVideoEditing.Height);
        }
        private void pnVideoEditing_Paint(object sender, PaintEventArgs e)
        {
            DrawVideoEditingLines();
        }

        private void pnVideoEditing_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Debug.WriteLine("Clicked: " + me.Location);
        }

        private void pnVideoEditing_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string[] formats = e.Data.GetFormats();
            if (files?.Count() > 0)
            {
                double duration = 0;
                foreach (var file in files)
                {
                    duration += Duration(file);

                    string extension = Path.GetExtension(file);
                    if (extension == ".mp4")
                    {

                        Button item = new Button();
                        item.Name = file + "," + duration;
                        item.Click += Item_Click;
                        item.Size = new Size((int)duration, 40);
                        pnVideoEditing.Controls.Add(item);
                        item.Location = new Point(heightColumn + 5, 5);
                        pbCursor.Location = item.Location;
                        pnVideoEditing.Update();
                    }

                }
            }
            else
            {
                MessageBox.Show("No files added!");
            }


        }

        private void Item_Click(object sender, EventArgs e)
        {
            Button crtButton = sender as Button;
            if (axWindowsMediaPlayer1.URL != crtButton.Name.Split(',')[0] && !isPlaying())
            {
                axWindowsMediaPlayer1.URL = crtButton.Name.Split(',')[0];
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }

        }
        private bool isPlaying()
        {
            return axWindowsMediaPlayer1.playState == WMPPlayState.wmppsReady || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying;
        }
        private void pnVideoEditing_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void pnVideoEditing_DragLeave(object sender, EventArgs e)
        {
        }
        public double Duration(string file)
        {
            IWMPMedia mediainfo = axWindowsMediaPlayer1.newMedia(file);
            return mediainfo.duration;
        }

        private void FormEditor_ClientSizeChanged(object sender, EventArgs e)
        {
            DrawVideoEditingLines();
        }

        private void pnVideoEditing_Enter(object sender, EventArgs e)
        {
        }

        private void pnVideoEditing_MouseEnter(object sender, EventArgs e)
        {
            Debug.WriteLine("Mouse enter: " + sender.GetType());
            Cursor.Current = Cursors.Cross;
        }

        private void pnVideoEditing_MouseHover(object sender, EventArgs e)
        {
            if (mouseDown == 1)
            {
                pnVideoEditing.Location = new Point(pnVideoEditing.Location.X, this.PointToClient(Cursor.Position).Y);
            }
        }

        private void pnVideoEditing_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Location.Y < 3)
            {
                if (Cursor.Current != Cursors.SizeNS)
                {
                    Cursor.Current = Cursors.SizeNS;
                }
                if (mouseDown == 1)
                {
                    pnVideoEditing.Location = new Point(pnVideoEditing.Location.X, this.PointToClient(Cursor.Position).Y);
                }
            }
            else
            {
                if (Cursor.Current != Cursors.Default)
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            Debug.WriteLine("Mouse move: " + e.Location);
        }

        private void pnVideoEditing_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseDown != 1)
            {
                mouseDown = 1;
            }
        }

        private void pnVideoEditing_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown != 0)
            {
                mouseDown = 0;
            }
        }

        private void FormEditor_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("KeyDown: " + e.KeyCode);
            if (isPlaying())
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
            else
            {
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void FormEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine("KeyPress: " + e.KeyChar);
        }

        private void FormEditor_SizeChanged(object sender, EventArgs e)
        {
            DrawVideoEditingLines();
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pbCursor.Location = new Point(heightColumn + (int)axWindowsMediaPlayer1.Ctlcontrols.currentPosition, pbCursor.Location.Y);
        }

        private void pbCursor_Click(object sender, EventArgs e)
        {

        }

        private void pbCursor_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownCursor == 1)
            {
                pbCursor.Location = new Point(e.Location.X, pbCursor.Location.Y);
            }
        }

        private void pbCursor_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseDownCursor != 1)
            {
                mouseDownCursor = 1;
            }
        }

        private void pbCursor_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDownCursor != 0)
            {
                mouseDownCursor = 0;
            }
        }

        private void pbCursor_MouseHover(object sender, EventArgs e)
        {
            if (mouseDownCursor == 1)
            {
                pbCursor.Location = new Point(pnVideoEditing.PointToClient(Cursor.Position).X, pbCursor.Location.Y);
            }
        }
    }
}
