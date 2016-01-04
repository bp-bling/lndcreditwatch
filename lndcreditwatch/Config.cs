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
        public RMSecureString APIID { get; set; }
        public RMSecureString APIKey { get; set; }
        public double FixedCostPerInterval { get; set; }
        public double PreviousCreditBalance { get; set; }
        public string SmtpFromAddress { get; set; }
        public string SmtpHostname { get; set; }
        public RMSecureString SmtpPassword { get; set; }
        public int SmtpPort { get; set; }
        public bool SmtpSsl { get; set; }
        public string SmtpToAddress { get; set; }
        public string SmtpUsername { get; set; }
        public double VariableCostPerInterval { get; set; }

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

                Console.WriteLine(" What are you fixed costs per interval?");
                Console.WriteLine(" For example, if you have one flexible virtual machine that costs $0.0097 per");
                Console.WriteLine(" hour, then your fixed cost is $0.0097 (assuming you run lndcreditwatch hourly)");
                while (true)
                {
                    Console.Write(" > ");
                    double TempDouble;
                    if (double.TryParse(Console.ReadLine().TrimStart('$').Trim(), out TempDouble))
                    {
                        this.FixedCostPerInterval = TempDouble;
                        break;
                    }
                    else
                    {
                        Console.WriteLine(" ERROR: Invalid input, try again");
                    }
                }
                Console.WriteLine();

                Console.WriteLine(" What are you variable costs per interval?");
                Console.WriteLine(" For example, if you have one flexible virtual machine that you want to be");
                Console.WriteLine(" alerted about if you use more than 10% of a core, you would calculate:");
                Console.WriteLine("  $16 per cpu core per month / 30 days per month / 24 hours per day * 10%");
                Console.WriteLine(" That is $0.00222222, so enter that (assuming you run lndcreditwatch hourly)");
                while (true)
                {
                    Console.Write(" > ");
                    double TempDouble;
                    if (double.TryParse(Console.ReadLine().TrimStart('$').Trim(), out TempDouble))
                    {
                        this.VariableCostPerInterval = TempDouble;
                        break;
                    }
                    else
                    {
                        Console.WriteLine(" ERROR: Invalid input, try again");
                    }
                }
                Console.WriteLine();

                base.Save();

                Console.WriteLine(" There's some additional settings related to email notifications that I'm");
                Console.WriteLine(" too lazy to prompt you for here, so please go edit the settings here:");
                Console.WriteLine($" {base.FileName}");
                Console.WriteLine();
                Console.WriteLine(" Hit a key to finish setup");
                Console.ReadKey();

                Console.Clear();
            }
        }

        public new void Save()
        {
            base.Save();
        }

        public double TotalCostPerInterval
        {
            get
            {
                return this.FixedCostPerInterval + this.VariableCostPerInterval;
            }
        }
    }
}
