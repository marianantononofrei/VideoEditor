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
using System.Runtime.InteropServices;
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
        int mouseDownEditor = 0;
        int mouseDownCursor = 0;
        int mouseDownForm = 0;
        public FormEditor()
        {
            InitializeComponent();
            var fullHeight = nrLines * heightColumn;
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
        void DrawVideoEditingLines()
        {
            editGraphic = pnVideoEditing.CreateGraphics();
            editGraphic.Clear(Color.White);
            Brush lineColor = Brushes.Black;
            for (int i = 0; i < nrLines; i++)
            {
                editGraphic.DrawLine(new Pen(lineColor), new Point(0, i * heightColumn), new Point(pnVideoEditing.Width, i * heightColumn));
            }
            // Vertical Lines
            editGraphic.DrawLine(new Pen(lineColor), new Point(heightColumn, 0), new Point(heightColumn, pnVideoEditing.Height));


            editGraphic.DrawLine(new Pen(lineColor), new Point(0, pnVideoEditing.Height - 1), new Point(pnVideoEditing.Width, pnVideoEditing.Height - 1));
        }
        /// <summary>
        /// Drag & Drop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnVideoEditing_Paint(object sender, PaintEventArgs e)
        {
            DrawVideoEditingLines();
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
        private void pnVideoEditing_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void pnVideoEditing_DragLeave(object sender, EventArgs e)
        {
        }
        private void pnVideoEditing_Enter(object sender, EventArgs e)
        {
        }

        private void pnVideoEditing_MouseEnter(object sender, EventArgs e)
        {
            Debug.WriteLine("Mouse enter: " + sender.GetType());
            Cursor.Current = Cursors.SizeNS;
        }

        private void pnVideoEditing_MouseHover(object sender, EventArgs e)
        {
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
        public double Duration(string file)
        {
            IWMPMedia mediainfo = axWindowsMediaPlayer1.newMedia(file);
            return mediainfo.duration;
        }
        private bool isPlaying()
        {
            return axWindowsMediaPlayer1.playState == WMPPlayState.wmppsReady || axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying;
        }
        private void FormEditor_ClientSizeChanged(object sender, EventArgs e)
        {
            DrawVideoEditingLines();
        }
        /// <summary>
        /// Mouse Move Video Editing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnVideoEditing_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Debug.WriteLine("Clicked: " + me.Location);
        }
        private void pnVideoEditing_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Location.Y < 3 && Cursor.Current == Cursors.Default && mouseDownEditor == 0)
            {
                Cursor.Current = Cursors.SizeNS;
            }
            if (mouseDownEditor == 1 && Cursor.Current == Cursors.SizeNS)
            {
                pnVideoEditing.Location = new Point(pnVideoEditing.Location.X, this.PointToClient(Cursor.Position).Y);
                pnVideoEditing.Size = new Size(pnVideoEditing.Width, this.Height + (this.Height - this.ClientRectangle.Height) - this.PointToClient(Cursor.Position).Y - 90);
                DrawVideoEditingLines();
            }
            Debug.WriteLine("Mouse move: " + e.Location);
        }

        private void pnVideoEditing_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseDownEditor != 1)
            {
                if (e.Location.Y < 3 && Cursor.Current == Cursors.Default)
                {
                    Cursor.Current = Cursors.SizeNS;
                }
                mouseDownEditor = 1;
            }
        }

        private void pnVideoEditing_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDownEditor != 0)
            {
                if (Cursor.Current == Cursors.SizeNS)
                {
                    pnVideoEditing.Location = new Point(pnVideoEditing.Location.X, this.PointToClient(Cursor.Position).Y);
                    pnVideoEditing.Size = new Size(pnVideoEditing.Width, this.Height + (this.Height - this.ClientRectangle.Height) - this.PointToClient(Cursor.Position).Y - 90);
                    DrawVideoEditingLines();
                    Cursor.Current = Cursors.Default;
                }
                mouseDownEditor = 0;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            pnVideoEditing.AutoScrollMargin = new Size(pnVideoEditing.Width, pnVideoEditing.Height);
            //SystemParameter.VerticalScrp
        }
        private void FormEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDownForm != 0)
            {
                mouseDownForm = 0;
            }
        }
        private void FormEditor_MouseDown(object sender, MouseEventArgs e)
        {
            if (Math.Abs(e.Location.Y - pnVideoEditing.Location.Y) < 3 && Cursor.Current == Cursors.Default)
            {
                Cursor.Current = Cursors.SizeNS;
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }
            if (Math.Abs(e.Location.X - axWindowsMediaPlayer1.Location.X) < 5 &&
       (Math.Abs(e.Location.Y - axWindowsMediaPlayer1.Height) < 5))
            {
                Cursor.Current = Cursors.SizeNESW;
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }
            if (mouseDownForm == 0)
            {
                mouseDownForm = 1;
            }
        }
        private void FormEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (Math.Abs(e.Location.X - axWindowsMediaPlayer1.Location.X) < 5 &&
                   (Math.Abs(e.Location.Y - axWindowsMediaPlayer1.Height) < 5))
            {
                if (mouseDownForm == 1)
                {
                    axWindowsMediaPlayer1.Location = new Point(e.X, axWindowsMediaPlayer1.Location.Y);
                    axWindowsMediaPlayer1.Size = new Size(this.Width - this.DefaultMargin.Horizontal - e.X - 1, e.Y);
                }
                Cursor.Current = Cursors.SizeNESW;
            }
            if (Math.Abs(e.Location.Y - pnVideoEditing.Location.Y) < 3)
            {
                if (e.Button != MouseButtons.Right)
                {
                    Cursor.Current = Cursors.SizeNS;
                }
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
        /// <summary>
        /// MAIN CURSOR
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbCursor_Click(object sender, EventArgs e)
        {

        }

        private void pbCursor_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownCursor == 1)
            {
                pbCursor.Location = new Point(pnVideoEditing.PointToClient(Cursor.Position).X + pnVideoEditing.AutoScrollPosition.X - 1, pbCursor.Location.Y);
                pbCursor.Update();
            }
        }

        private void pbCursor_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseDownCursor != 1)
            {
                mouseDownCursor = 1;
                timer1.Stop();
            }
        }

        private void pbCursor_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDownCursor != 0)
            {
                mouseDownCursor = 0;
                if (!isPlaying())
                {

                    timer1.Stop();
                }

                pbCursor.Location = new Point(pnVideoEditing.PointToClient(Cursor.Position).X + pnVideoEditing.AutoScrollPosition.X - 1, pbCursor.Location.Y);
                if (pbCursor.Location.X < heightColumn)
                {
                    pbCursor.Location = new Point(heightColumn, pbCursor.Location.Y);
                }
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = pbCursor.Location.X - heightColumn;
                pbCursor.Update();
            }
        }

        private void pbCursor_MouseHover(object sender, EventArgs e)
        {
            if (mouseDownCursor == 1)
            {
                // pbCursor.Location = new Point(this.PointToClient(Cursor.Position).X, pbCursor.Location.Y);
                // pbCursor.Update();
            }
        }

        private void pnVideoEditing_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void axWindowsMediaPlayer1_MouseMoveEvent(object sender, AxWMPLib._WMPOCXEvents_MouseMoveEvent e)
        {
            Point crsPoint = this.PointToClient(Cursor.Position);
            Debug.WriteLine("WMWidth: " + crsPoint.X + " | " + axWindowsMediaPlayer1.Location.X.ToString());
            Debug.WriteLine("WMH: " + crsPoint.Y + " | " + axWindowsMediaPlayer1.Height.ToString());

            if (Math.Abs(crsPoint.Y - axWindowsMediaPlayer1.Height) < 5 &&
                Math.Abs(crsPoint.X - axWindowsMediaPlayer1.Location.X) < 5)
            {
                Cursor.Current = Cursors.SizeNESW;
                if (e.nButton == 1)
                {
                    axWindowsMediaPlayer1.Location = new Point(crsPoint.X, axWindowsMediaPlayer1.Location.Y);
                    axWindowsMediaPlayer1.Size = new Size(this.Width - this.DefaultMargin.Horizontal, crsPoint.Y);
                }
                Debug.WriteLine("Button CLick" + e.nButton);
            }
            else
            {
                Cursor.Current = Cursors.Default;


            }
        }

        private void axWindowsMediaPlayer1_MouseDownEvent(object sender, AxWMPLib._WMPOCXEvents_MouseDownEvent e)
        {

        }

        private void pnVideoEditing_Resize(object sender, EventArgs e)
        {
            pnVideoEditing.AutoScrollMargin = new Size(pnVideoEditing.Width, pnVideoEditing.Height);
        }
    }
}
