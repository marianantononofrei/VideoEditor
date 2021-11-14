using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WMPLib;
using System.Runtime.InteropServices;

namespace VideoEditor
{
    public partial class VideoEditor : Form
    {
        protected override bool ShowFocusCues => false;
        int heightLine = 50;
        int nrHeightLines = 8;
        int[] heightIntervals;
        int maxWidth = 1000; // 25 min
        int zoom = 1;
        Graphics editGraphic;
        int currentItem;
        Dictionary<int, Item> items;
        Project currentProject;
        int CORNER_OFFSET = 10;
        int MARGIN_OFFSET = 5;
        int BUTTON_OFFSET = 5;
        int NEXT_VIDEO_OFFSET = 5;
        int mouseDownX;
        int mouseDownY;
        public VideoEditor()
        {
            InitializeComponent();
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnExit.FlatAppearance.BorderSize = 0;
            this.WindowState = FormWindowState.Maximized;
            var fullHeight = nrHeightLines * heightLine;
            heightIntervals = new int[nrHeightLines + 1];
            for (int i = 0; i <= nrHeightLines; i++)
            {
                heightIntervals[i] = heightLine * i;
            }
            axWindowsMediaPlayer1.PlayStateChange += AxWindowsMediaPlayer1_PlayStateChange;
            pnVideoEditing.VerticalScroll.Visible = false;
            items = new Dictionary<int, Item>();

            currentProject = new Project();
            lblProjectName.Text = "Current Project: " + currentProject.projectName;
            lblProjectName.Location = new Point(this.Width / 2 - lblProjectName.Width / 2, lblProjectName.Location.Y);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //SystemParameter.VerticalScrp
            pbCursor.Location = new Point(heightLine, 5);
            pbCursor.Height = heightLine * (nrHeightLines - 1);
            pnVideoEditing.AutoScrollPosition = new Point(0, 0);
            pnVideoEditing.AutoScrollOffset = new Point(maxWidth, 0);
        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr one, int two, int three, int four);
        private void AxWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 2)
            {
                timer1.Stop();
            }
            else if (e.newState == 9)
            {
                timer1.Stop();
                if (currentItem > 0)
                {
                    int btnEnd = items[currentItem].end;
                    var nr = items.Where(x => x.Key != currentItem).Where(x => (Math.Abs(pbCursor.Location.X + pbCursor.Width / 2 - btnEnd) < NEXT_VIDEO_OFFSET));
                    if (nr != null && nr.Count() == 1)
                    {
                        int key = nr.First().Key;
                        LoadItem(key);
                        currentItem = key;
                        axWindowsMediaPlayer1.Ctlcontrols.play();
                    }
                }

            }
            else if (e.newState == 3)
            {
                timer1.Start();
            }
        }
        void DrawVideoEditingLines()
        {
            editGraphic = pnVideoEditing.CreateGraphics();
            editGraphic.Clear(Color.White);
            Brush lineColor = Brushes.Black;
            for (int i = 0; i < nrHeightLines; i++)
            {
                editGraphic.DrawLine(new Pen(lineColor), new Point(0, i * heightLine),
                                                        new Point(pnVideoEditing.Width, i * heightLine));
            }
            // Vertical Lines
            if (Math.Abs(pnVideoEditing.AutoScrollPosition.X) < heightLine)
            {
                editGraphic.DrawLine(new Pen(lineColor), new Point(heightLine - Math.Abs(pnVideoEditing.AutoScrollPosition.X), 0),
                                    new Point(heightLine - Math.Abs(pnVideoEditing.AutoScrollPosition.X), pnVideoEditing.Height));
            }
            editGraphic.DrawLine(new Pen(lineColor), new Point(0, pnVideoEditing.Height - 1),
                                                    new Point(pnVideoEditing.Width, pnVideoEditing.Height - 1));
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
                        crtButton.Cursor = Cursors.Hand;
                        crtButton.Click += Item_Button_Click;
                        crtButton.MouseUp += Item_Button_MouseUp;
                        crtButton.MouseDown += Item_Button_MouseDown;
                        crtButton.MouseMove += Item_Button_MouseMove;
                        crtButton.Size = new Size((int)duration, 40);
                        pnVideoEditing.Controls.Add(crtButton);
                        crtButton.Location = new Point(startPos, BUTTON_OFFSET + heightLine * (grid - 1));
                        pbCursor.Location = crtButton.Location;
                        Item item = new Item
                        {
                            path = file,
                            fileName = fileName,
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
                        LoadItem(key);
                        currentItem = key;
                        pnVideoEditing.Update();
                    }

                }
            }
            else
            {
                MessageBox.Show("No files added!");
            }


        }

        private void Item_Button_MouseMove(object sender, MouseEventArgs e)
        {
            Button crtButton = sender as Button;
            if (e.Button == MouseButtons.Left && crtButton.BackColor == DefaultBackColor)
            {
                Point position = pnVideoEditing.PointToClient(Cursor.Position);
                position = new Point(Math.Abs(position.X), Math.Abs(position.Y));
                crtButton.Location = new Point(position.X - mouseDownX, position.Y - mouseDownY);
            }
        }

        private void Item_Button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Button crtButton = sender as Button;
                Point position = pnVideoEditing.PointToClient(Cursor.Position);
                position = new Point(Math.Abs(position.X), Math.Abs(position.Y));
                Console.WriteLine("Left: " + e.Location);
                mouseDownX = position.X - crtButton.Location.X;
                mouseDownY = position.Y - crtButton.Location.Y;
                int key = int.Parse(crtButton.Name.Split(',').Last());
                if (currentItem != key)
                {
                    currentItem = key;
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                Console.WriteLine("Right: " + e.Location);
            }

        }
        private void Item_Button_MouseUp(object sender, MouseEventArgs e)
        {
            Button crtButton = sender as Button;
            if (e.Button == MouseButtons.Left && crtButton.BackColor == DefaultBackColor)
            {
                int key = int.Parse(crtButton.Name.Split(',').Last());
                int interval = items[key].grid;
                int centerY = crtButton.Location.Y + crtButton.Height / 2;
                for (int i = 0; i < heightIntervals.Count() - 1; i++)
                {
                    if (centerY >= heightIntervals[i] && centerY < heightIntervals[i + 1])
                    {
                        interval = i;
                        break;
                    }
                }
                items[key].grid = interval;
                items[key].start = crtButton.Location.X;
                items[key].end = crtButton.Location.X + crtButton.Width;
                currentItem = key;
                crtButton.Location = new Point(crtButton.Location.X, interval * heightLine + BUTTON_OFFSET);
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

        private void Item_Button_Click(object sender, EventArgs e)
        {
            Button crt = sender as Button;
            int key = int.Parse(crt.Name.Split(',').Last());
            if (currentItem != key)
            {
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
            return axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying;
        }
        private bool isPaused()
        {
            return axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPaused;
        }
        private void FormEditor_ClientSizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                btnMaximize.Text = @"🗗";
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                btnMaximize.Text = @"🗖";
            }
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
            Debug.WriteLine("Video Editing Clicked: " + me.Location);
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
                pnVideoEditing.Size = new Size(pnVideoEditing.Width,
                    this.Height - this.PointToClient(Cursor.Position).Y - pnTitle.Height / 2);
                hScrollBarZoom.Location = new Point(hScrollBarZoom.Location.X,
                    this.PointToClient(Cursor.Position).Y - hScrollBarZoom.Height - hScrollBarZoom.Margin.Bottom - pnVideoEditing.Margin.Top);
                DrawVideoEditingLines();
            }
            //Debug.WriteLine("Mouse move: " + e.Location);
            if (this.WindowState != FormWindowState.Maximized)
            {
                if (Cursor == Cursors.SizeNWSE || this.PointToClient(Cursor.Position).X > this.Width - MARGIN_OFFSET * 4 && this.PointToClient(Cursor.Position).Y > this.Height - MARGIN_OFFSET * 4)
                {
                    if (e.Button == MouseButtons.None && Cursor != Cursors.SizeNWSE)
                    {
                        Cursor = Cursors.SizeNWSE;
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        this.Height = this.PointToClient(Cursor.Position).Y;
                        this.Width = this.PointToClient(Cursor.Position).X;
                        Cursor = Cursors.SizeNWSE;
                    }
                }
            }
        }

        private void pnVideoEditing_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Location.Y < 3 && Cursor.Current == Cursors.Default)
                {
                    Cursor.Current = Cursors.SizeNS;
                }
                if (Cursor.Current == Cursors.Default)
                {
                    pbCursor.Location = new Point(Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).X), pbCursor.Location.Y);

                    if (items.Count() > 0 && currentItem > 0)
                    {
                        UpdateCurrentItemByCursor();
                    }
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
                    pnVideoEditing.Size = new Size(pnVideoEditing.Width,
                        this.Height - this.PointToClient(Cursor.Position).Y - pnTitle.Height / 2);
                    hScrollBarZoom.Location = new Point(hScrollBarZoom.Location.X,
                        this.PointToClient(Cursor.Position).Y - hScrollBarZoom.Height - hScrollBarZoom.Margin.Bottom - pnVideoEditing.Margin.Top);
                    DrawVideoEditingLines();
                    Cursor.Current = Cursors.Default;
                }
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
        }
        private void FormEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (Math.Abs(e.Location.X - axWindowsMediaPlayer1.Location.X) < CORNER_OFFSET &&
                      (Math.Abs(e.Location.Y - axWindowsMediaPlayer1.Height - pnTitle.Height) < CORNER_OFFSET))
            {
                Cursor.Current = Cursors.SizeNESW;
            }
            if (Math.Abs(e.Location.X - axWindowsMediaPlayer1.Location.X) < CORNER_OFFSET * 5 &&
                   (Math.Abs(e.Location.Y - axWindowsMediaPlayer1.Height) < CORNER_OFFSET * 5))
            {
                if (e.Button == MouseButtons.Left && Cursor.Current == Cursors.SizeNESW)
                {
                    axWindowsMediaPlayer1.Location = new Point(e.X, axWindowsMediaPlayer1.Location.Y);
                    axWindowsMediaPlayer1.Size = new Size(this.Width - e.X - MARGIN_OFFSET, e.Y - pnTitle.Height);
                    axWindowsMediaPlayer1.Update();
                }
            }
            if (e.Button == MouseButtons.None && Cursor != Cursors.Default)
            {
                Cursor = Cursors.Default;
            }
            if (Math.Abs(e.Location.Y - pnVideoEditing.Location.Y) < MARGIN_OFFSET)
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
            if (e.KeyCode == Keys.Space)
            {
                if (isPlaying())
                {
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                }
                else
                {
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }

            }
            if (e.KeyCode == Keys.Delete)
            {

                DeleteCurrentItem();
            }
        }
        void DeleteCurrentItem()
        {
            if (currentItem > 0)
            {
                if (isPlaying())
                {
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                }
                Button btn = items[currentItem].button;
                pnVideoEditing.Controls.Remove(btn);
                items.Remove(currentItem);
                if (items.Count() > 0)
                {
                    currentItem = items.Keys.Max();
                }
                else
                {
                    currentItem = 0;
                }
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
            if (currentItem > 0)
            {
                pbCursor.Location = new Point(items[currentItem].start + (int)axWindowsMediaPlayer1.Ctlcontrols.currentPosition - Math.Abs(pnVideoEditing.AutoScrollPosition.X), pbCursor.Location.Y);
            }
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
        void UpdateCurrentItemByCursor()
        {
            var crt = items.FirstOrDefault(x => x.Value.start <= pbCursor.Location.X && x.Value.end >= pbCursor.Location.X);
            if (crt.Value == null)
            {
                return;
            }
            currentItem = crt.Key;
            LoadItem(currentItem);
            axWindowsMediaPlayer1.Ctlcontrols.currentPosition = pbCursor.Location.X - items[currentItem].button.Location.X;
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }
        void ChangeCursorLocation()
        {
            if (currentItem > 0)
            {
                if (pbCursor.Location.X >= items[currentItem].start && pbCursor.Location.X <= items[currentItem].end)
                {
                    pbCursor.Location = new Point(Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).X) - 1, pbCursor.Location.Y);
                    if (pbCursor.Location.X < heightLine && (Math.Abs(pnVideoEditing.AutoScrollPosition.X) < heightLine))
                    {
                        pbCursor.Location = new Point(heightLine, pbCursor.Location.Y);
                    }
                    axWindowsMediaPlayer1.Ctlcontrols.currentPosition = pbCursor.Location.X - items[currentItem].button.Location.X;
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }
                else
                {
                    var crt = items.FirstOrDefault(x => x.Value.start <= pbCursor.Location.X && x.Value.end >= pbCursor.Location.X);
                    if (crt.Value == null)
                    {
                        return;
                    }
                    int key = crt.Key;
                    LoadItem(key);
                    currentItem = key;
                    axWindowsMediaPlayer1.Ctlcontrols.currentPosition = pbCursor.Location.X - items[key].button.Location.X;
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }
            }
        }
        private void pbCursor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (items.Count > 0 && currentItem > 0)
                {
                    ChangeCursorLocation();
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                if (items.Count > 0 && currentItem > 0)
                {
                    ContextMenu cm = new ContextMenu();
                    if (isPaused())
                    {
                        cm.MenuItems.Add("Split", new EventHandler(Split_Video));
                    }
                    cm.MenuItems.Add("Detach Audio", new EventHandler(Detach_Audio));
                    cm.MenuItems.Add("Remove", new EventHandler(Remove_Video));
                    cm.Show(this, new Point(Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).X) + pnVideoEditing.Location.X + 10, Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).Y) + pnVideoEditing.Location.Y));
                }
            }
        }

        private void Detach_Audio(object sender, EventArgs e)
        {
            if (currentItem > 0)
            {
                VideoProcessing.ConvertMp4ToMp3(items[currentItem].path,
                    currentProject.projectsPath + @"\" + currentProject.projectName + @"\" +
                    VideoProcessing.FileNameWithoutExtension(items[currentItem].fileName) + ".mp3");
            }
        }

        private void Remove_Video(object sender, EventArgs e)
        {
            DeleteCurrentItem();
        }

        private void Split_Video(object sender, EventArgs e)
        {
            SplitCurrentItem();
        }
        void SplitCurrentItem()
        {
            if (isPlaying())
            {
                MessageBox.Show("Press Space to pause then try to split the video!");
            }
            Console.WriteLine("Pressed on split crt item: ");
        }
        void LoadItem(int key)
        {
            if (items.Count > 0 && key > 0)
            {
                if (axWindowsMediaPlayer1.URL != items[key].path)
                {
                    axWindowsMediaPlayer1.URL = items[key].path;
                }
            }
        }
        private void pnVideoEditing_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void axWindowsMediaPlayer1_MouseMoveEvent(object sender, AxWMPLib._WMPOCXEvents_MouseMoveEvent e)
        {
            Point crsPoint = this.PointToClient(Cursor.Position);
            //Debug.WriteLine("WMWidth: " + crsPoint.X + " | " + axWindowsMediaPlayer1.Location.X.ToString());
            //Debug.WriteLine("WMH: " + crsPoint.Y + " | " + axWindowsMediaPlayer1.Height.ToString());

            if (Math.Abs(crsPoint.Y - axWindowsMediaPlayer1.Height - pnTitle.Height) < CORNER_OFFSET &&
                Math.Abs(crsPoint.X - axWindowsMediaPlayer1.Location.X) < CORNER_OFFSET)
            {
                Cursor.Current = Cursors.SizeNESW;
                if (e.nButton == 1)
                {
                    axWindowsMediaPlayer1.Location = new Point(crsPoint.X, axWindowsMediaPlayer1.Location.Y);
                    axWindowsMediaPlayer1.Size = new Size(this.Width - crsPoint.X - (this.DefaultMargin.Horizontal + axWindowsMediaPlayer1.Margin.Horizontal) - MARGIN_OFFSET, crsPoint.Y - pnTitle.Height);
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

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void pnTitle_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x112, 0xf012, 0);
            if (e.Button == MouseButtons.Left && e.Clicks >= 2)
            {
                pnTitle_MouseDoubleClick(sender, e);
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, Constants.WM_NCLBUTTONDOWN, Constants.HTCAPTION, 0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {

                this.WindowState = FormWindowState.Maximized;
                btnMaximize.Text = @"🗗";
            }
            else if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                btnMaximize.Text = @"🗖";
            }
            if (items.Count > 0)
            {
                items[currentItem].button.Select();
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Minimized;
            }
            if (items.Count > 0)
            {
                items[currentItem].button.Select();
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Project test = new Project(false);
            if (test.projectName != "")
            {
                currentProject = test;
                lblProjectName.Text = "Current Project: " + currentProject.projectName;
                lblProjectName.Location = new Point(this.Width / 2 - lblProjectName.Width / 2, lblProjectName.Location.Y);
            }
        }

        private void btnExit_MouseEnter(object sender, EventArgs e)
        {
            Button crtButton = sender as Button;
            crtButton.BackColor = Color.Red;
        }

        private void btnExit_MouseLeave(object sender, EventArgs e)
        {
            Button crtButton = sender as Button;
            crtButton.BackColor = Color.Transparent;
        }

        private void pnTitle_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Maximized;
                    btnMaximize.Text = @"🗗";
                }
                else if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    btnMaximize.Text = @"🗖";
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Video Editor v0.0.1\nCreated by Tutorialeu.com\nAll rights Reserved!");
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Press Space to pause!\nRight click on cursor for other options!\n" +
                "Split is avaliable only on pause time!\nPress delete to remove a video from editor!\nSelect and move the videos as you want!");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteCurrentItem();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = currentProject.projectsPath;
                openFileDialog.Filter = "json files (*.json)|*.json";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = string.Empty;
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;


                    currentProject = new Project(filePath);
                    lblProjectName.Text = "Current Project: " + currentProject.projectName;
                    lblProjectName.Location = new Point(this.Width / 2 - lblProjectName.Width / 2, lblProjectName.Location.Y);
                }
            }
        }
    }
}
