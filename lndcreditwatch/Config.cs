using RandM.RMLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lndcreditwatch
{
    class Config : ConfigHelper
    {
        public double AcceptableCostPerInterval { get; set; }
        public RMSecureString APIID { get; set; }
        public RMSecureString APIKey { get; set; }
        public double PreviousCreditBalance { get; set; }
        public string SmtpFromAddress { get; set; }
        public string SmtpHostname { get; set; }
        public RMSecureString SmtpPassword { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpSsl { get; set; }
        public string SmtpToAddress { get; set; }
        public string SmtpUsername { get; set; }

        public static Config Default = new Config();

        public Config() : base()
        {
            PreviousCreditBalance = double.MinValue;
            SmtpFromAddress = "lndcreditwatch@localhost";
            SmtpHostname = "localhost";
            SmtpPassword = "";
            SmtpPort = 25;
            SmtpSsl = false;
            SmtpToAddress = "lndcreditwatch@localhost";
            SmtpUsername = "";

#if DEBUG
            string SectionName = "DEBUG";
#else
            string SectionName = "CONFIGURATION";
#endif

            if (!base.Load(SectionName))
            {
                Console.WriteLine($" {Path.GetFileName(base.FileName)} not found, prompting for settings");
                Console.WriteLine();

                Console.WriteLine(" What is the API ID?");
                Console.WriteLine(" (NOTE: For additional security, only assign billing.credit to the API key\r\n you use with lndcreditwatch, since it's the only API call it makes)");
                while (true)
                {
                    Console.Write(" > ");
                    this.APIID = Console.ReadLine().Trim();
                    if (this.APIID.Length == 16)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(" ERROR: API ID should be 16 characters, try again");
                    }
                }
                Console.WriteLine();

                Console.WriteLine(" What is the API Key?");
                Console.WriteLine(" (HINT: You can click the icon at the top left, then click the Edit menu,\r\n then click the Paste option to avoid having to type all 128 characters!)");
                while (true)
                {
                    Console.Write(" > ");
                    this.APIKey = Console.ReadLine().Trim();
                    if (this.APIKey.Length == 128)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(" ERROR: API Key should be 128 characters, try again");
                    }
                }
                Console.WriteLine();

                Console.WriteLine(" What is the acceptable cost per interval?");
                Console.WriteLine(" For example, if you plan to run this as an hourly scheduled task, then add up");
                Console.WriteLine(" the fixed hourly cost of all your virtual machines, and then also calculate");
                Console.WriteLine(" the variable cost of your expected virtual cpu usage on any flexible plans.");
                Console.WriteLine();
                Console.WriteLine(" In my case I have one flexible virtual machine which costs $0.00972222 hourly,");
                Console.WriteLine(" and I want to be alerted if I'm using more than 10% of a core, so I calculate");
                Console.WriteLine(" $16 per cpu core per month / 30 days per month / 24 hours per day * 10%");
                Console.WriteLine(" This works out to $0.00222222, so I add that to the fixed cost of $0.00972222,");
                Console.WriteLine(" which is $0.01194444, so that's what I would enter on the prompt below.");
                while (true)
                {
                    Console.Write(" > ");
                    double TempDouble;
                    if (double.TryParse(Console.ReadLine().TrimStart('$').Trim(), out TempDouble))
                    {
                        this.AcceptableCostPerInterval = TempDouble;
                        break;
                    }
                    else
                    {
                        Console.WriteLine(" ERROR: Invalid input, try again");
                    }
                }
                Console.WriteLine();

                base.Save();
            }
        }

        public new void Save()
        {
            base.Save();
        }
    }
}
