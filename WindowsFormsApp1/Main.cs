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
using SDDPopup;
using SDDTabs;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls.Primitives;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SDDWebBrowser
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        ChromiumWebBrowser currentPage; //to purge
        string defaultURL = "www.google.com"; // to purge
        public List<string> domains;
        List<string> historyStack; // to purge
        List<Tab> tabs = new List<Tab>(); // to purge
        Tab currentTab; // to purge
        List<(Panel, Panel)> appPanels = new List<(Panel, Panel)>(); // to purge
        UserControl[] triggers;
        bool isMergingToApp = false;
        public List<Action> needsHandle = new List<Action>();
        Main appMergingTo;
        long lastFocused = 0;
        public bool isDead;
        int debug = 0;
        string lastSite; // purger from here
        int historyStackIndex;
        bool fromHistory; // to here
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
            generateNewTab(defaultURL);

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
            updateNavButtons();
            appPanels.Add((Tabs, Content));
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

        private void generateNewTab(string Url) // to purge
        {
            currentPage = new ChromiumWebBrowser();
            currentPage.LoadUrl(Url);
            //Content.Controls.Clear();
            Content.Controls.Add(currentPage);
            currentPage.BringToFront();
            currentPage.Dock = DockStyle.Fill;
            currentPage.AddressChanged += new EventHandler<AddressChangedEventArgs>(browser_AddressChanged);
            currentPage.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(browser_FrameLoadEnd);
            currentPage.FrameLoadStart += new EventHandler<FrameLoadStartEventArgs>(browser_FrameLoadStart);
            currentPage.TitleChanged += delegate (object titleSender, TitleChangedEventArgs titleArgs)
            {
                currentPage.Name = titleArgs.Title;
            };

            currentTab = addTab(currentPage);
            changeTabs(currentTab);

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

        private void form_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMergingToApp)
            {
                foreach (Tab tab in tabs) 
                {
                    transferTabEvents(tab, this, appMergingTo);
                }
                appMergingTo.ExtendTabs(tabs);
                Console.WriteLine(tabs.Count);
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

        // Browser - to purge
        private void OnBrowserLoadError(object sender, LoadErrorEventArgs e) // https://github.com/cefsharp/CefSharp.MinimalExample/blob/master/CefSharp.MinimalExample.WinForms/BrowserForm.cs#L32
        {
            //Actions that trigger a download will raise an aborted error.
            //Aborted is generally safe to ignore
            if (e.ErrorCode == CefErrorCode.Aborted)
            {
                return;
            }

            var errorHtml = string.Format("<html><body><h2>Failed to load URL {0} with error {1} ({2}).</h2></body></html>",
                                              e.FailedUrl, e.ErrorText, e.ErrorCode);

            _ = e.Browser.SetMainFrameDocumentContentAsync(errorHtml);

            //AddressChanged isn't called for failed Urls so we need to manually update the Url TextBox
            textURL.Text = e.FailedUrl;
        }

        private void browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            if (isDead) {  return; }
            if (sender == currentPage)
            {
                setTextURL(e.Address);
            }
            if (historyStack.Count == 0 || e.Address != lastSite)
            {
                if (!fromHistory)
                {
                    if (historyStackIndex < historyStack.Count)
                    {
                        historyStack.RemoveRange(historyStackIndex - 1, historyStack.Count - historyStackIndex);
                    }
                    if (historyStack.Count == historyStackIndex)
                    {
                        historyStack.Add(e.Address);
                        historyStackIndex++;
                        currentTab.setHistoryIndex(historyStackIndex);
                    }
                }
                fromHistory = false;
                updateNavButtons();

            }
            lastSite = e.Address;
        }

        

        private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {

        }

        private void browser_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {

        }

        //purge to here

        delegate void SetTextCallback(string text);
        public void setTextURL(string text) // not purge
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

        private void forwardButton_Paint(object sender, PaintEventArgs e)
        {
            //https://stackoverflow.com/questions/72644619/how-to-change-the-forecolor-of-a-disabled-button
            Button btn = (Button)sender;
            var solidBrush = new SolidBrush(btn.ForeColor);
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.DrawString(">", btn.Font, solidBrush, e.ClipRectangle, stringFormat);
            solidBrush.Dispose();
            stringFormat.Dispose();
        }

        private void backButton_Paint(object sender, PaintEventArgs e)
        {
            //https://stackoverflow.com/questions/72644619/how-to-change-the-forecolor-of-a-disabled-button
            Button btn = (Button)sender;
            var solidBrush = new SolidBrush(btn.ForeColor);
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.DrawString("<", btn.Font, solidBrush, e.ClipRectangle, stringFormat);
            solidBrush.Dispose(); 
            stringFormat.Dispose();
        }

        // navigation

        private void updateNavButtons()
        {
            if (isDead) return;
            setBackButtonEnabled(historyStackIndex > 1);
            setForwardButtonEnabled(historyStackIndex < historyStack.Count);
            if (backButton.Enabled)
            {
                setBackButtonForeColor(foreColor);
            }
            else
            {
                setBackButtonForeColor(Color.Gray);
            }

            if (forwardButton.Enabled)
            {
                setForwardButtonForeColor(foreColor);
            }
            else
            {
                setForwardButtonForeColor(Color.Gray);
            }
        }

        delegate void SetBoolCallback(bool enabled);
        private void setBackButtonEnabled(bool enabled)
        {
            if (backButton.InvokeRequired)
            {
                var d = new SetBoolCallback(setBackButtonEnabled);
                backButton.Invoke(d, new object[] { enabled });
            }
            else
            {
                backButton.Enabled = enabled;
            }
        }

        private void setForwardButtonEnabled(bool enabled)
        {
            if (forwardButton.InvokeRequired)
            {
                var d = new SetBoolCallback(setForwardButtonEnabled);
                forwardButton.Invoke(d, new object[] { enabled });
            }
            else
            {
                forwardButton.Enabled = enabled;
            }
        }

        delegate void SetColorCallback(Color color);

        private void setBackButtonForeColor(Color color)
        {
            if (backButton.InvokeRequired)
            {
                var d = new SetColorCallback(setBackButtonForeColor);
                backButton.Invoke(d, new object[] { color });
            }
            else
            {
                backButton.ForeColor = color;
                backButton.UseVisualStyleBackColor = true;
            }
        }

        private void setForwardButtonForeColor(Color color)
        {
            if (forwardButton.InvokeRequired)
            {
                var d = new SetColorCallback(setForwardButtonForeColor);
                forwardButton.Invoke(d, new object[] { color });
            }
            else
            {
                forwardButton.ForeColor = color;
                forwardButton.UseVisualStyleBackColor = true;
            }
        }

        private void textURL_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                enterText();
            }
        }

        private void enterText()
        {
            if (CheckUrl(textURL.Text))
            {
                Console.WriteLine("loading \"" + textURL.Text + "\"");
                currentPage.LoadUrl(textURL.Text);
            }
            else
            {
                // NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                // queryString.Add("q", textURL.Text);  // https://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c
                string queryString = Uri.EscapeDataString(textURL.Text); // https://stackoverflow.com/questions/14574695/converting-a-string-to-a-url-safe-string-for-twilio
                currentPage.LoadUrl("https://www.google.com/search?q=" + queryString);
            }
        }

        private void textURL_TextChanged(object sender, EventArgs e)
        {
            if (CheckUrl(textURL.Text))
            {
                searchIcon.Text = "🌐";
            }
            else
            {
                searchIcon.Text = "G";
            }
        }

        protected bool CheckUrl(string url)
        {
            string urlWithProtocol = "http://" + url;
            bool result = Uri.IsWellFormedUriString(url.ToString(), UriKind.Absolute)
                || (Uri.IsWellFormedUriString(urlWithProtocol, UriKind.Absolute)
                && (url.EndsWith("/") 
                || (domains.Any(x => url.Contains(x))
                && char.IsLetterOrDigit(url[0]))
                || Regex.IsMatch(url, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")))
                || Directory.Exists(url);
            return result;
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

        private void backButton_Click(object sender, EventArgs e)
        {
            if (historyStackIndex > 1)
            {
                historyStackIndex--;
                currentTab.setHistoryIndex(historyStackIndex);
                fromHistory = true;
                currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
            }
        }

        private void backButton_Hover(object sender, EventArgs e)
        {
            closeButton.BackColor = Color.Gray;
        }

        private void backButton_Leave(object sender, EventArgs e)
        {
            closeButton.BackColor = backColor;
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            if (historyStackIndex < historyStack.Count())
            {
                historyStackIndex++;
                currentTab.setHistoryIndex(historyStackIndex);
                fromHistory = true;
                currentPage.LoadUrl(historyStack[historyStackIndex - 1]);
            }
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            currentPage.Reload();
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

        private void updateTabs() //to purge from here
        {
            if (isDead) return;
            clearTabs();
            int currentXPos = 0;
            foreach (Tab tab in tabs) 
            {
                Button btn = tab.GetButton();
                changeBtnLocation(btn, new Point(currentXPos, 0));
                addBtnControl(btn);
                currentXPos += btn.Width;
            }
            addBtnControl(newTabBtn);
            setNewTabButtonLocation(new Point(currentXPos, newTabBtn.Location.Y));
        }

        delegate void SetButtonPointCallback(Button button, Point point);
        private void changeBtnLocation(Button button, Point point)
        {
            if (button.InvokeRequired)
            {
                var d = new SetButtonPointCallback(changeBtnLocation);
                button.Invoke(d, new object[] { button, point });
            }
            else
            {
                button.Location = point;
            }
        }

        delegate void SetButtonCallback(Button button);
        private void addBtnControl(Button button)
        {
            if (Tabs.InvokeRequired || button.InvokeRequired)
            {
                var d = new SetButtonCallback(addBtnControl);
                if (IsHandleCreated)
                {   
                    Tabs.Invoke(d, new object[] { button });
                }
                else
                {
                    Action invokeTabs = ()=>Tabs.Invoke(d, new object[] { button });
                    needsHandle.Add(invokeTabs);
                }
            }
            else
            {
                Tabs.Controls.Add(button);
            }
        }

        delegate void SetTabsCallback(List<Tab> tabs);


        delegate void SetPointCallback(Point point);
        private void setNewTabButtonLocation(Point point)
        {
            if (newTabBtn.InvokeRequired)
            {
                var d = new SetPointCallback(setNewTabButtonLocation);
                newTabBtn.Invoke(d, new object[] { point });
            }
            else
            {
                newTabBtn.Location = point;
            }
        }

        delegate void SetVoidCallback();
        private void clearTabs()
        {
            if (Tabs.InvokeRequired)
            {
                var d = new SetVoidCallback(clearTabs);
                Tabs.Invoke(d, new object[] {  });
            }
            else
            {
                Tabs.Controls.Clear();
            }
        }

        private Tab addTab(ChromiumWebBrowser tab)
        {
            Tab newTab = new Tab(tab, updateTabs);

            Button btn = newTab.GetButton();
            btn.MouseDown += new MouseEventHandler(TabsButtonMouseDown);
            btn.MouseUp += new MouseEventHandler(TabsButtonMouseUp);
            btn.MouseMove += new MouseEventHandler(TabsButtonMouseMove);

            btn.ForeColor = foreColor;
            btn.BackColor = backColor;

            Button closeBtn = newTab.GetCloseButton();
            closeBtn.Click += new EventHandler(closeTab);

            Tabs.Controls.Add(btn);
            tabs.Add(newTab);
            updateTabs();
            return newTab;
        }

        private Tab getTabsButton(Button btn)
        {
            bool checkTabButtons(Tab testTab)
            {
                return (testTab.GetButton() == btn);
            }
            Tab tab = tabs.Find(checkTabButtons);
            return tab;
        }
        // purge to here
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
                
                closeTab(tab.GetCloseButton(), e);

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
                };
                newApp.Text = Application.OpenForms.Count.ToString();
                newApp.Show();
                newApp.SetTabs(newTabList);
                MouseEventArgs eventArgs = new MouseEventArgs(MouseButtons.Left, 1, 10, 10, 0);
                Console.WriteLine("creation");

                //popup.Location = new Point(MousePosition.X - 10, MousePosition.Y - 10);
                //popup.BringToFront();
                //popup.isDragging = true;
                //popup.lastRectangle = new Rectangle(10, 10, popup.Width, popup.Height);
                //popup.DoMouseClick();
            }
        }

        public void transferTabEvents(Tab tab, Main fromApp, Main toApp)
        {
            tab.GetButton().MouseDown -= fromApp.TabsButtonMouseDown;
            tab.GetButton().MouseDown += new MouseEventHandler(toApp.TabsButtonMouseDown);
            tab.GetButton().MouseMove -= fromApp.TabsButtonMouseMove;
            tab.GetButton().MouseMove += new MouseEventHandler(toApp.TabsButtonMouseMove);
            tab.GetButton().MouseUp -= fromApp.TabsButtonMouseUp;
            tab.GetButton().MouseUp += new MouseEventHandler(toApp.TabsButtonMouseUp);
            tab.GetCloseButton().Click -= fromApp.closeTab;
            tab.GetCloseButton().Click += new EventHandler(toApp.closeTab);
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

        private void createAppPanel(Tab tab, Rectangle area) //to purge
        {
            Button btn = tab.GetButton();
            ChromiumWebBrowser browser = tab.GetBrowser();
            Panel contentPanel = new Panel();
            Panel tabPanel = new Panel();
            tabPanel.Height = Tabs.Height;
            tabPanel.Width = area.Width;
            tabPanel.Location = new Point(area.X, area.Y);
            contentPanel.Width = area.Width;
            contentPanel.Height = area.Height - tabPanel.Height;
            contentPanel.Location = new Point(area.X, area.Y + tabPanel.Height);
            tabPanel.BringToFront();
            contentPanel.Size = browser.Parent.Size;
            contentPanel.BringToFront();
            btn.Parent.Controls.Remove(btn);
            browser.Parent.Controls.Remove(browser);
            tabPanel.Controls.Add(btn);
            contentPanel.Controls.Add(browser);
            Controls.Add(tabPanel);
            Controls.Add(contentPanel);
            appPanels.Add((tabPanel, contentPanel));
        }

        public void SetTabs(List<Tab> newTabList) //to purge
        {
            tabs = newTabList;
            updateTabs();
            Content.Controls.Clear();
            foreach (Tab tab in tabs)
            {
                Content.Controls.Add(tab.GetBrowser());
            }
            changeTabs(tabs[tabs.Count - 1]);
        }

        public void ExtendTabs(List<Tab> newTabList) //to purge
        {
            tabs.AddRange(newTabList);
            updateTabs();
            foreach (Tab tab in newTabList)
            {
                Content.Controls.Add(tab.GetBrowser());
            }
            if (tabs.Count > 0)
            {
                changeTabs(tabs[tabs.Count - 1]);
            }
            Console.WriteLine("tab count:");
            Console.WriteLine(tabs.Count);
        }

        private void ResetPanelPositions()
        {

        }

        private void TabsButtonMouseUp(object sender, MouseEventArgs e)
        {
            Tab tab = getTabsButton((Button)sender);
            tab.isMouseDown = false;
            if (!tab.isDragging)
            {
                changeTabs(tab);
            }
            tab.isDragging = false;
        }

        private void TabsButtonMouseDown(object sender, MouseEventArgs e)
        {
            Tab tab = getTabsButton((Button)sender);
            tab.isMouseDown = true;
        }

        private void changeTabs(Tab tab)
        {
            currentPage = tab.GetBrowser();
            currentTab = tab;
            textURL.Text = tab.GetBrowser().Address;
            tab.GetBrowser().BringToFront();
            historyStack = tab.getHistory();
            historyStackIndex = tab.getHistoryIndex();
            updateNavButtons();
        } 

        private void newTabBtn_Click(object sender, EventArgs e)
        {
            generateNewTab(defaultURL); 
        }

        private void closeTab(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            bool checkButton(Tab t)
            {
                return (t.GetCloseButton() == btn);
            }
            Tab tab = tabs.Find(checkButton);
            tabs.Remove(tab);
            Content.Controls.Remove(tab.GetBrowser());            
            updateTabs();
            if (currentTab == tab)
            {
                foreach (Control browser in Content.Controls)
                {
                    if (Content.Controls.GetChildIndex(browser) == 0)
                    {
                        currentPage = (ChromiumWebBrowser)browser;
                        bool checkBrowser(Tab testTab)
                        {
                            return (testTab.GetBrowser() == currentPage);
                        }
                        Tab currentTab = tabs.Find(checkBrowser);
                        if (currentTab != null)
                        {
                            changeTabs(currentTab);
                        }
                    }
                }
            }
        } // purge until

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
