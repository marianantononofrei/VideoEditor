
namespace VideoEditor
{
    partial class FormEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditor));
            this.pnVideoEditing = new System.Windows.Forms.Panel();
            this.pbCursor = new System.Windows.Forms.PictureBox();
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.hScrollBarZoom = new System.Windows.Forms.HScrollBar();
            this.pnVideoEditing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCursor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            this.SuspendLayout();
            // 
            // pnVideoEditing
            // 
            this.pnVideoEditing.AllowDrop = true;
            this.pnVideoEditing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pnVideoEditing.AutoScroll = true;
            this.pnVideoEditing.AutoScrollMargin = new System.Drawing.Size(10, 0);
            this.pnVideoEditing.Controls.Add(this.pbCursor);
            this.pnVideoEditing.Location = new System.Drawing.Point(1, 393);
            this.pnVideoEditing.Name = "pnVideoEditing";
            this.pnVideoEditing.Size = new System.Drawing.Size(1325, 125);
            this.pnVideoEditing.TabIndex = 0;
            this.pnVideoEditing.Scroll += new System.Windows.Forms.ScrollEventHandler(this.pnVideoEditing_Scroll);
            this.pnVideoEditing.Click += new System.EventHandler(this.pnVideoEditing_Click);
            this.pnVideoEditing.DragDrop += new System.Windows.Forms.DragEventHandler(this.pnVideoEditing_DragDrop);
            this.pnVideoEditing.DragEnter += new System.Windows.Forms.DragEventHandler(this.pnVideoEditing_DragEnter);
            this.pnVideoEditing.DragLeave += new System.EventHandler(this.pnVideoEditing_DragLeave);
            this.pnVideoEditing.Paint += new System.Windows.Forms.PaintEventHandler(this.pnVideoEditing_Paint);
            this.pnVideoEditing.Enter += new System.EventHandler(this.pnVideoEditing_Enter);
            this.pnVideoEditing.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnVideoEditing_MouseDown);
            this.pnVideoEditing.MouseEnter += new System.EventHandler(this.pnVideoEditing_MouseEnter);
            this.pnVideoEditing.MouseHover += new System.EventHandler(this.pnVideoEditing_MouseHover);
            this.pnVideoEditing.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnVideoEditing_MouseMove);
            this.pnVideoEditing.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnVideoEditing_MouseUp);
            this.pnVideoEditing.Resize += new System.EventHandler(this.pnVideoEditing_Resize);
            // 
            // pbCursor
            // 
            this.pbCursor.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pbCursor.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.pbCursor.Location = new System.Drawing.Point(146, 24);
            this.pbCursor.Name = "pbCursor";
            this.pbCursor.Size = new System.Drawing.Size(4, 50);
            this.pbCursor.TabIndex = 0;
            this.pbCursor.TabStop = false;
            this.pbCursor.Click += new System.EventHandler(this.pbCursor_Click);
            this.pbCursor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbCursor_MouseDown);
            this.pbCursor.MouseHover += new System.EventHandler(this.pbCursor_MouseHover);
            this.pbCursor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbCursor_MouseMove);
            this.pbCursor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbCursor_MouseUp);
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(732, 0);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(594, 351);
            this.axWindowsMediaPlayer1.TabIndex = 2;
            this.axWindowsMediaPlayer1.MouseDownEvent += new AxWMPLib._WMPOCXEvents_MouseDownEventHandler(this.axWindowsMediaPlayer1_MouseDownEvent);
            this.axWindowsMediaPlayer1.MouseMoveEvent += new AxWMPLib._WMPOCXEvents_MouseMoveEventHandler(this.axWindowsMediaPlayer1_MouseMoveEvent);
            this.axWindowsMediaPlayer1.Enter += new System.EventHandler(this.axWindowsMediaPlayer1_Enter);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // hScrollBarZoom
            // 
            this.hScrollBarZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hScrollBarZoom.Location = new System.Drawing.Point(1237, 380);
            this.hScrollBarZoom.Name = "hScrollBarZoom";
            this.hScrollBarZoom.Size = new System.Drawing.Size(82, 10);
            this.hScrollBarZoom.TabIndex = 1;
            // 
            // FormEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1328, 520);
            this.Controls.Add(this.hScrollBarZoom);
            this.Controls.Add(this.axWindowsMediaPlayer1);
            this.Controls.Add(this.pnVideoEditing);
            this.KeyPreview = true;
            this.Name = "FormEditor";
            this.Text = "FormEditor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ClientSizeChanged += new System.EventHandler(this.FormEditor_ClientSizeChanged);
            this.SizeChanged += new System.EventHandler(this.FormEditor_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormEditor_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormEditor_KeyPress);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormEditor_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormEditor_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormEditor_MouseUp);
            this.pnVideoEditing.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCursor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnVideoEditing;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.PictureBox pbCursor;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.HScrollBar hScrollBarZoom;
    }
}

