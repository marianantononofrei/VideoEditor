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

        int heightLine = 50;
        int nrHeightLines = 8;
        int maxWidth = 6000; // 100 min
        int zoom = 1;
        Graphics editGraphic;
        int currentItem;
        Dictionary<int, Item> items;
        int CORNER_OFFSET = 10;
        int MARGIN_OFFSET = 5;
        public FormEditor()
        {
            InitializeComponent();
            var fullHeight = nrHeightLines * heightLine;
            axWindowsMediaPlayer1.PlayStateChange += AxWindowsMediaPlayer1_PlayStateChange;
            pnVideoEditing.Width = maxWidth + heightLine;
            items = new Dictionary<int, Item>();
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
            for (int i = 0; i < nrHeightLines; i++)
            {
                editGraphic.DrawLine(new Pen(lineColor), new Point(0, i * heightLine), new Point(pnVideoEditing.Width, i * heightLine));
            }
            // Vertical Lines
            if (Math.Abs(pnVideoEditing.AutoScrollPosition.X) < heightLine)
            {
                editGraphic.DrawLine(new Pen(lineColor), new Point(heightLine - Math.Abs(pnVideoEditing.AutoScrollPosition.X), 0), new Point(heightLine - Math.Abs(pnVideoEditing.AutoScrollPosition.X), pnVideoEditing.Height));
            }


            editGraphic.DrawLine(new Pen(lineColor), new Point(0, pnVideoEditing.Height - 1), new Point(pnVideoEditing.Width, pnVideoEditing.Height - 1));
            editGraphic.TranslateTransform(pnVideoEditing.AutoScrollPosition.X, pnVideoEditing.AutoScrollPosition.Y);

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
                    duration = Duration(file);
                    string extension = Path.GetExtension(file);
                    string fileName = Path.GetFileName(file);
                    if (extension == ".mp4")
                    {
                        int key = 0;
                        int startPos = 0;
                        int grid = 1;
                        if (items.Count > 0)
                        {
                            startPos = items.Values.Max(x => x.end);
                            key = items.Keys.Max() + 1;
                        }
                        else
                        {
                            key = 1;
                            startPos = heightLine;
                        }
                        Button crtButton = new Button();
                        crtButton.Text = fileName + " : " + DurationReadeble(duration);
                        crtButton.Name = file + "," + duration + "," + key;
                        crtButton.Click += Item_Click;
                        crtButton.Size = new Size((int)duration, 40);
                        pnVideoEditing.Controls.Add(crtButton);
                        crtButton.Location = new Point(startPos, 5 + heightLine * (grid - 1));
                        pbCursor.Location = crtButton.Location;
                        Item item = new Item
                        {
                            path = file,
                            type = extension,
                            grid = grid,
                            start = crtButton.Location.X,
                            end = crtButton.Location.X + (int)duration,
                            startVideo = 0,
                            endVideo = (int)duration,
                            duration = (int)duration,
                            button = crtButton,
                        };
                        items.Add(key, item);
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
            int key = int.Parse(crtButton.Name.Split(',').Last());
            if (isPlaying())
            {
                axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
            if (axWindowsMediaPlayer1.URL != items[key].path)
            {
                axWindowsMediaPlayer1.URL = items[key].path;
                axWindowsMediaPlayer1.Ctlcontrols.play();
                currentItem = key;
            }

        }
        public double Duration(string file)
        {
            IWMPMedia mediainfo = axWindowsMediaPlayer1.newMedia(file);
            return mediainfo.duration;
        }
        public string DurationReadeble(double duration)
        {
            return TimeSpan.FromSeconds(duration).ToString(@"mm\:ss");
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
            if (e.Location.Y < 3 && Cursor.Current == Cursors.Default && e.Button == MouseButtons.None)
            {
                Cursor.Current = Cursors.SizeNS;
            }
            if (Cursor.Current == Cursors.SizeNS && e.Button == MouseButtons.Left)
            {
                pnVideoEditing.Location = new Point(pnVideoEditing.Location.X, this.PointToClient(Cursor.Position).Y);
                pnVideoEditing.Size = new Size(pnVideoEditing.Width, this.Height + (this.Height - this.ClientRectangle.Height) - this.PointToClient(Cursor.Position).Y - 90);
                hScrollBarZoom.Location = new Point(hScrollBarZoom.Location.X, this.PointToClient(Cursor.Position).Y - hScrollBarZoom.Height - hScrollBarZoom.Margin.Bottom - pnVideoEditing.Margin.Top);
                DrawVideoEditingLines();
            }
            Debug.WriteLine("Mouse move: " + e.Location);
        }

        private void pnVideoEditing_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Location.Y < 3 && Cursor.Current == Cursors.Default)
                {
                    Cursor.Current = Cursors.SizeNS;
                }
            }
        }

        private void pnVideoEditing_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Cursor.Current == Cursors.SizeNS)
                {
                    pnVideoEditing.Location = new Point(pnVideoEditing.Location.X, this.PointToClient(Cursor.Position).Y);
                    pnVideoEditing.Size = new Size(pnVideoEditing.Width, this.Height + (this.Height - this.ClientRectangle.Height) - this.PointToClient(Cursor.Position).Y - 90);
                    hScrollBarZoom.Location = new Point(hScrollBarZoom.Location.X, this.PointToClient(Cursor.Position).Y - hScrollBarZoom.Height - hScrollBarZoom.Margin.Bottom - pnVideoEditing.Margin.Top);
                    DrawVideoEditingLines();
                    Cursor.Current = Cursors.Default;
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //SystemParameter.VerticalScrp
            pbCursor.Location = new Point(heightLine, 5);
            pbCursor.Height = heightLine * (nrHeightLines - 1);
            pnVideoEditing.AutoScrollPosition = new Point(0, 0);
            pnVideoEditing.AutoScrollOffset = new Point(9000, 0);
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
        }
        private void FormEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (Math.Abs(e.Location.X - axWindowsMediaPlayer1.Location.X) < CORNER_OFFSET &&
                      (Math.Abs(e.Location.Y - axWindowsMediaPlayer1.Height) < CORNER_OFFSET))
            {
                Cursor.Current = Cursors.SizeNESW;
            }
            if (Math.Abs(e.Location.X - axWindowsMediaPlayer1.Location.X) < CORNER_OFFSET * 5 &&
                   (Math.Abs(e.Location.Y - axWindowsMediaPlayer1.Height) < CORNER_OFFSET * 5))
            {
                if (e.Button == MouseButtons.Left && Cursor.Current == Cursors.SizeNESW)
                {
                    axWindowsMediaPlayer1.Location = new Point(e.X, axWindowsMediaPlayer1.Location.Y);
                    axWindowsMediaPlayer1.Size = new Size(this.Width - e.X - (this.DefaultMargin.Horizontal + axWindowsMediaPlayer1.Margin.Horizontal) - MARGIN_OFFSET, e.Y);
                    axWindowsMediaPlayer1.Update();
                }
            }
            if (Math.Abs(e.Location.Y - pnVideoEditing.Location.Y) < 3)
            {
                if (e.Button == MouseButtons.None)
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
            pbCursor.Location = new Point(items[currentItem].start + (int)axWindowsMediaPlayer1.Ctlcontrols.currentPosition - Math.Abs(pnVideoEditing.AutoScrollPosition.X), pbCursor.Location.Y);
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
            if (e.Button == MouseButtons.Left)
            {
                pbCursor.Location = new Point(Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).X) - 1, pbCursor.Location.Y);
                pbCursor.Update();
            }
        }
        Button GetIntersectedButton()
        {
            foreach (var btn in pnVideoEditing.Controls.OfType<Button>())
            {
                if (pbCursor.Bounds.IntersectsWith(btn.Bounds))
                {
                    return btn;
                }
            }
            return null;
        }
        private void pbCursor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pbCursor.Location = new Point(Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).X) - 1, pbCursor.Location.Y);
                if (pbCursor.Location.X < heightLine && (Math.Abs(pnVideoEditing.AutoScrollPosition.X) < heightLine))
                {
                    pbCursor.Location = new Point(heightLine, pbCursor.Location.Y);
                }
                Button intButton = GetIntersectedButton();
                if (intButton == null)
                {
                    return;
                }
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = pbCursor.Location.X - intButton.Location.X;
                pbCursor.Update();
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

            if (Math.Abs(crsPoint.Y - axWindowsMediaPlayer1.Height) < CORNER_OFFSET &&
                Math.Abs(crsPoint.X - axWindowsMediaPlayer1.Location.X) < CORNER_OFFSET)
            {
                Cursor.Current = Cursors.SizeNESW;
                if (e.nButton == 1)
                {
                    axWindowsMediaPlayer1.Location = new Point(crsPoint.X, axWindowsMediaPlayer1.Location.Y);
                    axWindowsMediaPlayer1.Size = new Size(this.Width - crsPoint.X - (this.DefaultMargin.Horizontal + axWindowsMediaPlayer1.Margin.Horizontal) - MARGIN_OFFSET, crsPoint.Y);
                    axWindowsMediaPlayer1.Update();
                }
                Debug.WriteLine("Button CLick" + e.nButton);
            }
            else
            {
                Cursor.Current = Cursors.Default;


            }
        }

        private void pnVideoEditing_Resize(object sender, EventArgs e)
        {
            pnVideoEditing.AutoScrollMargin = new Size(pnVideoEditing.Width, pnVideoEditing.Height);
            pbCursor.Height = nrHeightLines * (heightLine - 1);
        }

        private void FormEditor_Resize(object sender, EventArgs e)
        {
            pnVideoEditing.AutoScrollMargin = new Size(pnVideoEditing.Width, pnVideoEditing.Height);
            pbCursor.Height = nrHeightLines * (heightLine - 1);
        }
    }
}
