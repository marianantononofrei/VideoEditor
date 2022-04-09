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
        private Brush lineColor = Brushes.Black;
        Size visibleSize;
        int INTERVAL_HEIGHT = 30;
        int xIntervalLocation = 0;
        int HEIGHT_LINES = 80;
        int maxNrHeightLines = 8;
        int[] heightIntervals;
        int maxWidth = 8_000;
        int fragment = 6; // 6-36 pixels fragments
        int stage = 0; // 0 - 6 zoomStage
        double[] stageIntervals = new double[6] { 3600, 600, 60, 20, 4, 0.2 }; // all zoom stage intervals
        int MAX_STAGE = 5;
        int previousZoom = 0;
        int maxZoomNrOfPixels;
        int MIN_FRAGMENT = 6;
        int MAX_FRAGMENT = 36;
        int zoom = 0; // current zoom
        double onePixelSec = 30; // 1 pixel is 30 sec
        Graphics videoGraphic;
        Graphics leftMenuGraphic;
        int currentItem;
        Dictionary<int, Item> items;
        Project currentProject;
        int NEXT_VIDEO_OFFSET = 5;
        int mouseDownButtonX;
        int mouseDownButtonY;


        void UpdateTotalViewNrOfPixels()
        {
            maxZoomNrOfPixels = visibleSize.Width;
        }
        public VideoEditor()
        {
            InitializeComponent();
            pnVideoEditing.Width = maxWidth;
            pnVideoEditing.Height = maxNrHeightLines * HEIGHT_LINES + INTERVAL_HEIGHT;
            visibleSize = new Size(this.Width - this.DefaultMargin.Horizontal, this.Height - this.DefaultMargin.Vertical - pnVideoEditing.Location.Y);
            videoGraphic = pnVideoEditing.CreateGraphics();
            leftMenuGraphic = pnLeftMenu.CreateGraphics();
            UpdateTotalViewNrOfPixels();
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMaximize.FlatAppearance.BorderSize = 0;
            btnExit.FlatAppearance.BorderSize = 0;
            this.WindowState = FormWindowState.Maximized;
            var fullHeight = maxNrHeightLines * HEIGHT_LINES;
            heightIntervals = new int[maxNrHeightLines + 1];
            for (int i = 0; i <= maxNrHeightLines; i++)
            {
                heightIntervals[i] = HEIGHT_LINES * i;
            }
            axWindowsMediaPlayer1.PlayStateChange += AxWindowsMediaPlayer1_PlayStateChange;
            //pnVideoEditing.VerticalScroll.Visible = false;
            items = new Dictionary<int, Item>();

            currentProject = new Project();
            lblProjectName.Text = "Current Project: " + currentProject.projectName;
            lblProjectName.Location = new Point(this.Width / 2 - lblProjectName.Width / 2, lblProjectName.Location.Y);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //SystemParameter.VerticalScrp
            pbCursor.Location = new Point(HEIGHT_LINES, 0);
            pbCursor.Height = HEIGHT_LINES * (maxNrHeightLines - 1);
            pnVideoEditing.AutoScrollPosition = new Point(0, 0);
            //cursorTimer.Interval = 1000;
            cursorTimer.Start();
            pnVideoEditing.Update();
            hScrollBarZoom.BringToFront();
        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr one, int two, int three, int four);
        private void AxWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 9)
            {
                if (currentItem > 0)
                {
                    int btnEnd = items[currentItem].endPoint;
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
        }
        void DrawLeftSideLines()
        {
            // horizontal line
            for (int i = 0; i * HEIGHT_LINES + INTERVAL_HEIGHT < pnLeftMenu.Height; i++)
            {
                leftMenuGraphic.DrawLine(new Pen(lineColor), new Point(0, i * HEIGHT_LINES + INTERVAL_HEIGHT),
                                                        new Point(pnLeftMenu.Width, i * HEIGHT_LINES + INTERVAL_HEIGHT));

            }
        }
        void DrawVideoEditingLines()
        {

            //videoGraphic.DrawLine(new Pen(lineColor), new Point(visibleSize.Width, 0),
            //                                            new Point(visibleSize.Width, visibleTotalWidth));

            int nrTotalPixels = visibleSize.Width;
            int nrCurrentPixels = maxZoomNrOfPixels;
            //int nrTotalSeconds = maxZoomNrOfPixels * onePixelSec;

            int stepCount = 0;
            zoom = nrTotalPixels - nrCurrentPixels;
            //var zoomRatio = 0;
            if (zoom > 0)
            {
                stepCount = nrCurrentPixels / zoom;
                //nrTotalSeconds = nrCurrentPixels * onePixelSec + (nrCurrentPixels % onePixelSec);
                //zoomRatio = nrTotalSeconds / nrTotalPixels;
                //Debug.WriteLine("zoomRatio: " + zoomRatio);
                //Debug.WriteLine("moduloRatio: " + (nrTotalSeconds % nrTotalPixels));
                Debug.WriteLine("onePixelSec: " + onePixelSec);
            }
            //Debug.WriteLine("NrOfTotalVisibleSeconds: " + nrTotalSeconds);
            Debug.WriteLine("Zoom: " + zoom);
            //int markDifference = (nrTotalPixels / fragment + (nrTotalPixels % fragment)) - ((nrTotalPixels / (fragment + 1)) + (nrTotalPixels % fragment + 1));
            //Debug.WriteLine("markDifference: " + markDifference + " stepCount: " + stepCount);
            //Debug.WriteLine("Visible Width: " + nrTotalPixels + " / current mark: " + fragment + " = " + nrTotalPixels / fragment);

            // fragment interval: min - 6px
            // fragment interval: max - 36px if > 36px = 6
            int nextZoom = nrTotalPixels / (fragment + 1);
            int prevZoom = nrTotalPixels / fragment;
            Debug.WriteLine("nextZoom: " + nextZoom);
            if (zoom > nextZoom)
            {
                if (fragment < MAX_FRAGMENT)
                {
                    fragment++;
                    Debug.WriteLine("New Mark++: " + fragment);
                    previousZoom = zoom;
                    onePixelSec = stageIntervals[stage] / (fragment * 20d);
                    RefreshItemsPositionAndSize();
                    maxZoomNrOfPixels = visibleSize.Width;
                    stepCount = 0;
                }
                if (fragment == 36)
                {
                    // zoomStage ++
                    if (stage < MAX_STAGE)
                    {
                        fragment = MIN_FRAGMENT;
                        stage++;
                        onePixelSec = stageIntervals[stage] / (fragment * 20d);
                        RefreshItemsPositionAndSize();
                        hScrollBarZoom.Value = stage;
                    }
                }
            }
            if (zoom == 0)
            {
                if (fragment > MIN_FRAGMENT)
                {
                    fragment--;
                    onePixelSec = stageIntervals[stage] / (fragment * 20d);
                    RefreshItemsPositionAndSize();
                    maxZoomNrOfPixels = visibleSize.Width - previousZoom;
                    nrCurrentPixels = maxZoomNrOfPixels;
                    zoom = nrTotalPixels - nrCurrentPixels;
                    if (zoom == 0)
                    {
                        return;
                    }
                    stepCount = nrCurrentPixels / zoom;
                    Debug.WriteLine("New Mark-: " + fragment);
                }
                if (fragment == 6)
                {
                    // zoomStage ++
                    if (stage > 0)
                    {
                        fragment = MAX_FRAGMENT - 1;
                        stage--;
                        onePixelSec = stageIntervals[stage] / (fragment * 20d);
                        RefreshItemsPositionAndSize();
                        hScrollBarZoom.Value = stage;
                    }
                }
            }
            Debug.WriteLine("New Pixel Sec: " + onePixelSec);
            Debug.WriteLine("Stage is: " + stage);
            Debug.WriteLine("Final Mark Is: " + fragment);
            // vertical Line
            int crtSepCount = stepCount;
            //drawing small interval
            int k = 0; // time distance
            int time_show = fragment == 6 ? 4 : fragment == 7 ? 2 : fragment > 12 ? 1 : 2; // 6 >= 4, 7 >= 2, 13 >= 1
            int j = 0; // big line distnace
            int prev = 0; // time offset

            // draw time intervals 
            for (int i = 0; i < nrTotalPixels; i += fragment)
            {
                videoGraphic.DrawLine(new Pen(lineColor), new Point(i, INTERVAL_HEIGHT),
                                                        new Point(i, INTERVAL_HEIGHT - 4));
                if (j == 0)
                {
                    videoGraphic.DrawLine(new Pen(lineColor), new Point(i, INTERVAL_HEIGHT),
                                        new Point(i, INTERVAL_HEIGHT - 8));
                    if (k == 0)
                    {
                        videoGraphic.DrawString(TimeSpan.FromSeconds(onePixelSec * (i - prev)).ToString(@"hh\:mm\:ss\:ff"),
                                                            SystemFonts.DefaultFont, lineColor, i, 6);
                        k = time_show;
                    }
                    j = 5;
                    k--;
                }
                /*
                if (fragment > 29 && j == 3)
                {
                    videoGraphic.DrawString(TimeSpan.FromSeconds(onePixelSec * (i - prev)).ToString(@"hh\:mm\:ss\:ff"),
                                                            SystemFonts.DefaultFont, lineColor, i + 12, 6);
                }*/
                j--;
                if (crtSepCount > 0 && i >= crtSepCount)
                {
                    i++;
                    prev++;
                    crtSepCount += stepCount;
                }
            }

            // horizontal line
            for (int i = 0; i * HEIGHT_LINES + INTERVAL_HEIGHT < visibleSize.Height; i++)
            {
                videoGraphic.DrawLine(new Pen(lineColor), new Point(0, i * HEIGHT_LINES + INTERVAL_HEIGHT),
                                                        new Point(visibleSize.Width, i * HEIGHT_LINES + INTERVAL_HEIGHT));
            }
            videoGraphic.TranslateTransform(pnVideoEditing.AutoScrollPosition.X, pnVideoEditing.AutoScrollPosition.Y);
            //if (zoom < 0)
            //{
            //    UpdateTotalViewNrOfPixels();
            //}
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
        private void pnMenu_Paint(object sender, PaintEventArgs e)
        {
            DrawLeftSideLines();
        }

        void RefreshItemsPositionAndSize()
        {
            if (items != null && items.Count > 0)
            {
                foreach (var item in items.Values)
                {
                    item.button.Location = new Point((int)Math.Ceiling(item.startPoint / onePixelSec), item.button.Location.Y);
                    item.button.Size = new Size((int)Math.Ceiling(item.duration / onePixelSec), item.button.Size.Height);
                }
            }

        }
        private void pnVideoEditing_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string[] formats = e.Data.GetFormats();
            if (files?.Count() > 0)
            {
                foreach (var file in files)
                {
                    double duration = Duration(file);
                    string extension = Path.GetExtension(file);
                    string fileName = Path.GetFileName(file);
                    AddNewFile(file, fileName, duration, extension);
                }

            }
            else
            {
                MessageBox.Show("No files added!");
            }


        }
        void UpdateVideoEditingMaxWidth(int maxPosition)
        {
            int position = maxPosition + (int)(0.3 * maxPosition);
            if (visibleSize.Width < position)
            {
                pnVideoEditing.Width = maxPosition + (int)(0.3 * maxPosition);
            }
        }

        private void AddVideoImage(string fileName, string projectPath, string path, int duration, Button crtButton)
        {
            var wavFileName = VideoProcessing.FileNameWithoutExtension(fileName) + ".wav";
            var outputWav = projectPath + wavFileName;
            VideoProcessing.ConvertFileToWAV(path, duration, outputWav);
            Image waveImage = null;
            if (File.Exists(outputWav))
            {
                waveImage = VideoProcessing.CreateWaveImage(outputWav, (int)duration);
            }
            else
            {
                MessageBox.Show("Adding wave failed!");
            }
            List<Image> alignedImages = new List<Image>();
            int i = 0;
            while (i < duration)
            {
                Image crtImg = VideoProcessing.GetVideoTumbnail(path, i, projectPath);
                i += crtImg.Width;
                alignedImages.Add(crtImg);
            }
            int concImgWidth = alignedImages.Sum(x => x.Width);
            int concMaxHeight = alignedImages.Max(x => x.Height);
            if (waveImage != null)
            {
                concMaxHeight += waveImage.Height;
            }
            var bitmap = new Bitmap(concImgWidth, concMaxHeight);
            using (var g = Graphics.FromImage(bitmap))
            {
                int ypos = 0;
                foreach (var image in alignedImages)
                {
                    g.DrawImage(image, ypos, 0);
                    ypos += image.Width;
                }
                if (waveImage != null)
                {
                    g.DrawImage(waveImage, 0, concMaxHeight - waveImage.Height);
                }
            }
            crtButton.FlatStyle = FlatStyle.Flat;
            crtButton.BackgroundImageLayout = ImageLayout.Stretch;
            crtButton.BackgroundImage = bitmap;
        }
        private void AddNewFile(string path, string fileName, double duration, string extension)
        {
            Cursor = Cursors.WaitCursor;
            FileType fileType;
            if (Constants.SUPPORTED_VIDEO_FORMATS.Contains(extension))
            {
                fileType = FileType.Video;
            }
            else if (Constants.SUPPORTED_AUDIO_FORMATS.Contains(extension))
            {
                fileType = FileType.Audio;
            }
            else if (Constants.SUPPORTED_IMAGE_FORMATS.Contains(extension))
            {
                fileType = FileType.Image;
            }
            else
            {
                MessageBox.Show("Supported video extensions: " +
               string.Join(", ", Constants.SUPPORTED_VIDEO_FORMATS) + ".\nSupported audio extensions:" +
               string.Join(", ", Constants.SUPPORTED_AUDIO_FORMATS) + ".\nSupported image extensions: \n" +
               string.Join(", ", Constants.SUPPORTED_IMAGE_FORMATS) + ".", "File extension not supported!");
                return;
            }
            int key = 0;
            int startPos = 0;
            int grid = 1;
            if (items.Count > 0)
            {
                startPos = items.Values.Max(x => x.endPoint);
                key = items.Keys.Max() + 1;
            }
            else
            {
                key = 1;
            }
            Button crtButton = new Button();
            crtButton.Text = fileName + " : " + DurationReadeble(duration);
            crtButton.Name = path + "," + duration + "," + key;
            crtButton.Cursor = Cursors.Hand;
            crtButton.Click += Item_Button_Click;
            crtButton.MouseUp += Item_Button_MouseUp;
            crtButton.MouseDown += Item_Button_MouseDown;
            crtButton.MouseMove += Item_Button_MouseMove;
            crtButton.Size = new Size((int)(Math.Ceiling(duration / onePixelSec)), HEIGHT_LINES);
            crtButton.Location = new Point((int)Math.Ceiling(startPos / onePixelSec), (grid - 1) + INTERVAL_HEIGHT);
            UpdateVideoEditingMaxWidth(crtButton.Location.X + crtButton.Width);
            pnVideoEditing.Controls.Add(crtButton);
            //pbCursor.Location = new Point(crtButton.Location.X, pbCursor.Location.Y);
            Item item = new Item
            {
                path = path,
                fileName = fileName,
                fileType = fileType,
                type = extension,
                grid = grid,
                startPoint = crtButton.Location.X,
                endPoint = crtButton.Location.X + crtButton.Size.Width,
                startVideo = 0,
                endVideo = (int)duration,
                duration = (int)duration,
                button = crtButton,
            };
            // drawing on button info
            if (fileType == FileType.Video)
            {
                var projectPath = currentProject.projectsPath + currentProject.projectName + @"\";
                AddVideoImage(fileName, projectPath, path, (int)duration, crtButton);
            }
            // drawing audio info
            if (fileType == FileType.Audio)
            {
                if (extension != ".wav")
                {
                    var wavFileName = VideoProcessing.FileNameWithoutExtension(fileName + ".wav");
                    var output = currentProject.projectsPath + currentProject.projectName + @"\" + wavFileName;
                    VideoProcessing.ConvertFileToWAV(path, (int)duration, output);
                    Image wave = VideoProcessing.CreateWaveImage(output, (int)duration);
                    crtButton.BackgroundImageLayout = ImageLayout.Stretch;
                    crtButton.BackgroundImage = wave;
                }
                else if (extension == ".wav")
                {
                    Image wave = VideoProcessing.CreateWaveImage(path, (int)duration);
                    crtButton.BackgroundImageLayout = ImageLayout.Stretch;
                    crtButton.BackgroundImage = wave;
                }
            }
            // drawing image info
            if (fileType == FileType.Image)
            {

            }
            items.Add(key, item);
            LoadItem(key);
            currentItem = key;
            pnVideoEditing.Update();
            Cursor = Cursors.Default;
        }

        private void Item_Button_MouseMove(object sender, MouseEventArgs e)
        {
            Button crtButton = sender as Button;
            if (e.Button == MouseButtons.Left && crtButton.BackColor == DefaultBackColor)
            {
                Point position = pnVideoEditing.PointToClient(Cursor.Position);
                position = new Point(Math.Abs(position.X), Math.Abs(position.Y));
                crtButton.Location = new Point(position.X - mouseDownButtonX, position.Y - mouseDownButtonY);
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
                mouseDownButtonX = position.X - crtButton.Location.X;
                mouseDownButtonY = position.Y - crtButton.Location.Y;
                int key = GetCurrentKeyByButton(crtButton);
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
                items[key].startPoint = crtButton.Location.X;
                items[key].endPoint = crtButton.Location.X + crtButton.Width;
                currentItem = key;
                crtButton.Location = new Point(crtButton.Location.X, interval * HEIGHT_LINES + INTERVAL_HEIGHT);
            }

        }

        private void pnVideoEditing_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void pnVideoEditing_DragLeave(object sender, EventArgs e)
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
            DrawLeftSideLines();
            DrawVideoEditingLines();
        }
        private void pnVideoEditing_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Y < INTERVAL_HEIGHT)
            {
                Cursor = Cursors.SizeWE;
                //Debug.WriteLine("Mouse: " + e.Location);
                if (e.Button == MouseButtons.Left)
                {
                    if (e.X < xIntervalLocation)
                    {
                        if (maxZoomNrOfPixels >= visibleSize.Width)
                        {
                            //Debug.WriteLine("maxZoomNrOfPixels=: " + maxZoomNrOfPixels + " visibleSizeWidth: " + visibleSize.Width);
                            maxZoomNrOfPixels = visibleSize.Width;
                            xIntervalLocation = maxZoomNrOfPixels;
                            pnVideoEditing.Refresh();
                        }
                        else
                        {
                            //Debug.WriteLine("maxZoomNrOfPixels+: " + maxZoomNrOfPixels + " visibleSizeWidth: " + visibleSize.Width);
                            maxZoomNrOfPixels += xIntervalLocation - e.X;
                            xIntervalLocation = e.X;
                            pnVideoEditing.Refresh();
                        }
                    }
                    else if (e.X > xIntervalLocation)
                    {
                        //Debug.WriteLine("maxZoomNrOfPixels-: " + maxZoomNrOfPixels + " visibleSizeWidth: " + visibleSize.Width);
                        maxZoomNrOfPixels -= e.X - xIntervalLocation;
                        xIntervalLocation = e.X;
                        pnVideoEditing.Refresh();
                    }

                }
            }
            else
            {
                if (Cursor != Cursors.Default)
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void pnVideoEditing_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Y < INTERVAL_HEIGHT)
                {
                    Cursor = Cursors.SizeWE;
                    xIntervalLocation = e.X;
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
        private void FormEditor_MouseDown(object sender, MouseEventArgs e)
        {

        }
        private void FormEditor_MouseMove(object sender, MouseEventArgs e)
        {
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
            if (leftMenuGraphic != null && videoGraphic != null)
            {
                DrawLeftSideLines();
                DrawVideoEditingLines();
            }
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (axWindowsMediaPlayer1.playState == WMPPlayState.wmppsPlaying && currentItem > 0)
            {
                pbCursor.Location = new Point(items[currentItem].startPoint + (int)axWindowsMediaPlayer1.Ctlcontrols.currentPosition - Math.Abs(pnVideoEditing.AutoScrollPosition.X), pbCursor.Location.Y);
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
        int GetCurrentKeyByButton(Button btn)
        {
            return int.Parse(btn.Name.Split(',').Last());
        }
        Item GetIntersectedItem()
        {
            Button crtButton = GetIntersectedButton();
            if (crtButton == null)
            {
                return null;
            }
            int key = GetCurrentKeyByButton(crtButton);
            return items[key];
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
            var crt = items.FirstOrDefault(x => x.Value.startPoint <= pbCursor.Location.X && x.Value.endPoint >= pbCursor.Location.X);
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
                if (pbCursor.Location.X >= items[currentItem].startPoint && pbCursor.Location.X <= items[currentItem].endPoint)
                {
                    pbCursor.Location = new Point(Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).X) - 1, pbCursor.Location.Y);
                    pbCursor.Location = new Point(0, pbCursor.Location.Y);
                    axWindowsMediaPlayer1.Ctlcontrols.currentPosition = pbCursor.Location.X - items[currentItem].button.Location.X;
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                }
                else
                {
                    var crt = items.FirstOrDefault(x => x.Value.startPoint <= pbCursor.Location.X && x.Value.endPoint >= pbCursor.Location.X);
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
                        cm.MenuItems.Add("Split", new EventHandler(SplitVideo));
                    }
                    Item crtItem = GetIntersectedItem();
                    if (crtItem != null && crtItem.fileType == FileType.Video)
                    {
                        cm.MenuItems.Add("Detach Audio", new EventHandler(DetachAudio));
                    }
                    cm.MenuItems.Add("Remove", new EventHandler(RemoveFile));
                    cm.Show(this, new Point(Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).X) + pnVideoEditing.Location.X + 10, Math.Abs(pnVideoEditing.PointToClient(Cursor.Position).Y) + pnVideoEditing.Location.Y));
                }
            }
        }

        private void DetachAudio(object sender, EventArgs e)
        {
            Item intItem = GetIntersectedItem();
            var wavFileName = VideoProcessing.FileNameWithoutExtension(intItem.fileName) + ".wav";
            var output = currentProject.projectsPath + currentProject.projectName + @"\" + wavFileName;
            VideoProcessing.ConvertFileToWAV(intItem.path, (int)intItem.duration, output);
            ExtractVideoWaveFile(output, wavFileName, (int)intItem.duration,
            VideoProcessing.CreateWaveImage(output, (int)intItem.duration), ".wav", intItem.startPoint, intItem.grid + 1);
        }

        private void ExtractVideoWaveFile(string file, string fileName, double duration, Image waveImage, string extension, int startPos, int nextGrid)
        {
            if (extension == ".wav")
            {
                int key = 0;
                if (items.Count > 0)
                {
                    key = items.Keys.Max() + 1;
                }
                else
                {
                    key = 1;
                }

                Button crtButton = new Button();
                crtButton.Text = fileName + " : " + DurationReadeble(duration);
                crtButton.Name = file + "," + duration + "," + key;
                crtButton.Cursor = Cursors.Hand;
                crtButton.Click += Item_Button_Click;
                crtButton.MouseUp += Item_Button_MouseUp;
                crtButton.MouseDown += Item_Button_MouseDown;
                crtButton.MouseMove += Item_Button_MouseMove;
                crtButton.Size = new Size((int)(duration * onePixelSec), 40);
                crtButton.BackgroundImageLayout = ImageLayout.Stretch;
                crtButton.BackgroundImage = waveImage;
                pnVideoEditing.Controls.Add(crtButton);
                crtButton.Location = new Point(startPos, HEIGHT_LINES * (nextGrid - 1));
                //pbCursor.Location = new Point(crtButton.Location.X, pbCursor.Location.Y);
                Item item = new Item
                {
                    path = file,
                    fileName = fileName,
                    type = extension,
                    fileType = FileType.Audio,
                    grid = nextGrid,
                    startPoint = crtButton.Location.X,
                    endPoint = crtButton.Location.X + (int)duration,
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

        private void RemoveFile(object sender, EventArgs e)
        {
            DeleteCurrentItem();
        }

        private void SplitVideo(object sender, EventArgs e)
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

        private void FormEditor_Resize(object sender, EventArgs e)
        {
            visibleSize = new Size(this.Width - SystemInformation.FixedFrameBorderSize.Width * 2 - pnVideoEditing.Location.X - 15, this.Height - (splitHorizontal.Location.Y + splitHorizontal.SplitterDistance + this.DefaultMargin.Vertical));
            UpdateTotalViewNrOfPixels();
            pnVideoEditing.Refresh();
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
            MessageBox.Show(this, "Video Editor v" + Application.ProductVersion +
                "\nCreated by Tutorialeu.com\nAll rights Reserved!");
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

        private void hScrollBarZoom_ValueChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Scroll Value Changed: " + hScrollBarZoom.Value);
            if (stage != hScrollBarZoom.Value)
            {
                previousZoom = 51;
                UpdateTotalViewNrOfPixels();
                maxZoomNrOfPixels++;
                fragment = MIN_FRAGMENT;
                stage = hScrollBarZoom.Value;
                onePixelSec = stageIntervals[stage] / (fragment * 20d);
                RefreshItemsPositionAndSize();
                videoGraphic.Clear(pnVideoEditing.BackColor);
                pnVideoEditing.Refresh();
            }
        }

        private void lblProjectName_MouseDown(object sender, MouseEventArgs e)
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

        private void pnVideoEditing_Resize(object sender, EventArgs e)
        {
        }

        private void pnVideoEditing_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }
    }
}
