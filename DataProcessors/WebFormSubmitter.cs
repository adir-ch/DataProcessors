using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Data;

namespace DataProcessors
{
    class WebFormSubmitter
    {
        string GetInputData(string form)
        {
            string problem;
            MatchCollection mc = Regex.Matches(form, "Question:(?<problem>.*)</p");

            if (mc.Count != 1)
            {
                Console.WriteLine("No math problem found!");
                return "";
            }

            problem = mc[0].Groups["problem"].ToString().Trim();
            Console.WriteLine("The math problem: " + problem);
            return problem;
        }

        string InputConvert(string encoded)
        {
            string decodedString;

            try
            {
                decodedString = HttpUtility.HtmlDecode(encoded);
            }
            catch
            {
                Console.WriteLine("unable to decode string!");
                return string.Empty;
            }

            Console.WriteLine("Decoded string: " + decodedString.Replace(" ", "")); // this 255 ascii char replace - not white spacce!
            return decodedString.Replace(" ", "");
        }

        string Calculate(string input)
        {
            var loDataTable = new DataTable();
            var loDataColumn = new DataColumn("Eval", typeof(Int32), input);
            loDataTable.Columns.Add(loDataColumn);
            loDataTable.Rows.Add(0);
            return ((Int32)(loDataTable.Rows[0]["Eval"])).ToString();
        }

        public void SubmitJobForm()
        {
            Uri address = new Uri("http://apply.dataprocessors.com.au/");
            CookieContainer cookieContainer = new CookieContainer();

            HttpWebRequest formRequest = (HttpWebRequest)WebRequest.Create(address);
            formRequest.CookieContainer = cookieContainer;

            HttpWebResponse formResponse = (HttpWebResponse)formRequest.GetResponse();
            StreamReader streamReader = new StreamReader(formResponse.GetResponseStream());
            string formData = streamReader.ReadToEnd();

            string encodedData = GetInputData(formData);
            string decodedData = InputConvert(encodedData);
            string result = Calculate(decodedData);

            Console.WriteLine("Result = " + result);
            string postData = "title=submit&jobref=26963628&value=" + result;
            byte[] submitData = Encoding.Default.GetBytes(postData);

            cookieContainer.Add(new Cookie("c3po", "r2d2") { Domain = address.Host }); // adding secret cookie
            Console.WriteLine("Will submit: " + postData + " (len=" + submitData.Length + ")");

            HttpWebRequest postBackRequest = (HttpWebRequest)WebRequest.Create(address);
            postBackRequest.CookieContainer = cookieContainer;
            postBackRequest.Method = "POST";
            postBackRequest.ContentType = "application/x-www-form-urlencoded";
            postBackRequest.ContentLength = submitData.Length;
            postBackRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            postBackRequest.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.5");
            postBackRequest.Headers.Set(HttpRequestHeader.AcceptCharset, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            postBackRequest.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

            Stream postBackStream = postBackRequest.GetRequestStream();
            postBackStream.Write(submitData, 0, submitData.Length);
            postBackStream.Close();

            HttpWebResponse postBackReply = (HttpWebResponse)postBackRequest.GetResponse();
            StreamReader replyStream = new StreamReader(postBackReply.GetResponseStream());
            string reply = replyStream.ReadToEnd();
            Console.WriteLine("Reply: " + reply);
        }
    }
}
