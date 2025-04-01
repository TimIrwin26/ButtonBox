using ButtonBoxServer.Properties;

namespace ButtonBoxServer
{
    public class ButtonBoxServerApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public ButtonBoxServerApplicationContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = ByteArrayToIcon(Resources.AppIcon),
                Text = Resources.Title,
                ContextMenuStrip = new ContextMenuStrip()
                {
                    Items = { new ToolStripMenuItem("Exit", null, Exit) }
                },
                Visible = true
            };
        }

        void Exit(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        private static Icon ByteArrayToIcon(byte[] byteArray)
        {
            using MemoryStream ms = new MemoryStream(byteArray);
            return new Icon(ms);
        }
    }
}