using lndapi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lndcreditwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                MainAsync(args).GetAwaiter().GetResult();
            }
            else
            {
                // TODO DisplayUsage();
            }

            if (Debugger.IsAttached) Console.ReadKey();
        }

        static async Task MainAsync(string[] args)
        {
            double CostPerInterval = double.Parse(args[0]);
            string APIID = args[1];
            string APIKey = args[2];

            Console.WriteLine("lnd credit watch starting up");
            Console.WriteLine($"allowed spending per interval: {CostPerInterval}");

            using (LNDynamic client = new LNDynamic(APIID, APIKey))
            {
                // Get current credit
                double CurrentCredit = await client.BillingCreditAsync();
                Console.WriteLine($"current credit: {CurrentCredit}");

                // Compare to previous credit
                string PreviousCreditFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "previouscredit.txt");
                if (File.Exists(PreviousCreditFilename))
                {
                    double PreviousCredit = double.Parse(File.ReadAllText(PreviousCreditFilename));
                    Console.WriteLine($"previous credit: {PreviousCredit}");

                    double AmountSpentThisInterval = PreviousCredit - CurrentCredit;
                    Console.WriteLine($"amount spent this interval: {AmountSpentThisInterval}");

                    if (AmountSpentThisInterval > CostPerInterval)
                    {
                        // We spent too much during this last interval!
                        Console.WriteLine($"we OVERSPENT by {AmountSpentThisInterval - CostPerInterval}");
                        // TODO Send an eail alert
                    }
                    else
                    {
                        // We were within our spending limit
                        Console.WriteLine($"we UNDERSPENT by {CostPerInterval - AmountSpentThisInterval}");
                    }
                }
                else
                {
                    // No previous file, so send an email alert saying watch has been setup
                    Console.WriteLine("previous credit: N/A (looks like this is our first run)");
                    // TODO Send an email alert
                }

                // Store current credit as previous credit
                Console.WriteLine("updating previouscredit.txt with our current credit balance");
                File.WriteAllText(PreviousCreditFilename, CurrentCredit.ToString());

                Console.WriteLine("done");
            }
        }
    }
}
