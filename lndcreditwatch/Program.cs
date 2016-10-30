using lndapi;
using RandM.RMLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace lndcreditwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
            if (Debugger.IsAttached) {
                Console.WriteLine("Hit a key to quit");
                Console.ReadKey();
            }
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine(" lndcreditwatch starting up");

            try {
                using (LNDynamic client = new LNDynamic(Config.Default.APIID.GetPlainText(), Config.Default.APIKey.GetPlainText())) {
                    // Get current credit balance
                    double CurrentCreditBalance = await client.BillingCreditAsync();

                    // Handle the midnight hour, which will alert us to the usage from the previous day
                    string DailyUsageFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dailyusage.txt");
                    if (DateTime.Now.Hour == 0) {
                        Console.WriteLine(" midnight hour, alerting previous day's usage");
                        if (File.Exists(DailyUsageFilename)) {
                            // Alert daily usage
                            SendEmail($"current Luna Node credit balance: ${CurrentCreditBalance}\r\nlndcreditwatch daily recap:\r\n\r\n{File.ReadAllText(DailyUsageFilename)}");

                            // And reset
                            File.Delete(DailyUsageFilename);
                        }
                    }

                    // Output some information
                    Console.WriteLine($" fixed cost per interval    : ${Config.Default.FixedCostPerInterval}");
                    Console.WriteLine($" variable cost per interval : ${Config.Default.VariableCostPerInterval}");
                    Console.WriteLine($" total cost per interval    : ${Config.Default.TotalCostPerInterval}");
                    Console.WriteLine($" current credit balance     : ${CurrentCreditBalance}");

                    // Compare to previous credit balance
                    if (Config.Default.PreviousCreditBalance > double.MinValue) {
                        Console.WriteLine($" previous credit balance    : ${Config.Default.PreviousCreditBalance}");

                        double CostForThisInterval = Config.Default.PreviousCreditBalance - CurrentCreditBalance;
                        double CostPerDay = CostForThisInterval * 24.0; // TODOX Assumes hourly interval
                        double CostPerMonth = CostPerDay * 365.0 / 12.0;

                        Console.WriteLine($" cost for this interval     : ${CostForThisInterval}");
                        Console.WriteLine($" cost per day               : ${CostPerDay}");
                        Console.WriteLine($" cost per month             : ${CostPerMonth}");

                        if (CostForThisInterval > Config.Default.TotalCostPerInterval) {
                            // We spent too much during this last interval
                            File.AppendAllText(DailyUsageFilename, $"OVERSPEND\t${CostForThisInterval}\t{CostPerDay}\t{CostPerMonth}\r\n");
                            SendEmail($"lndcreditwatch noticed you overspent.  At ${CostForThisInterval}/hr you'd be spending:\r\n${CostPerDay}/day\r\n${CostPerMonth}/month.\r\nCurrent balance: ${CurrentCreditBalance}");
                        } else {
                            // We were within our spending limit
                            File.AppendAllText(DailyUsageFilename, $"UNDERSPEND\t${CostForThisInterval}\t{CostPerDay}\t{CostPerMonth}\r\n");
                            if (Debugger.IsAttached) SendEmail($"lndcreditwatch noticed you underspent.  At ${CostForThisInterval}/hr you'd be spending:\r\n${CostPerDay}/day\r\n${CostPerMonth}/month.\r\nCurrent balance: ${CurrentCreditBalance}");
                        }
                    } else {
                        // No previous balance, so send an email alert saying watch has been setup
                        Console.WriteLine(" previous credit balance     : N/A (looks like this is our first run)");
                        SendEmail("lndcreditwatch is now monitoring your account!");
                    }

                    // Store current credit as previous credit
                    Config.Default.PreviousCreditBalance = CurrentCreditBalance;
                    Config.Default.Save();

                    Console.WriteLine(" done");
                }
            } catch (Exception ex) {
                SendEmail($"Exception running lndcreditwatch: {ex.ToString()}");
            }
        }

        private static void SendEmail(string body)
        {
            if (Config.Default.SmtpToAddress == "lndcreditwatch@localhost")
            {
                Console.WriteLine();
                Console.WriteLine("*******************************************************************************");
                Console.WriteLine(" please edit this file to customize your smtp settings to allow email delivery");
                Console.WriteLine(" (sorry, it's not currently possible to specify a password, if your smtp server");
                Console.WriteLine(" requires it let me know and I'll look into making that configurable)");
                Console.WriteLine($" {Config.Default.FileName}");
                Console.WriteLine("*******************************************************************************");
                Console.WriteLine();
            }
            else
            {
                try
                {
                    WebUtils.Email(Config.Default.SmtpHostname,
                        Config.Default.SmtpPort,
                        new MailAddress(Config.Default.SmtpFromAddress),
                        new MailAddress(Config.Default.SmtpToAddress),
                        new MailAddress(Config.Default.SmtpFromAddress),
                        "lndcreditwatch notification",
                        body,
                        false,
                        Config.Default.SmtpUsername,
                        Config.Default.SmtpPassword,
                        Config.Default.SmtpSsl
                    );
                    Console.WriteLine($" email notification sent to {Config.Default.SmtpToAddress}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("*******************************************************************************");
                    Console.WriteLine(" there was an error sending the email notification");
                    Console.WriteLine(" please double check your settings in this file, and try again");
                    Console.WriteLine($" {Config.Default.FileName}");
                    Console.WriteLine(" error message:");
                    Console.WriteLine($" {ex.Message}");
                    Console.WriteLine("*******************************************************************************");
                    Console.WriteLine();
                }
            }
        }
    }
}
