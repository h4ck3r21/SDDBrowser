using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using SDDBrowser;
using SDDPopup;
using SDDTabs;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls.Primitives;

namespace SDDWebBrowser
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        public List<string> domains;
        List<ContentPanel> contentPanels = new List<ContentPanel>();
        UserControl[] triggers;
        bool isMergingToApp = false;
        public List<Action> needsHandle = new List<Action>();
        Main appMergingTo;
        string positionMergingTo;
        long lastFocused = 0;
        public bool isDead;
        int debug = 0;
        Size lastSize; 
        bool loaded;
        protected bool isDragging = false;
        bool wasFullScreen = false;
        protected Rectangle lastRectangle = new Rectangle();
        int resizeWidth = 3;
        int edgeResizeWidth = 50;
        List<Control> edges = new List<Control>();
        UISettings uiSettings;
        public Color accentColor;
        public Color backColor;
        public Color foreColor;
        string edgeSnap = "None";
        Size oldSize;
        bool snapped = false;

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


        // window
        private void Main_Load(object sender, EventArgs e)
        {
            Console.WriteLine("started new app");
            Activated += new EventHandler(form_gotFocus);
            lastFocused = DateTime.UtcNow.Ticks;

            ContentPanel contentPanel = new ContentPanel(this, "top");
            contentPanels.Add(contentPanel);
            contentPanel.generateNewTab(ContentPanel.defaultURL);
            newTabBtn.Click += new EventHandler(contentPanel.newTabBtn_Click);

            getColours();

            StreamReader reader = File.OpenText("domain_names.csv");
            domains = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                domains.Add(values[0]);
            }
            
            lastSize = Size;
            loaded = true;

            initialiseFormEdge();
            initialiseTriggers();
            contentPanel.updateNavButtons();           
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            foreach (Action action in needsHandle)
            {
                action();
            }
        }

        private void form_gotFocus(object sender, EventArgs e)
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
                ResizeWidthControl(Content, Size);
                ResizeHeightControl(Content, Size);
                ResizeWidthControl(contentHeader, Size);
                ResizeWidthControl(textURL, Size);
                ResizeWidthControl(Tabs, Size);
                RepositionWidthPosition(closeButton, Size);
                RepositionWidthPosition(maximiseButton, Size);
                RepositionWidthPosition(minimiseButton, Size);
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
            }
        }

        private void ResizeWidthControl(Control control, Size newSize)
        {
            int width = newSize.Width - lastSize.Width;
            control.Width += width;
        }

        private void ResizeHeightControl(Control control, Size newSize)
        {
            int height = newSize.Height - lastSize.Height;
            control.Height += height;
        }

        private void RepositionWidthPosition(Control control, Size newSize)
        {
            int width = newSize.Width - lastSize.Width;
            control.Left += width;
        }

        protected void initialiseFormEdge()
        {
            Color borderColor = accentColor;
            MouseDown += new MouseEventHandler(form_MouseDown);
            MouseMove += new MouseEventHandler(form_MouseMove);
            MouseUp += form_MouseUp;

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
            bottomEdge.MouseDown += form_MouseDown;
            bottomEdge.MouseUp += mouseUp;
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
            rightEdge.MouseDown += form_MouseDown;
            rightEdge.MouseUp += mouseUp;
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
            bottomRightEdge.MouseDown += form_MouseDown;
            bottomRightEdge.MouseUp += mouseUp;
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
            topRightEdge.MouseDown += form_MouseDown;
            topRightEdge.MouseUp += mouseUp;
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
            topEdge.MouseDown += form_MouseDown;
            topEdge.MouseUp += mouseUp;
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
            leftEdge.MouseDown += form_MouseDown;
            leftEdge.MouseUp += mouseUp;
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
            bottomLeftEdge.MouseDown += form_MouseDown;
            bottomLeftEdge.MouseUp += mouseUp;
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
            topLeftEdge.MouseDown += form_MouseDown;
            topLeftEdge.MouseUp += mouseUp;
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

        private void initialiseTriggers()
        {
            int triggerWidth = Content.Location.X;
            int triggerHeight = Content.Location.Y;
            triggers = new UserControl[3] {
                new UserControl()
                {
                    Anchor = (AnchorStyles.Top | AnchorStyles.Left),
                    Height = Height,
                    Width = triggerWidth,
                    Left = 0,
                    Top = 0,
                    BackColor = Color.Transparent,
                    Name = "left"
                },
                new UserControl()
                {
                    Anchor = (AnchorStyles.Top | AnchorStyles.Right),
                    Height = Height,
                    Width = triggerWidth,
                    Left = DisplayRectangle.Width - resizeWidth,
                    Top = 0,
                    BackColor = Color.Transparent,
                    Name = "right"
                },
                new UserControl()
                {
                    Anchor = (AnchorStyles.Bottom | AnchorStyles.Left),
                    Height = triggerHeight,
                    Width = Width - 2 * triggerWidth,
                    Left = triggerWidth,
                    Top = DisplayRectangle.Height - resizeWidth,
                    BackColor = Color.Transparent,
                    Name = "bottom"
                }
            };
            
            
        }


        public void form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastRectangle = new Rectangle(e.Location.X, e.Location.Y, this.Width, this.Height);
                //Location = new Point(0, 0);
            }
        }

        private void form_MouseMove(object sender, MouseEventArgs e)
        {
            if (snapped && isDragging)
            {
                snapped = false;
                Size = oldSize;
                form_MouseDown(sender, e);
            }
            else if (wasFullScreen && isDragging)
            {
                form_MouseDown(sender, e);
                wasFullScreen = false;
            }
            if (isDragging && WindowState == FormWindowState.Normal)
            {
                DragControl(this, e, lastRectangle);
                updateMergingApp();
            }
            else if (isDragging && WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                Location = new Point(MousePosition.X - Width/2, MousePosition.Y);
                wasFullScreen = true;
            }
            //I need a way either to trigger an event whenever the mouse moves or better yet send events between apps and a way to prevent apps from closing when the first app is closed.
        }

        private void DragControl(Control control, MouseEventArgs e, Rectangle rectangle)
        {
            int x = (control.Location.X + (e.Location.X - rectangle.X));
            int y = (control.Location.Y + (e.Location.Y - rectangle.Y));

            control.Location = new Point(x, y);
        }

        delegate void nearEdgeFunction(Rectangle rectangle);
        private void checkEdgeResizing(nearEdgeFunction edgeFunction)
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
                rectangle = new Rectangle(screenbounds.X, screenbounds.Y, screenbounds.Width/2, screenbounds.Height/2);
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

        private void drawRectangle(Rectangle rectangle, Color colour)
        {
            Control control = new Control();
            Graphics graphics = control.CreateGraphics();
            Brush brush = new SolidBrush(colour);
            graphics.FillRectangle(brush, rectangle);

        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void mergeContentPanel(string position)
        {
            ContentPanel panelMergingFrom = contentPanels.Find(p => p.position == position);
            List<Tab> tabs = panelMergingFrom.tabs;
            foreach (Tab tab in tabs)
            {
                transferTabEvents(tab, this, appMergingTo);
            }
            ContentPanel panelMergingTo = appMergingTo.contentPanels.Find(p => p.position == position);
            panelMergingTo.ExtendTabs(tabs);
            Console.WriteLine(tabs.Count);
        }

        private void form_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMergingToApp)
            {
                foreach (ContentPanel contentPanel in contentPanels)
                {
                    mergeContentPanel(contentPanel.position);
                }
                
                isDead = true;
                Close();
                Console.WriteLine("close");
            }
            else
            {
                isDragging = false;
                nearEdgeFunction resizeWindow = delegate (Rectangle rectangle)
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
                };
                checkEdgeResizing(resizeWindow);
            }
        }

        delegate void SetTextCallback(string text);
        public void setTextURL(string text)
        {
            if (textURL.InvokeRequired)
            {
                var d = new SetTextCallback(setTextURL);
                textURL.Invoke(d, new object[] { text });
            }
            else
            {
                textURL.Text = text;
            }
        }

        // colours
        private void getColours()
        {
            uiSettings = new UISettings();
            uiSettings.ColorValuesChanged += new Windows.Foundation.TypedEventHandler<UISettings, object>(colourChanged);
            colourChanged(uiSettings, this);
        }

        private void colourChanged(UISettings uiSettings, object sender)
        {
            uiSettings = new UISettings();
            var windowsAccentColor = uiSettings.GetColorValue(UIColorType.Accent);
            accentColor = convertColor(windowsAccentColor);
            var windowsBackColor = uiSettings.GetColorValue(UIColorType.Background);
            var windowsForeColor = uiSettings.GetColorValue(UIColorType.Foreground);
            backColor = convertColor(windowsBackColor);
            foreColor = convertColor(windowsForeColor);
            foreach (Control cnt in getChildControls(this))
            {
                cnt.BackColor = backColor;
                cnt.ForeColor = foreColor;
            }
            foreach (Control edge in edges)
            {
                edge.BackColor = accentColor;
            }
        }

        private Color convertColor(Windows.UI.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private List<Control> getChildControls(Control control)
        {
            List<Control> controls = new List<Control>();
            controls.Add(control);
            foreach (Control cnt in control.Controls)
            {
                controls.AddRange(getChildControls(cnt));
            }
            return controls;

        }

        //buttons
        private void closeButton_Click(object sender, EventArgs e)
        {
            isDead = true;
            Close();
        }

        private void closeButton_Enter(object sender, EventArgs e)
        {
            closeButton.BackColor = Color.Gray;
        }

        private void closeButton_Leave(object sender, EventArgs e)
        {
            closeButton.BackColor = backColor;
        }

        

        private void maximiseButton_Click(object sender, EventArgs e)
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

        private void minimiseButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


        // tabs

        public void TabsButtonMouseMove(object sender, MouseEventArgs e)
        {
            Tab tab = getTabsButton((Button)sender);
            if (tab != null && tab.isMouseDown)
            {
                tab.isDragging = true;
                tab.isMouseDown = false;
                List<Tab> newTabList = new List<Tab>() 
                { 
                    tab 
                };
                Main newApp = new Main();
                transferTabEvents(tab, this, newApp);
                
                getContentPanelFromTab(tab).closeTab(tab.GetCloseButton(), e);

                //popup.isDragging = true;
                //popup.lastRectangle = new Rectangle(10, 10, popup.Width, popup.Height);
                newApp.Load += delegate
                {
                    newApp.BringToFront();
                    int X = MousePosition.X;
                    int Y = MousePosition.Y;;
                    newApp.Location = new Point(X - 10, Y - 10);
                    Cursor.Position = new Point(X, Y);
                    SendMessage(newApp.Handle, (int)WMessages.WM_LBUTTONDOWN, 0, MAKELPARAM(0, 0));
                    //https://stackoverflow.com/questions/19237034/c-sharp-need-to-psuedo-click-a-window
                    Console.WriteLine("loaded");
                    newApp.contentPanels[0].SetTabs(newTabList);
                };
                newApp.Text = Application.OpenForms.Count.ToString();
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

        private Tab getTabsButton(Button btn)
        {
            Tab tab;
            foreach (ContentPanel panel in contentPanels)
            {
                tab = panel.getTabsButton(btn);
                if (tab != null)
                {
                    return tab;
                }
            }
            throw new ArgumentException();
        }

        private ContentPanel getContentPanelFromTab(Tab tab)
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

        public void transferTabEvents(Tab tab, Main fromApp, Main toApp)
        {
            tab.GetButton().MouseMove -= fromApp.TabsButtonMouseMove;
            tab.GetButton().MouseMove += new MouseEventHandler(toApp.TabsButtonMouseMove);
        }

        public void updateMergingApp()
        {
            //getFrontmostApp(appsHoveringOver);
            Main highestwindow = GetHighestWindowThatIsNotThis(f => PointInControl(MousePosition, f.Tabs));
            
            if (highestwindow == null)
            {
                appMergingTo = null;
                isMergingToApp = false;
                Opacity = 1;
            }
            else
            {
                appMergingTo = highestwindow;
                isMergingToApp = true;
                Opacity = 0.8;
            }
        }

        private Main GetHighestWindowThatIsNotThis(Func<Main, bool> predicate)
        {
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
                && point.X <= rectangle.X + rectangle.Width
                && point.Y >= rectangle.Y
                && point.Y <= rectangle.Y + rectangle.Height;
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
            return ((p_2 << 16) | (p & 0xFFFF));
        }

        public Button getNewTabButton()
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
    }
}
