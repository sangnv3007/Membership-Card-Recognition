using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TD.MCVR
{
    public class MemberCardExtracter
    {
        public void runPython()
        {      
            string FileName = @"D:\vietOCR\process.py";
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Python 3.7\python.exe", FileName)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();
            //Console.OutputEncoding = Encoding.UTF8;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(output);
            Console.ReadLine();
        }
        public void Show(string PathFilePython, string PathFileImage)
        {
            using (Py.GIL())
            {
                using (PyScope scope = Py.CreateScope())
                {
                    string code = File.ReadAllText(PathFilePython);

                    var scriptCompiled = PythonEngine.Compile(code);

                    scope.Execute(scriptCompiled);
                    dynamic func = scope.Get("ReturnInfoCard");                 
                    var text = func(PathFileImage);
                    Image<Bgr,byte> crop = new Image<Bgr, byte>("anhthe.jpg");
                    CardInfoReturn obj = new CardInfoReturn();
                    CardInformation res = obj.Result((string)text.id, (string)text.name, (string)text.dob, (string)text.home, (string)text.join_date, (string)text.official_date, (string)text.issued_by, (string)text.issue_date, crop, true);
                    CvInvoke.Imshow("Anhthe", res.Image.Mat);
                    CvInvoke.WaitKey();
                }
            }
        }
        public void TestIronPython()
        {
            ////var var1 = 0; var2 = 0
            //var path = @"D:\Download Chorme\Members\Download Internet\1.jpg";
            //ScriptEngine engine = Python.CreateEngine();
            //var searchPaths = engine.GetSearchPaths();
            //searchPaths.Add(@"c:\python 3.7\lib\site-packages\");
            //searchPaths.Add(@"c:\python 3.7\lib\");
            //engine.SetSearchPaths(searchPaths);
            //ScriptScope scope = engine.CreateScope();
            //engine.ExecuteFile(@"process1.py", scope);
            //dynamic testFunction = scope.GetVariable("ReturnInfoCard");
            //var result = testFunction(path);
        }
    }
    public class CardInformation
    {
        /// <summary>
        /// ID 
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Họ và tên
        /// </summary>       
        public string FullName { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public string DateOfBirth { get; set; }
        /// <summary>
        /// Quê quán
        /// </summary>
        public string Home { get; set; }
        /// <summary>
        /// Vào đảng ngày
        /// </summary>
        public string JoinDate { get; set; }
        /// <summary>
        /// Chính thức ngày
        /// </summary>
        public string OfficialDate { get; set; }
        /// <summary>
        /// Nơi cấp thẻ
        /// </summary>
        public string IssuedBy { get; set; }
        /// <summary>
        /// Ngày cấp thẻ
        /// </summary>
        public string IssueDate { get; set; }
        /// <summary>
        /// Ảnh thẻ
        /// </summary>
        public Image<Bgr, byte> Image { get; set; }
    }
    public class CardInfoReturn
    {
        public CardInformation Result(string id, string fullName, string dateofBirth, string home, string joinDate, string officialDate, string issuedBy, string issueDate, Image<Bgr, byte> image, bool saveImg)
        {
            CardInformation results = new CardInformation();
            results.ID = id;
            results.FullName = fullName;
            results.DateOfBirth = dateofBirth;
            results.Home = home;
            results.JoinDate = joinDate;
            results.OfficialDate = officialDate;
            results.IssuedBy = issuedBy;
            results.IssueDate = issueDate;
            if (saveImg)
            {
                results.Image = image;
                string root = Environment.CurrentDirectory;
                string pathSave = root + @"\anhthe";
                if (Directory.Exists(pathSave))
                {
                    CvInvoke.Imwrite(pathSave + @"\anhthe" + id + ".jpg", image);
                }
                else
                {
                    Directory.CreateDirectory(pathSave);
                    CvInvoke.Imwrite(pathSave + @"\anhthe" + id + ".jpg", image);
                }
            }
            else results.Image = null;
            return results;
        }
    }
}
