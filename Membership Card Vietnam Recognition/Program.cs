using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TD.MCVR;

namespace Membership_Card_Vietnam_resognition
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
            Stopwatch swObj = new Stopwatch();
            swObj.Start();
            CardInformation res = extracter.ProcessImage(@"D:\Download Chorme\Members\Detect_edge\obj\Membership (10).jpg", true);
            swObj.Stop();
            Console.WriteLine(Math.Round(swObj.Elapsed.TotalSeconds, 2).ToString() + " giây");

            Stopwatch swObj1 = new Stopwatch();
            swObj1.Start();
            CardInformation res1 = extracter.ProcessImage(@"D:\Download Chorme\Members\Detect_edge\obj\Membership (11).jpg", true);
            swObj1.Stop();
            Console.WriteLine(Math.Round(swObj1.Elapsed.TotalSeconds, 2).ToString() + " giây");

            //Console.WriteLine("ID: {0}", res.ID);
            //Console.WriteLine("Name: {0}", res.FullName);
            //Console.WriteLine("Date of birth: {0}", res.DateOfBirth);
            //Console.WriteLine("Home: {0}", res.Home);
            //Console.WriteLine("Issued By: {0}", res.IssuedBy);
            //Console.WriteLine("Issue Date: {0}", res.IssueDate);          
        }
    }
}
