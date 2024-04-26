using Microsoft.VisualBasic;
using SDDBrowser;
using SDDPopup;
using SDDTabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.ApplicationModel.Background;
using Windows.UI.ViewManagement;


namespace SDDWebBrowser
{
    public partial class Main : Form
    {
        const int resizeWidth = 3;
        const int edgeResizeWidth = 50;

        public Main()
        {
            InitializeComponent();
            ContentPanel contentPanel = new ContentPanel(this, "top");
            contentPanels.Add(contentPanel);
            contentPanel.GenerateNewTab(SDDBrowser.ContentPanel.defaultURL);
            newTabBtn.Click += new EventHandler(contentPanel.NewTabBtn_Click);
        }

        public List<string> urlDomains;
        readonly List<ContentPanel> contentPanels = new List<ContentPanel>();
        Trigger[] triggers;
        List<Trigger> triggerAreas;
        bool isMergingToApp = false;
        readonly public List<Action> needsHandle = new List<Action>();
        Main appMergingTo;
        string positionMergingTo;
        long lastFocused = 0;
        public bool isDead;
        Size lastSize;
        Rectangle lastContentSize;
        bool loaded;
        protected bool isDragging = false;
        bool wasFullScreen = false;
        protected Rectangle lastRectangle = new Rectangle();
        readonly List<Control> edges = new List<Control>();
        UISettings uiSettings;
        public Color accentColor;
        public Color backColor;
        public Color foreColor;
        string edgeSnap = "None";
        Size oldSize;
        bool snapped = false;
        readonly Dictionary<string, Rectangle> positionArea = new Dictionary<string, Rectangle>();
        readonly Dictionary<(ContentPanel, ContentPanel), Control> bordersCreated = new Dictionary<(ContentPanel, ContentPanel), Control>();
        Control contentSpace;
        private const int minimumWidthOfContentPanel = 420;
        private const int minimumHeightOfContentPanel = 328;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool BlockInput(bool fBlockIt);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_MOVE = 0x01;
        public enum WMessages : int
        {
            WM_LBUTTONDOWN = 0x201, //Left mousebutton down
            WM_LBUTTONUP = 0x202,   //Left mousebutton up
            WM_LBUTTONDBLCLK = 0x203, //Left mousebutton doubleclick
            WM_RBUTTONDOWN = 0x204, //Right mousebutton down
            WM_RBUTTONUP = 0x205,   //Right mousebutton up
            WM_RBUTTONDBLCLK = 0x206, //Right mousebutton do
        }

        public void RemoveControl(Control control)
        {
            Controls.Remove(control);
        }

        public void AddControl(Control control)
        {
            Controls.Add(control);
        }

        // window
        private void Main_Load(object sender, EventArgs e)
        {
            Console.WriteLine("started new app");
            Activated += new EventHandler(Form_gotFocus);
            lastFocused = DateTime.UtcNow.Ticks;

            GetColours();

            StreamReader reader = File.OpenText("domain_names.csv");
            urlDomains = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                urlDomains.Add(values[0]);
            }

            lastSize = Size;
            loaded = true;

            InitialiseFormEdge();
            InitialiseTriggers();
            AddButtonHover();
            lastContentSize = contentSpace.Bounds;


            foreach (ContentPanel contentPanel in contentPanels)
            {
                //addToolStripsForPanel(contentPanel);
            }
        }

        private void AddButtonHover()
        {
            foreach (Control control in GetChildControls(this))
            {
                if (control.GetType() == typeof(Button))
                {
                    AddHoverColorToButton((Button)control);
                }
            }
        }

        private void AddToolStripsForPanel(ContentPanel contentPanel)
        {
            ToolStripMenuItem importItem = new ToolStripMenuItem
            {
                Text = contentPanel.position,
                Name = contentPanel.position + "Button"
            };
            importItem.Click += delegate (object sent, EventArgs eventargs) { contentPanel.ImportBookmarks(); };
            ImportButton.DropDownItems.Add(importItem);
            ToolStripMenuItem exportItem = new ToolStripMenuItem
            {
                Text = contentPanel.position,
                Name = contentPanel.position + "Button"
            };
            exportItem.Click += delegate (object sent, EventArgs eventargs) { contentPanel.ExportBookmarks(); };
            ExportButton.DropDownItems.Add(exportItem);
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            foreach (Action action in needsHandle)
            {
                action();
            }
        }

        private void Form_gotFocus(object sender, EventArgs e)
        {
            lastFocused = DateTime.UtcNow.Ticks;
            Console.WriteLine($"{Text} Got Focus: {lastFocused}");
        }

        // https://stackoverflow.com/questions/22780571/scale-windows-forms-window
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (loaded)
            {
                ResizeContentPanels(Size);
                RepositionWidthPosition(CloseButton, Size);
                RepositionWidthPosition(MaximiseButton, Size);
                RepositionWidthPosition(MinimiseButton, Size);
                lastSize = Size;
                if (WindowState == FormWindowState.Maximized)
                {
                    foreach (Control edge in edges)
                    {
                        edge.Enabled = false;
                        edge.Visible = false;
                    }
                }
                else
                {
                    foreach (Control edge in edges)
                    {
                        edge.Enabled = true;
                        edge.Visible = true;
                    }
                }
                UpdateTriggers();
                ResizeBorders();
                foreach (Control border in bordersCreated.Values)
                {
                    border.BringToFront();
                }
            }
        }

        private void ResizeBorders()
        {
            foreach ((ContentPanel, ContentPanel) pair in bordersCreated.Keys)
            {
                Control border = bordersCreated[pair];
                Controls.Remove(border);
            }
            bordersCreated.Clear();
            foreach (ContentPanel contentPanel in contentPanels)
            {
                AddEdgesToContentPanel(contentPanel);
            }
        }

        private void ResizeContentPanels(Size newSize)
        {
            ResizeWidthControl(contentSpace, newSize);
            ResizeHeightControl(contentSpace, newSize);
            foreach (ContentPanel panel in contentPanels)
            {
                Rectangle rectangle = panel.LastBounds;
                Rectangle size = new Rectangle();
                float xScale = (float)contentSpace.Width / (float)lastContentSize.Width;
                float yScale = (float)contentSpace.Height / (float)lastContentSize.Height;
                size.X = (int)((rectangle.X - contentSpace.Left) * xScale) + contentSpace.Left;
                size.Y = (int)((rectangle.Y - contentSpace.Top) * yScale) + contentSpace.Top;
                size.Width = (int)(rectangle.Width * xScale);
                size.Height = (int)(rectangle.Height * yScale);
                panel.SetSizeAndPosition(size);
            }
        }

        internal void ResizeWidthControl(Control control, Size newSize)
        {
            int width = newSize.Width - lastSize.Width;
            control.Width += width;
        }

        internal void ResizeHeightControl(Control control, Size newSize)
        {
            int height = newSize.Height - lastSize.Height;
            control.Height += height;
        }

        internal void RepositionWidthPosition(Control control, Size newSize)
        {
            int width = newSize.Width - lastSize.Width;
            control.Left += width;
        }

        protected void InitialiseFormEdge()
        {
            Color borderColor = accentColor;
            MouseDown += new MouseEventHandler(Form_MouseDown);
            MouseMove += new MouseEventHandler(Form_MouseMove);
            MouseUp += new MouseEventHandler(Form_MouseUp);
            contentSpace = new Control()
            {
                Location = ContentHeader.Location,
                Size = new Size(ContentHeader.Width, ContentHeader.Height + TabsPanel.Height + ContentPanel.Height)
            };

            // bottom
            UserControl bottomEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
                Height = resizeWidth,
                Width = DisplayRectangle.Width - (resizeWidth * 2),
                Left = resizeWidth,
                Top = DisplayRectangle.Height - resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeNS,
                Name = "bottom"
            };
            bottomEdge.MouseDown += Form_MouseDown;
            bottomEdge.MouseUp += ControlMouseUp;
            bottomEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    Size = new Size(lastRectangle.Width, e.Y - lastRectangle.Y + Height);
                }
            };
            bottomEdge.BringToFront();
            edges.Add(bottomEdge);
            Controls.Add(bottomEdge);

            // right
            UserControl rightEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom),
                Height = DisplayRectangle.Height - (resizeWidth * 2),
                Width = resizeWidth,
                Left = DisplayRectangle.Width - resizeWidth,
                Top = resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeWE,
                Name = "right"
            };
            rightEdge.MouseDown += Form_MouseDown;
            rightEdge.MouseUp += ControlMouseUp;
            rightEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    Size = new Size(e.X - lastRectangle.X + Width, lastRectangle.Height);
                }
            };
            rightEdge.BringToFront();
            edges.Add(rightEdge);
            Controls.Add(rightEdge);

            // bottom-right
            UserControl bottomRightEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = DisplayRectangle.Width - resizeWidth,
                Top = DisplayRectangle.Height - resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeNWSE,
                Name = "bottomRight"
            };
            bottomRightEdge.MouseDown += Form_MouseDown;
            bottomRightEdge.MouseUp += ControlMouseUp;
            bottomRightEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    Size = new Size((e.X - lastRectangle.X + Width), (e.Y - lastRectangle.Y + Height));
                }
            };
            bottomRightEdge.BringToFront();
            edges.Add(bottomRightEdge);
            Controls.Add(bottomRightEdge);

            // top-right
            UserControl topRightEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Right),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = DisplayRectangle.Width - resizeWidth,
                Top = 0,
                BackColor = borderColor,
                Cursor = Cursors.SizeNESW,
                Name = "topRight"
            };
            topRightEdge.MouseDown += Form_MouseDown;
            topRightEdge.MouseUp += ControlMouseUp;
            topRightEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.Y - lastRectangle.Y);
                    int y = (Location.Y + diff);
                    int new_height = Height - diff;
                    if (!(diff > 0 && MinimumSize.Height == Height))
                    {
                        Location = new Point(Location.X, y);
                    }
                    else
                    {
                        new_height = Height;
                    }
                    Size = new Size(e.X - lastRectangle.X + Width, new_height);
                }
            };
            topRightEdge.BringToFront();
            edges.Add(topRightEdge);
            Controls.Add(topRightEdge);

            // top
            UserControl topEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
                Height = resizeWidth,
                Width = DisplayRectangle.Width - (resizeWidth * 2),
                Left = resizeWidth,
                Top = 0,
                BackColor = borderColor,
                Cursor = Cursors.SizeNS,
                Name = "top"
            };
            topEdge.MouseDown += Form_MouseDown;
            topEdge.MouseUp += ControlMouseUp;
            topEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.Y - lastRectangle.Y);
                    int y = (Location.Y + diff);

                    if (!(diff > 0 && MinimumSize.Height == Height))
                    {
                        Location = new Point(Location.X, y);
                        Size = new Size(lastRectangle.Width, Height - diff);
                    }
                }
            };
            topEdge.BringToFront();
            edges.Add(topEdge);
            Controls.Add(topEdge);

            // left
            UserControl leftEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom),
                Height = DisplayRectangle.Height - (resizeWidth * 2),
                Width = resizeWidth,
                Left = 0,
                Top = resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeWE,
                Name = "left"
            };
            leftEdge.MouseDown += Form_MouseDown;
            leftEdge.MouseUp += ControlMouseUp;
            leftEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.X - lastRectangle.X);
                    int x = (Location.X + diff);

                    if (!(diff > 0 && MinimumSize.Width == Width))
                    {
                        Location = new Point(x, Location.Y);
                        Size = new Size((Width - diff), Height);
                    }

                }
            };
            leftEdge.BringToFront();
            edges.Add(leftEdge);
            Controls.Add(leftEdge);

            // bottom-left
            UserControl bottomLeftEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Left),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = 0,
                Top = DisplayRectangle.Height - resizeWidth,
                BackColor = borderColor,
                Cursor = Cursors.SizeNESW,
                Name = "bottomLeft"
            };
            bottomLeftEdge.MouseDown += Form_MouseDown;
            bottomLeftEdge.MouseUp += ControlMouseUp;
            bottomLeftEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int diff = (e.Location.X - lastRectangle.X);
                    int x = (Location.X + diff);

                    int new_width = Width - diff;
                    if (!(diff > 0 && MinimumSize.Width == Width))
                    {
                        Location = new Point(x, Location.Y);
                    }
                    else
                    {
                        new_width = Width;
                    }
                    Size = new Size(new_width, (e.Y - lastRectangle.Y + Height));
                }
            };
            bottomLeftEdge.BringToFront();
            edges.Add(bottomLeftEdge);
            Controls.Add(bottomLeftEdge);

            // top-left
            UserControl topLeftEdge = new UserControl()
            {
                Anchor = (AnchorStyles.Top | AnchorStyles.Left),
                Height = resizeWidth,
                Width = resizeWidth,
                Left = 0,
                Top = 0,
                BackColor = borderColor,
                Cursor = Cursors.SizeNWSE,
                Name = "topLeft"
            };
            topLeftEdge.MouseDown += Form_MouseDown;
            topLeftEdge.MouseUp += ControlMouseUp;
            topLeftEdge.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (isDragging && WindowState == FormWindowState.Normal)
                {
                    int dX = (e.Location.X - lastRectangle.X);
                    if (dX > 0 && MinimumSize.Width == Width)
                    {
                        dX = 0;

                    }
                    int dY = (e.Location.Y - lastRectangle.Y);
                    if (dY > 0 && MinimumSize.Height == Height)
                    {
                        dY = 0;

                    }
                    int x = (this.Location.X + dX);
                    int y = (this.Location.Y + dY);


                    this.Location = new Point(x, y);
                    this.Size = new Size((this.Width - dX), (this.Height - dY));
                }

            };
            topLeftEdge.BringToFront();
            edges.Add(topLeftEdge);
            this.Controls.Add(topLeftEdge);
        }

        private void UpdateTriggers()
        {
            int triggerWidth = contentSpace.Location.X;
            int triggerHeight = contentSpace.Location.Y;
            triggers = new Trigger[3] {
                new Trigger("left",
                new Rectangle(0, 0, triggerWidth, Height), this),
                new Trigger("right",
                new Rectangle(DisplayRectangle.Width - triggerWidth, 0, triggerWidth, Height), this),
                new Trigger("bottom",
                new Rectangle(triggerWidth, DisplayRectangle.Height - triggerWidth, Width - 2 * triggerWidth, triggerHeight), this)
            };
            triggerAreas = triggers.ToList();
            foreach (ContentPanel contentPanel in contentPanels)
            {
                triggerAreas.Add(ControlToTrigger(contentPanel.TabsPanel, contentPanel.position));
            }
        }

        private Trigger ControlToTrigger(Control control, string position)
        {
            Point location = PointToScreen(control.Location);
            Rectangle rectangle = new Rectangle(location.X - Left, location.Y - Top, control.Width, control.Height);
            return new Trigger(position, rectangle, this);
        }

        private void InitialiseTriggers()
        {
            positionArea.Add("left", new Rectangle(0, 0, 1, 2));
            positionArea.Add("top", new Rectangle(1, 0, 1, 1));
            positionArea.Add("right", new Rectangle(2, 0, 1, 2));
            positionArea.Add("bottom", new Rectangle(1, 1, 1, 1));
            UpdateTriggers();
        }


        public void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastRectangle = new Rectangle(e.Location.X, e.Location.Y, this.Width, this.Height);
                //Location = new Point(0, 0);
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (snapped && isDragging)
            {
                snapped = false;
                Size = oldSize;
                Form_MouseDown(sender, e);
            }
            else if (wasFullScreen && isDragging)
            {
                Form_MouseDown(sender, e);
                wasFullScreen = false;
            }
            if (isDragging && WindowState == FormWindowState.Normal)
            {
                DragControl(this, e, lastRectangle);
                UpdateMergingApp();
            }
            else if (isDragging && WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                Location = new Point(MousePosition.X - Width / 2, MousePosition.Y);
                wasFullScreen = true;
            }
        }

        private void DragControl(Control control, MouseEventArgs e, Rectangle rectangle)
        {
            int x = (control.Location.X + (e.Location.X - rectangle.X));
            int y = (control.Location.Y + (e.Location.Y - rectangle.Y));

            control.Location = new Point(x, y);
        }

        delegate void nearEdgeFunction(Rectangle rectangle);
        private void CheckEdgeResizing(nearEdgeFunction edgeFunction)
        {
            bool[] nearEdges = new bool[4];
            Rectangle screenbounds = Screen.GetWorkingArea(MousePosition);
            Point screenMousePosition = new Point(MousePosition.X - screenbounds.X, MousePosition.Y - screenbounds.Y);
            nearEdges[0] = screenMousePosition.X < edgeResizeWidth;
            nearEdges[1] = screenMousePosition.Y < edgeResizeWidth;
            nearEdges[2] = screenMousePosition.X > screenbounds.Width - edgeResizeWidth;
            nearEdges[3] = screenMousePosition.Y > screenbounds.Height - edgeResizeWidth;
            bool[] topLeft = { true, true, false, false };
            bool[] topRight = { false, true, true, false };
            bool[] bottomLeft = { true, false, false, true };
            bool[] bottomRight = { false, false, true, true };
            bool[] left = { true, false, false, false };
            bool[] top = { false, true, false, false };
            bool[] right = { false, false, true, false };
            bool[] bottom = { false, false, false, true };
            Rectangle rectangle;

            if (nearEdges.SequenceEqual(topLeft))
            {
                edgeSnap = "topLeft";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y, screenbounds.Width / 2, screenbounds.Height / 2);
                edgeFunction(rectangle);

            }
            else if (nearEdges.SequenceEqual(topRight))
            {
                edgeSnap = "topRight";
                rectangle = new Rectangle(screenbounds.X + screenbounds.Width / 2, screenbounds.Y, screenbounds.Width / 2, screenbounds.Height / 2);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(bottomLeft))
            {
                edgeSnap = "bottomLeft";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y + screenbounds.Height / 2, screenbounds.Width / 2, screenbounds.Height / 2);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(bottomRight))
            {
                edgeSnap = "bottomRight";
                rectangle = new Rectangle(screenbounds.X + screenbounds.Width / 2, screenbounds.Y + screenbounds.Height / 2, screenbounds.Width / 2, screenbounds.Height / 2);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(left))
            {
                edgeSnap = "Left";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y, screenbounds.Width / 2, screenbounds.Height);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(right))
            {
                edgeSnap = "Right";
                rectangle = new Rectangle(screenbounds.X + screenbounds.Width / 2, screenbounds.Y, screenbounds.Width / 2, screenbounds.Height);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(top))
            {
                edgeSnap = "top";
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y, screenbounds.Width, screenbounds.Height);
                edgeFunction(rectangle);
            }
            else if (nearEdges.SequenceEqual(bottom))
            {
                edgeSnap = "Bottom";
            }
            else
            {
                edgeSnap = "None";
            }
            //Console.WriteLine(edgeSnap);
            //Console.WriteLine("{0,1}, {1,1}, {2,1}, {3,1}", nearEdges[0], nearEdges[1], nearEdges[2], nearEdges[3]);
        }

        private void ControlMouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void MergeContentPanel()
        {
            List<ContentPanel> Panels = new List<ContentPanel>();
            Panels.AddRange(contentPanels);
            foreach (ContentPanel panelMergingFrom in Panels)
            {
                List<Tab> tabs = panelMergingFrom.tabs;
                ContentPanel panelMergingTo = appMergingTo.contentPanels.Find(p => p.position == positionMergingTo);
                if (panelMergingTo is null)
                {
                    panelMergingTo = appMergingTo.contentPanels.Find(p => p.TabsPanel.Name == positionMergingTo);
                    if (panelMergingTo is null)
                    {
                        panelMergingTo = appMergingTo.NewContentPanel(positionMergingTo);
                    }
                }

                foreach (Tab tab in tabs)
                {
                    TransferTabEventsForPanel(tab, panelMergingFrom, panelMergingTo);
                }
                panelMergingTo.ExtendTabs(tabs);
            }
        }

        private ContentPanel NewContentPanel(string position)
        {
            ContentPanel content = new ContentPanel(this, position);
            contentPanels.Add(content);
            //addToolStripsForPanel(content);
            content.GenerateAllPanels(contentSpace.Bounds);
            GenerateContentPanelBounds();

            ResizeBorders();

            lastContentSize = contentSpace.Bounds;
            //OnResize(new EventArgs());
            UpdateTriggers();
            foreach (Control border in bordersCreated.Values)
            {
                border.BringToFront();
            }
            return content;
        }

        internal void RemoveContentPanel(ContentPanel contentPanel)
        {
            contentPanels.Remove(contentPanel);
            GenerateContentPanelBounds();
            ResizeBorders();
            lastContentSize = contentSpace.Bounds;
            UpdateTriggers();
            foreach (Control border in bordersCreated.Values)
            {
                border.BringToFront();
            }
        }

        private void GenerateContentPanelBounds()
        {
            List<Rectangle> rectangles = GetRectangles();
            Size table = GetTablesize();
            Size gridSize = GetGridSize();
            foreach (ContentPanel contentPanel in contentPanels)
            {
                if (positionArea.TryGetValue(contentPanel.position, out Rectangle rectangle))
                {
                    int rightExtension = ExtendRectangleWidthRight(rectangle.X + rectangle.Width, rectangle.Y, table.Width, rectangle.Height, rectangles);
                    int downExtension = ExtendRectangleHeightDown(rectangle.Y + rectangle.Height, rectangle.X, table.Height, rectangle.Width + rightExtension, rectangles);
                    int leftExtension = ExtendRectangleWidthLeft(rectangle.X + rectangle.Width, rectangle.Y, rectangle.Height, rectangles);
                    int upExtension = ExtendRectangleHeightUp(rectangle.Y + rectangle.Height, rectangle.X, rectangle.Width + rightExtension, rectangles);
                    rectangle.X = (rectangle.X - leftExtension) * gridSize.Width + contentSpace.Left;
                    rectangle.Y = (rectangle.Y - upExtension) * gridSize.Height + contentSpace.Top;
                    rectangle.Width = (rectangle.Width + rightExtension + leftExtension) * gridSize.Width;
                    rectangle.Height = (rectangle.Height + downExtension + upExtension) * gridSize.Height;
                    contentPanel.LastBounds = rectangle;
                    contentPanel.SetSizeAndPosition(rectangle);
                }
            }
        }

        private void AddEdgesToContentPanel(ContentPanel contentPanel)
        {
            Rectangle rectangle = contentPanel.Bounds;
            Point[] points = new Point[4];
            points[0] = new Point(rectangle.Left - 1, rectangle.Top);
            points[1] = new Point(rectangle.Right + 1, rectangle.Top);
            points[2] = new Point(rectangle.Left, rectangle.Top - 1);
            points[3] = new Point(rectangle.Left, rectangle.Bottom + 1);

            //left
            ContentPanel leftNeighbour = contentPanels.Find(c => PointInRectangle(points[0], c.Bounds) && c != contentPanel);
            if (leftNeighbour != null)
            {
                Rectangle leftNeighbourRect = leftNeighbour.Bounds;
                (ContentPanel, ContentPanel) contentPair = (contentPanel, leftNeighbour);
                (ContentPanel, ContentPanel) contentPairReverse = (leftNeighbour, contentPanel);
                if ((!bordersCreated.Keys.Contains(contentPair)) && (!bordersCreated.Keys.Contains(contentPairReverse)))
                {
                    bool sharedBorder = false;
                    (ContentPanel, ContentPanel) pair = contentPair;
                    Control b2 = new Control();
                    foreach ((ContentPanel c1, ContentPanel c2) in bordersCreated.Keys)
                    {
                        if ((c1 == leftNeighbour && bordersCreated[(c1, c2)].Name == "Right") || (c2 == leftNeighbour && bordersCreated[(c1, c2)].Name == "Left"))
                        {
                            pair = (c1, c2);
                            b2 = bordersCreated[pair];
                            ContentPanel sharedNeighbour;
                            if (c1 == leftNeighbour)
                            {
                                sharedNeighbour = c2;
                            }
                            else
                            {
                                sharedNeighbour = c1;
                            }

                            sharedBorder = true;
                            b2.MouseMove += delegate (object sender, MouseEventArgs e)
                            {
                                if (isDragging)
                                {
                                    contentPanel.SetSizeAndPosition(new Rectangle(sharedNeighbour.Bounds.Left, contentPanel.Bounds.Top, sharedNeighbour.Bounds.Width, contentPanel.Bounds.Height));
                                    SetLastBounds();
                                    IEnumerable<Control> otherBorders = bordersCreated.Keys.Where(c => c.Item1 == contentPanel || c.Item2 == contentPanel).Select(c => bordersCreated[c]);
                                    foreach (Control b in otherBorders)
                                    {
                                        if (b.Width != resizeWidth)
                                        {
                                            b.Width = sharedNeighbour.Bounds.Width;
                                            b.Location = new Point(sharedNeighbour.Bounds.Left, b.Location.Y);
                                        }
                                    }
                                }
                            };
                            b2.Left = Math.Min(rectangle.Left - resizeWidth / 2, b2.Left);
                            b2.Top = Math.Min(rectangle.Top, b2.Top);
                            b2.Height += rectangle.Height;
                            b2.BringToFront();
                        }
                    }
                    if (sharedBorder)
                    {
                        bordersCreated[pair] = b2;
                    }
                    else
                    {
                        UserControl border = new UserControl()
                        {
                            Left = rectangle.Left - resizeWidth / 2,
                            Top = rectangle.Top,
                            Height = rectangle.Height,
                            Width = resizeWidth,
                            BackColor = accentColor,
                            Cursor = Cursors.SizeWE,
                            Name = "Left"
                        };
                        border.MouseDown += Form_MouseDown;
                        border.MouseUp += ControlMouseUp;
                        border.MouseMove += delegate (object sender, MouseEventArgs e)
                        {
                            if (isDragging)
                            {
                                int diff = (e.Location.X - lastRectangle.X);
                                leftNeighbourRect = leftNeighbour.Bounds;

                                if (minimumWidthOfContentPanel < (rectangle.Width - diff) && minimumWidthOfContentPanel < (leftNeighbourRect.Width + diff))
                                {
                                    rectangle.Width -= diff;
                                    rectangle.X += diff;
                                    leftNeighbourRect.Width += diff;
                                    contentPanel.SetSizeAndPosition(rectangle);
                                    leftNeighbour.SetSizeAndPosition(leftNeighbourRect);
                                    SetLastBounds();
                                    border.Location = new Point(border.Location.X + diff, border.Location.Y);
                                }
                            }
                        };
                        border.BringToFront();
                        bordersCreated[contentPair] = border;
                        Controls.Add(border);
                    }
                }
            }

            //right
            ContentPanel rightNeighbour = contentPanels.Find(c => PointInRectangle(points[1], c.Bounds) && c != contentPanel);
            if (rightNeighbour != null)
            {

                Rectangle rightNeighbourRect = rightNeighbour.Bounds;
                (ContentPanel, ContentPanel) contentPair = (contentPanel, rightNeighbour);
                (ContentPanel, ContentPanel) contentPairReverse = (rightNeighbour, contentPanel);
                if ((!bordersCreated.Keys.Contains(contentPair)) && (!bordersCreated.Keys.Contains(contentPairReverse)))
                {
                    bool sharedBorder = false;
                    (ContentPanel, ContentPanel) pair = contentPair;
                    Control b2 = new Control();
                    foreach ((ContentPanel c1, ContentPanel c2) in bordersCreated.Keys)
                    {
                        if ((c1 == rightNeighbour && bordersCreated[(c1, c2)].Name == "Left") || (c2 == rightNeighbour && bordersCreated[(c1, c2)].Name == "Right"))
                        {
                            pair = (c1, c2);
                            b2 = bordersCreated[pair];
                            ContentPanel sharedNeighbour;
                            if (c1 == rightNeighbour)
                            {
                                sharedNeighbour = c2;
                            }
                            else
                            {
                                sharedNeighbour = c1;
                            }

                            sharedBorder = true;
                            b2.MouseMove += delegate (object sender, MouseEventArgs e)
                            {
                                if (isDragging)
                                {
                                    contentPanel.SetSizeAndPosition(new Rectangle(sharedNeighbour.Bounds.Left, contentPanel.Bounds.Top, sharedNeighbour.Bounds.Width, contentPanel.Bounds.Height));
                                    SetLastBounds();
                                    IEnumerable<Control> otherBorders = bordersCreated.Keys.Where(c => c.Item1 == contentPanel || c.Item2 == contentPanel).Select(c => bordersCreated[c]);
                                    foreach (Control b in otherBorders)
                                    {
                                        if (b.Width != resizeWidth)
                                        {
                                            b.Width = sharedNeighbour.Bounds.Width;
                                        }
                                    }
                                }
                            };
                            b2.Left = Math.Min(rectangle.Right - resizeWidth / 2, b2.Left);
                            b2.Top = Math.Min(rectangle.Top, b2.Top);
                            b2.Height += rectangle.Height;
                            b2.BringToFront();
                        }
                    }
                    if (sharedBorder)
                    {
                        bordersCreated[pair] = b2;
                    }
                    else
                    {
                        UserControl border = new UserControl()
                        {
                            Left = rectangle.Right - resizeWidth / 2,
                            Top = rectangle.Top,
                            Height = rectangle.Height,
                            Width = resizeWidth,
                            BackColor = accentColor,
                            Cursor = Cursors.SizeWE,
                            Name = "Right"
                        };
                        border.MouseDown += Form_MouseDown;
                        border.MouseUp += ControlMouseUp;
                        border.MouseMove += delegate (object sender, MouseEventArgs e)
                        {
                            if (isDragging)
                            {
                                int diff = (e.Location.X - lastRectangle.X);
                                rightNeighbourRect = rightNeighbour.Bounds;

                                if (minimumWidthOfContentPanel < (rectangle.Width + diff) && minimumWidthOfContentPanel < (rightNeighbourRect.Width - diff))
                                {
                                    rectangle.Width += diff;
                                    rightNeighbourRect.X += diff;
                                    rightNeighbourRect.Width -= diff;
                                    contentPanel.SetSizeAndPosition(rectangle);
                                    rightNeighbour.SetSizeAndPosition(rightNeighbourRect);
                                    SetLastBounds();
                                    border.Location = new Point(border.Location.X + diff, border.Location.Y);
                                }
                            }
                        };
                        bordersCreated[contentPair] = border;
                        Controls.Add(border);
                        border.BringToFront();
                    }
                }
            }

            //top
            ContentPanel topNeighbour = contentPanels.Find(c => PointInRectangle(points[2], c.Bounds) && c != contentPanel);
            if (topNeighbour != null)
            {
                Rectangle topNeighbourRect = topNeighbour.Bounds;
                (ContentPanel, ContentPanel) contentPair = (contentPanel, topNeighbour);
                (ContentPanel, ContentPanel) contentPairReverse = (topNeighbour, contentPanel);
                if ((!bordersCreated.Keys.Contains(contentPair)) && (!bordersCreated.Keys.Contains(contentPairReverse)))
                {
                    bool sharedBorder = false;
                    (ContentPanel, ContentPanel) pair = contentPair;
                    Control b2 = new Control();
                    foreach ((ContentPanel c1, ContentPanel c2) in bordersCreated.Keys)
                    {
                        if ((c1 == topNeighbour && bordersCreated[(c1, c2)].Name == "Bottom") || (c2 == topNeighbour && bordersCreated[(c1, c2)].Name == "Top"))
                        {
                            pair = (c1, c2);
                            b2 = bordersCreated[pair];
                            ContentPanel sharedNeighbour;
                            if (c1 == topNeighbour)
                            {
                                sharedNeighbour = c2;
                            }
                            else
                            {
                                sharedNeighbour = c1;
                            }

                            sharedBorder = true;
                            b2.MouseMove += delegate (object sender, MouseEventArgs e)
                            {
                                if (isDragging)
                                {
                                    contentPanel.SetSizeAndPosition(new Rectangle(contentPanel.Bounds.Left, sharedNeighbour.Bounds.Top, contentPanel.Bounds.Width, sharedNeighbour.Bounds.Height));
                                    SetLastBounds();
                                    IEnumerable<Control> otherBorders = bordersCreated.Keys.Where(c => c.Item1 == contentPanel || c.Item2 == contentPanel).Select(c => bordersCreated[c]);
                                    foreach (Control b in otherBorders)
                                    {
                                        if (b.Width != resizeWidth)
                                        {
                                            b.Height = sharedNeighbour.Bounds.Height;
                                            b.Location = new Point(b.Location.X, sharedNeighbour.Bounds.Top);
                                        }
                                    }
                                }
                            };
                            b2.Left = Math.Min(rectangle.Left, b2.Left);
                            b2.Top = Math.Min(rectangle.Top - resizeWidth / 2, b2.Top);
                            b2.Width += rectangle.Width;
                            b2.BringToFront();
                        }
                    }
                    if (sharedBorder)
                    {
                        bordersCreated[pair] = b2;
                    }
                    else
                    {
                        UserControl border = new UserControl()
                        {
                            Left = rectangle.Left,
                            Top = rectangle.Top - resizeWidth / 2,
                            Height = resizeWidth,
                            Width = rectangle.Width,
                            BackColor = accentColor,
                            Cursor = Cursors.SizeNS,
                            Name = "Top"
                        };
                        border.MouseDown += Form_MouseDown;
                        border.MouseUp += ControlMouseUp;
                        border.MouseMove += delegate (object sender, MouseEventArgs e)
                        {
                            if (isDragging)
                            {
                                topNeighbourRect = topNeighbour.Bounds;
                                int diff = (e.Location.Y - lastRectangle.Y);

                                if (minimumHeightOfContentPanel < (rectangle.Height - diff) && minimumHeightOfContentPanel < (topNeighbourRect.Height + diff))
                                {
                                    rectangle.Height -= diff;
                                    rectangle.Y += diff;
                                    topNeighbourRect.Height += diff;
                                    contentPanel.SetSizeAndPosition(rectangle);
                                    topNeighbour.SetSizeAndPosition(topNeighbourRect);
                                    SetLastBounds();
                                    border.Location = new Point(border.Location.X, border.Location.Y + diff);
                                }
                            }
                        };
                        border.BringToFront();
                        bordersCreated[contentPair] = border;
                        Controls.Add(border);
                    }
                }
            }

            //bottom
            ContentPanel bottomNeighbour = contentPanels.Find(c => PointInRectangle(points[3], c.Bounds) && c != contentPanel);
            if (bottomNeighbour != null)
            {
                Rectangle bottomNeighbourRect = bottomNeighbour.Bounds;
                (ContentPanel, ContentPanel) contentPair = (contentPanel, bottomNeighbour);
                (ContentPanel, ContentPanel) contentPairReverse = (bottomNeighbour, contentPanel);
                if ((!bordersCreated.Keys.Contains(contentPair)) && (!bordersCreated.Keys.Contains(contentPairReverse)))
                {

                    bool sharedBorder = false;
                    (ContentPanel, ContentPanel) pair = contentPair;
                    Control b2 = new Control();
                    foreach ((ContentPanel c1, ContentPanel c2) in bordersCreated.Keys)
                    {
                        if ((c1 == bottomNeighbour && bordersCreated[(c1, c2)].Name == "Bottom") || (c2 == bottomNeighbour && bordersCreated[(c1, c2)].Name == "Top"))
                        {
                            pair = (c1, c2);
                            b2 = bordersCreated[pair];
                            ContentPanel sharedNeighbour;
                            if (c1 == bottomNeighbour)
                            {
                                sharedNeighbour = c2;
                            }
                            else
                            {
                                sharedNeighbour = c1;
                            }

                            sharedBorder = true;
                            b2.MouseMove += delegate (object sender, MouseEventArgs e)
                            {
                                if (isDragging)
                                {
                                    contentPanel.SetSizeAndPosition(new Rectangle(contentPanel.Bounds.Left, sharedNeighbour.Bounds.Top, contentPanel.Bounds.Width, sharedNeighbour.Bounds.Height));
                                    SetLastBounds();
                                    IEnumerable<Control> otherBorders = bordersCreated.Keys.Where(c => c.Item1 == contentPanel || c.Item2 == contentPanel).Select(c => bordersCreated[c]);
                                    foreach (Control b in otherBorders)
                                    {
                                        if (b.Width != resizeWidth)
                                        {
                                            b.Height = sharedNeighbour.Bounds.Height;
                                        }
                                    }
                                }
                            };
                            b2.Left = Math.Min(rectangle.Left, b2.Left);
                            b2.Top = Math.Min(rectangle.Bottom - resizeWidth / 2, b2.Top);
                            b2.Width += rectangle.Width;
                            b2.BringToFront();
                        }
                    }
                    if (sharedBorder)
                    {
                        bordersCreated[pair] = b2;
                    }
                    else
                    {
                        UserControl border = new UserControl()
                        {
                            Left = rectangle.Left,
                            Top = rectangle.Bottom - resizeWidth / 2,
                            Height = resizeWidth,
                            Width = rectangle.Width,
                            BackColor = accentColor,
                            Cursor = Cursors.SizeNS,
                            Name = "Bottom"
                        };
                        border.MouseDown += Form_MouseDown;
                        border.MouseUp += ControlMouseUp;
                        border.MouseMove += delegate (object sender, MouseEventArgs e)
                        {
                            if (isDragging)
                            {
                                int diff = (e.Location.Y - lastRectangle.Y);
                                bottomNeighbourRect = bottomNeighbour.Bounds;

                                if (minimumHeightOfContentPanel < (rectangle.Height + diff) && minimumHeightOfContentPanel < (bottomNeighbourRect.Height - diff))
                                {
                                    rectangle.Height += diff;
                                    bottomNeighbourRect.Y += diff;
                                    bottomNeighbourRect.Height -= diff;
                                    contentPanel.SetSizeAndPosition(rectangle);
                                    bottomNeighbour.SetSizeAndPosition(bottomNeighbourRect);
                                    SetLastBounds();
                                    border.Location = new Point(border.Location.X, border.Location.Y + diff);
                                }
                            }
                        };
                        border.BringToFront();
                        bordersCreated[contentPair] = border;
                        Controls.Add(border);
                    }


                }
            }
        }

        private void SetLastBounds()
        {
            lastContentSize = contentSpace.Bounds;
            foreach (ContentPanel contentPanel in contentPanels)
            {
                contentPanel.LastBounds = contentPanel.Bounds;
            }
        }

        private int ExtendRectangleWidthRight(int startX, int startY, int maxExtension, int thickness, List<Rectangle> rectangles)
        {
            /*
             Given a starting position of (startX, startY) a maximum x point in maxExtension, the height of the rectangle in thickness and a 
            list of rectangles that the rectangle needs to stop on when being extended. This function will check positions of the grid, checking the
            starting position first, to extend the rectangle right until it hits another rectangle or the edge, returning the distance it extended.
             */

            int extendX = 100;
            for (int y = 0; y < thickness; y++)
            {
                int newExtendX = 0;
                Point Coord = new Point(newExtendX + startX, y + startY);
                while (Coord.X < maxExtension)
                {
                    if (rectangles.Any(r => PointInRectangle(Coord, r)))
                    {
                        break;
                    }
                    else
                    {
                        newExtendX++;
                    }
                    Coord = new Point(newExtendX + startX, y + startY);
                }
                extendX = Math.Min(extendX, newExtendX);
            }
            return extendX;
        }

        private int ExtendRectangleHeightDown(int startY, int startX, int maxExtension, int thickness, List<Rectangle> rectangles)
        {
            int extendY = 100;
            for (int x = 0; x < thickness; x++)
            {
                int newExtendY = 0;
                Point Coord = new Point(x + startX, newExtendY + startY);
                while (Coord.Y < maxExtension)
                {
                    if (rectangles.Any(r => PointInRectangle(Coord, r)))
                    {
                        break;
                    }
                    else
                    {
                        newExtendY++;
                    }
                    Coord = new Point(x + startX, newExtendY + startY);
                }
                extendY = Math.Min(extendY, newExtendY);
            }
            return extendY;
        }

        private int ExtendRectangleWidthLeft(int startX, int startY, int thickness, List<Rectangle> rectangles)
        {
            int extendX = 100;
            for (int y = 0; y < thickness; y++)
            {
                int newExtendX = 0;
                Point Coord = new Point(startX - newExtendX - 2, y + startY);
                while (Coord.X >= 0)
                {
                    if (rectangles.Any(r => PointInRectangle(Coord, r)))
                    {
                        break;
                    }
                    else
                    {
                        newExtendX++;
                    }
                    Coord = new Point(startX - newExtendX - 2, y + startY);
                }
                extendX = Math.Min(extendX, newExtendX);
            }
            return extendX;
        }

        private int ExtendRectangleHeightUp(int startY, int startX, int thickness, List<Rectangle> rectangles)
        {
            int extendY = 100;
            for (int x = 0; x < thickness; x++)
            {
                int newExtendY = 0;
                Point Coord = new Point(x + startX, startY - newExtendY - 2);
                while (Coord.Y >= 0)
                {
                    if (rectangles.Any(r => PointInRectangle(Coord, r)))
                    {
                        break;
                    }
                    else
                    {
                        newExtendY++;
                    }
                    Coord = new Point(x + startX, startY - newExtendY - 2);
                }
                extendY = Math.Min(extendY, newExtendY);
            }
            return extendY;
        }

        private List<Rectangle> GetRectangles()
        {
            List<Rectangle> rectangles = new List<Rectangle>();
            foreach (ContentPanel contentPanel in contentPanels)
            {
                if (positionArea.TryGetValue(contentPanel.position, out Rectangle rectangle))
                {
                    rectangles.Add(rectangle);
                }
            }
            return rectangles;
        }

        private Size GetGridSize()
        {
            Size table = GetTablesize();
            int x = contentSpace.Width;
            int y = contentSpace.Height;
            return new Size(x / table.Width, y / table.Height);
        }

        private Size GetTablesize()
        {
            Rectangle rightRectangle = positionArea.Values.OrderByDescending(r => r.X).ThenBy(r => r.Width).FirstOrDefault();
            int rows = rightRectangle.Width + rightRectangle.X;
            Rectangle bottomRectangle = positionArea.Values.OrderByDescending(r => r.Y).ThenBy(r => r.Height).FirstOrDefault();
            int columns = bottomRectangle.Height + bottomRectangle.Y;
            return new Size(rows, columns);
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMergingToApp)
            {
                foreach (ContentPanel contentPanel in contentPanels)
                {
                    MergeContentPanel();
                }

                isDead = true;
                Close();
                Console.WriteLine("close");
            }
            else
            {
                isDragging = false;
                void resizeWindow(Rectangle rectangle)
                {
                    oldSize = Size;
                    if (this.edgeSnap == "top")
                    {
                        this.WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        Bounds = rectangle;
                    }
                    snapped = true;
                }
                CheckEdgeResizing(resizeWindow);
            }
        }

        delegate void SetTextCallback(string text);
        public void SetTextURL(string text)
        {
            if (TextURL.InvokeRequired)
            {
                var d = new SetTextCallback(SetTextURL);
                TextURL.Invoke(d, new object[] { text });
            }
            else
            {
                TextURL.Text = text;
            }
        }

        // colours
        private void GetColours()
        {
            uiSettings = new UISettings();
            uiSettings.ColorValuesChanged += new Windows.Foundation.TypedEventHandler<UISettings, object>(ColourChanged);
            ColourChanged(uiSettings, this);
        }

        private void ColourChanged(UISettings uiSettings, object sender)
        {
            uiSettings = new UISettings();
            var windowsAccentColor = uiSettings.GetColorValue(UIColorType.Accent);
            accentColor = ConvertColor(windowsAccentColor);
            var windowsBackColor = uiSettings.GetColorValue(UIColorType.Background);
            var windowsForeColor = uiSettings.GetColorValue(UIColorType.Foreground);
            backColor = ConvertColor(windowsBackColor);
            foreColor = ConvertColor(windowsForeColor);
            foreach (Control cnt in GetChildControls(this))
            {
                cnt.BackColor = backColor;
                cnt.ForeColor = foreColor;
            }
            foreach (Control edge in edges)
            {
                edge.BackColor = accentColor;
            }
            BaseMenuStrip.BackColor = Color.Transparent;
            foreach (ContentPanel cp in contentPanels)
            {
                cp.UpdateTabs();
            }
        }

        private Color ConvertColor(Windows.UI.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private List<Control> GetChildControls(Control control)
        {
            List<Control> controls = new List<Control>
            {
                control
            };
            foreach (Control cnt in control.Controls)
            {
                controls.AddRange(GetChildControls(cnt));
            }
            return controls;

        }

        //buttons
        private void CloseButton_Click(object sender, EventArgs e)
        {
            isDead = true;
            Close();
        }

        private void CloseButton_Enter(object sender, EventArgs e)
        {
            CloseButton.BackColor = Color.Gray;
        }

        private void CloseButton_Leave(object sender, EventArgs e)
        {
            CloseButton.BackColor = backColor;
        }



        private void MaximiseButton_Click(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                if (snapped)
                {
                    snapped = false;
                    Size = oldSize;
                }
            }

        }

        private void MinimiseButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


        // tabs
        public void TabsButtonMouseMove(object sender, MouseEventArgs e)
        {
            Tab tab = GetTabsButton((Button)sender);
            Point distance = Point.Subtract(MousePosition, (Size)tab.whereMouseDown);
            int sumDistance = Math.Abs(distance.X) + Math.Abs(distance.Y); //for some reason the absolute function is showing up as a virus
            if (tab != null && tab.isMouseDown && sumDistance >= 5)
            {
                tab.isDragging = true;
                tab.isMouseDown = false;
                Debug.WriteLine(this);
                ContentPanel contentPanel = contentPanels.Find(c => c.tabs.Contains(tab));
                List<Tab> newTabList = new List<Tab>()
                {
                    tab
                };
                Main newApp = new Main();
                TransferTabEventsForPanel(tab, contentPanel, newApp.contentPanels[0]);


                GetContentPanelFromTab(tab).CloseTab(tab.GetCloseButton(), e);

                //popup.isDragging = true;
                //popup.lastRectangle = new Rectangle(10, 10, popup.Width, popup.Height);
                newApp.Load += delegate
                {
                    newApp.BringToFront();
                    int X = MousePosition.X;
                    int Y = MousePosition.Y; ;
                    newApp.Location = new Point(X - 10, Y - 10);
                    Cursor.Position = new Point(X, Y);
                    SendMessage(newApp.Handle, (int)WMessages.WM_LBUTTONDOWN, 0, MAKELPARAM(0, 0));
                    //https://stackoverflow.com/questions/19237034/c-sharp-need-to-psuedo-click-a-window
                    Console.WriteLine("loaded");
                    newApp.contentPanels[0].SetTabs(newTabList, contentPanel);
                };
                newApp.Text = Name;
                newApp.Show();
                MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 1, 10, 10, 0);
                Console.WriteLine("creation");

                //popup.Location = new Point(MousePosition.X - 10, MousePosition.Y - 10);
                //popup.BringToFront();
                //popup.isDragging = true;
                //popup.lastRectangle = new Rectangle(10, 10, popup.Width, popup.Height);
                //popup.DoMouseClick();
            }
        }

        private Tab GetTabsButton(Button btn)
        {
            Tab tab;
            foreach (ContentPanel panel in contentPanels)
            {
                tab = panel.GetTabsButton(btn);
                if (tab != null)
                {
                    return tab;
                }
            }
            return null;
        }

        private ContentPanel GetContentPanelFromTab(Tab tab)
        {
            foreach (ContentPanel panel in contentPanels)
            {
                if (panel.tabs.Contains(tab))
                {
                    return panel;
                }
            }
            throw new ArgumentException();
        }

        internal void TransferTabEventsForPanel(Tab tab, ContentPanel fromPanel, ContentPanel toPanel)
        {
            fromPanel.TransferTabEventsTo(toPanel, tab);
        }

        public void UpdateMergingApp()
        {
            //getFrontmostApp(appsHoveringOver);
            Main highestwindow = GetHighestWindowThatIsNotThis(f => PointInTriggers(MousePosition, f.triggerAreas));

            if (highestwindow == null)
            {
                appMergingTo = null;
                positionMergingTo = "top";
                isMergingToApp = false;
                Opacity = 1;
            }
            else
            {
                appMergingTo = highestwindow;
                Trigger trigger = appMergingTo.triggerAreas.Find(t => PointInRectangle(MousePosition, t.GetRectangleOnScreen()));
                if (trigger == null) { return; }
                positionMergingTo = trigger.Name;
                isMergingToApp = true;
                Opacity = 0.6;
            }
        }

        public bool PointInControls(Point point, List<Control> controls)
        {
            return controls.Any(c => PointInControl(point, c));
        }

        public bool PointInTriggers(Point point, List<Trigger> triggers)
        {
            return triggers.Any(t => PointInRectangle(point, t.GetRectangleOnScreen()));
        }

        private Main GetHighestWindowThatIsNotThis(Func<Main, bool> predicate)
        {
            /*gets the highest window that is not this form and that abides by the predicat provided. If no app is found null is returned.*/
            var openForms = Application.OpenForms.Cast<Form>()
                .Where(f => f.Visible
                && !f.WindowState.Equals(FormWindowState.Minimized)
                && f.GetType() == typeof(Main));
            var openApps = openForms.Cast<Main>()
                .Where(predicate)
                .OrderByDescending(f => f.lastFocused);
            if (openApps.Count() < 1 || (openApps.Count() < 2 && openApps.Contains(this)))
            {
                return null;
            }
            else
            {
                //Console.WriteLine(openForms.ElementAtOrDefault(1).Text);
                //Console.WriteLine(openForms.ElementAtOrDefault(1).DisplayRectangle);
                if (openApps.Contains(this))
                {
                    return openApps.ElementAtOrDefault(1);
                }
                else
                {
                    return openApps.ElementAtOrDefault(0);
                }
            }
        }

        public bool PointInControl(Point position, Control control)
        {
            Form form = control.FindForm();
            Point locationOnForm = form.PointToScreen(control.Location);
            //https://stackoverflow.com/questions/1478022/c-sharp-get-a-controls-position-on-a-form
            Rectangle rectangle = new Rectangle(locationOnForm.X, locationOnForm.Y, control.Width, control.Height);
            Console.WriteLine(rectangle);
            Console.WriteLine(position);
            return PointInRectangle(position, rectangle);
        }

        public bool PointInRectangle(Point point, Rectangle rectangle)
        {
            return point.X >= rectangle.X
                && point.X < rectangle.X + rectangle.Width
                && point.Y >= rectangle.Y
                && point.Y < rectangle.Y + rectangle.Height;
        }

        public void DoMouseClick()
        {
            //https://stackoverflow.com/questions/2416748/how-do-you-simulate-mouse-click-in-c
            //Call the imported function with the cursor's current position
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN, X, Y, 0, 0);
        }

        private int MAKELPARAM(int p, int p_2)
        {
            //turns a coordinate (p, p_2) into a single integer parameter to signify that location to parse into a handle message.
            return ((p_2 << 16) | (p & 0xFFFF));
        }

        public Button GetNewTabButton()
        {
            return newTabBtn;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (Application.OpenForms.Count == 0)
            {
                Application.Exit(); // Close the application when all windows are closed
            }
        }

        private void SearchIcon_Click(object sender, EventArgs e)
        {
            Debug.Write(this);
            TabsPanel.Controls[1].Invalidate();
        }

        private void Settings_Click(object sender, EventArgs e)
        {

        }

        private void BookmarkSettings_Click(object sender, EventArgs e)
        {

        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            _ = Interaction.InputBox("Which panel do you wish to import to?", "Import Bookmarks");

        }

        private void ExportButton_Click(object sender, EventArgs e)
        {

        }

        private void BookmarkSettingsButton_Click(object sender, EventArgs e)
        {

        }

        private void MoreSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void ReloadButton_Paint(object sender, PaintEventArgs e)
        {
            SizeF size = e.Graphics.MeasureString("↻", ReloadButton.Font);
            Brush brush = new SolidBrush(ReloadButton.ForeColor);
            e.Graphics.DrawString("↻", ReloadButton.Font, brush, (ReloadButton.Width - size.Width)/2, (ReloadButton.Height - size.Height) / 2);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            var popup = new AppPopup();
            popup.Show();
        }

        public static void AddHoverColorToButton(Button button)
        {
            Color targetColor = Color.Gray;
            button.MouseEnter += delegate (object sender, EventArgs e)
            {
                button.BackColor = Color.FromArgb(255, 
                    (button.BackColor.R + targetColor.R) / 2, 
                    (button.BackColor.G + targetColor.G) / 2, 
                    (button.BackColor.B + targetColor.B) / 2
                    );
            };
            button.MouseLeave += delegate (object sender, EventArgs e)
            {
                try { 
                    button.BackColor = Color.FromArgb(255,
                        button.BackColor.R * 2 - targetColor.R,
                        button.BackColor.G * 2 - targetColor.G,
                        button.BackColor.B * 2 - targetColor.B
                        );
                }
                catch{

                }
            };
        }
    }
}
