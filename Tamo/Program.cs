using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Collections;

namespace Tamo
{
    class Program
    {
        static void Main(string[] args)
        {
            CookieContainer cookies = new CookieContainer();
            string UserName;
            string Password, DateString;
            DateTime date = DateTime.Now;
            DateString = date.ToString("yyyy-MM-dd");
            string NdUrl = "https://sistema.tamo.lt/Darbai/NamuDarbai";
            Console.WriteLine("Šiandien - " + DateString + " |\n-----------------------\n");
            Console.Write("Sveikas! \nĮvesk savo tamo prisijungimo duomenis, kad sužinotum namų darbus");

        badlogin:
            AskForCredentials(out UserName, out Password);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://sistema.tamo.lt/");
            request.CookieContainer = cookies;
            request.AllowWriteStreamBuffering = true;
            request.ProtocolVersion = HttpVersion.Version11;
            request.AllowAutoRedirect = true;
            request.Method = WebRequestMethods.Http.Post;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            request.ContentType = "application/x-www-form-urlencoded";
            string postData = String.Format("UserName={0}&Password={1}", UserName, Password);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            HttpWebRequest ndRequest = (HttpWebRequest)WebRequest.Create(NdUrl);
            ndRequest.CookieContainer = cookies;
            ndRequest.AllowWriteStreamBuffering = true;
            ndRequest.ProtocolVersion = HttpVersion.Version11;
            ndRequest.Method = "Get";
            ndRequest.AllowAutoRedirect = true;
            ndRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.6) Gecko/20060728 Firefox/1.5";
            ndRequest.ContentType = "application/x-www-form-urlencoded";
            ndRequest.Accept = "text/html";
            HttpWebResponse ndresponse;
            try
            {
                ndresponse = (HttpWebResponse)ndRequest.GetResponse();
            }
            catch
            {
                Console.WriteLine("Tikriausiai neteisingai suvedei duomenis :( Bandyk dar kartą");
                goto badlogin;
            }
            string ndresponseString = new StreamReader(ndresponse.GetResponseStream()).ReadToEnd();
            Console.WriteLine("Tavo namų darbai:");

            List<string> Lesson;
            List<string> Work;

            ParseHtml(out Lesson, out Work, ndresponseString);
            DisplayParsed(Work, Lesson);
            dataStream.Close();
            response.Close();
        }
        public static void AskForCredentials(out string name, out string password)
        {
            Console.Write("\nPrisijungimo vardas: ");
            name = Console.ReadLine();
            Console.Write("Slaptažodis: ");
            password = Console.ReadLine();
        }

        public static string GetDate()
        {
            string DateString;
            string year, month, day;
        baddate:
            Console.Clear();
            Console.Write("Įvesk metus (pvž 2014): ");
            year = Console.ReadLine();
            Console.Write("Įvesk mėnesį (pvž 09): ");
            month = Console.ReadLine();
            Console.Write("Įvesk dieną (pvž 07): ");
            day = Console.ReadLine();
            int m, d;
            try
            {
                m = Int32.Parse(month);
                d = Int32.Parse(day);
            }
            catch
            {
                Console.Write("Blogai įvedei datą, bandyk dar kartą");
                goto baddate;
            }
            if (m < 9 || m > 12 || month.Length < 2 || d < 1 || d > 31 || day.Length < 2)
            {
                goto baddate;

            }
            DateString = String.Format("{0}-{1}-{2}", year, month, day);
            return DateString;
        }

        public static void ParseHtml(out List<string> Lesson, out List<string> Work, string response)
        {
            Lesson = new List<string>();
            Work = new List<string>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            try
            {
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//tr/td[2]"))
                {
                    Lesson.Add(node.ChildNodes[0].InnerHtml.Trim());
                }
            }
            catch
            {
                Console.Clear();
                Console.WriteLine("Šiandien namų darbų neuždavė\nSpausk 0, kad grįžtum į pasirinkimų menu");
                if (Console.ReadLine() == "0")
                {

                }

            }

            foreach (HtmlNode node1 in doc.DocumentNode.SelectNodes("//tr//td/p"))
            {
                Work.Add(node1.ChildNodes[0].InnerHtml);
            }

            Console.WriteLine(" ");
        }

        public static void DisplayParsed(List<string> Work, List<string> Lesson)
        {
            for (int i = 0; i < Work.Count(); i++)
            {

                Console.Write(String.Format("{0}. {1}:  ", Work.Count - i, Lesson[i]));
                Console.WriteLine(" ");
                Console.Write(Work[i]);
                Console.WriteLine(" ");
                Console.WriteLine("--------------------------------------------");
            }
            Console.WriteLine(" ");
            Console.WriteLine("Norėdamas grįžti į pasirinkimų menu spausk 0");
            if (Console.ReadLine() == "0")
            {

            }
          
            Console.ReadLine();
        }

    }

}
