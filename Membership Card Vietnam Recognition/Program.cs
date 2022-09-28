using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TD.MCVR;

namespace Membership_Card_Vietnam_Recognition
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            var extracter = new MemberCardExtracter();
            extracter.Show("process.py", @"D:\Download Chorme\Members\Download Internet\H4-61.jpg");
        }
    }
}
