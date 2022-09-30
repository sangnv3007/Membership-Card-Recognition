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
        //public void runPython()
        //{
        //    string FileName = @"process.py";
        //    Process p = new Process();
        //    p.StartInfo = new ProcessStartInfo(@"C:\Python 3.7\python.exe", FileName)
        //    {
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };
        //    p.Start();
        //    //Console.OutputEncoding = Encoding.UTF8;
        //    dynamic output = p.StandardOutput.ReadToEnd();
        //    p.WaitForExit();
        //    Console.WriteLine(output.name);
        //    Console.ReadLine();
        //}
        public CardInformation ProcessImage(string PathFileImage, bool saveImg)
        {
            CardInformation res = new CardInformation();
            using (Py.GIL())
                {                
                    using (PyScope scope = Py.CreateScope())
                    {                   
                        string code = File.ReadAllText("process.py");                                            
                        var scriptCompiled = PythonEngine.Compile(code);                       
                        scope.Execute(scriptCompiled);
                        dynamic func = scope.Get("ReturnInfoCard");
                        var results = func(PathFileImage, saveImg);
                        CardInfoReturn obj = new CardInfoReturn();
                        res = obj.Result((string)results.id, (string)results.name, (string)results.dob, (string)results.home, (string)results.join_date, (string)results.official_date, (string)results.issued_by, (string)results.issue_date);                      
                    }
                }
            return res;
        }
        //public void TestIronPython()
        //{
        //    ////var var1 = 0; var2 = 0
        //    //var path = @"D:\Download Chorme\Members\Download Internet\1.jpg";
        //    //ScriptEngine engine = Python.CreateEngine();
        //    //var searchPaths = engine.GetSearchPaths();
        //    //searchPaths.Add(@"c:\python 3.7\lib\site-packages\");
        //    //searchPaths.Add(@"c:\python 3.7\lib\");
        //    //engine.SetSearchPaths(searchPaths);
        //    //ScriptScope scope = engine.CreateScope();
        //    //engine.ExecuteFile(@"process1.py", scope);
        //    //dynamic testFunction = scope.GetVariable("ReturnInfoCard");
        //    //var result = testFunction(path);
        //}
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
        public CardInformation Result(string id, string fullName, string dateofBirth, string home, string joinDate, string officialDate, string issuedBy, string issueDate)
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
            //if (saveImg)
            //{
            //    results.Image = image;
            //    string root = Environment.CurrentDirectory;
            //    string pathSave = root + @"\anhthe";
            //    if (Directory.Exists(pathSave))
            //    {
            //        CvInvoke.Imwrite(pathSave + @"\anhthe" + id + ".jpg", image);
            //    }
            //    else
            //    {
            //        Directory.CreateDirectory(pathSave);
            //        CvInvoke.Imwrite(pathSave + @"\anhthe" + id + ".jpg", image);
            //    }
            //}
            //else results.Image = null;
            return results;
        }
    }
}
