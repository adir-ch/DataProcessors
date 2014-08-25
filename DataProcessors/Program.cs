using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessors
{
    class Program
    {
        static void Main(string[] args)
        {
            WebFormSubmitter submitter = new WebFormSubmitter();
            submitter.SubmitJobForm();
            Console.ReadKey();
        }
    }
}
