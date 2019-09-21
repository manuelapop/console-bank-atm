using System;
using System.Collections.Generic;
using System.Linq;

namespace BankAccount
{
    public class BackAcountInfo
    {
        public string acountNumber { get; set; }
        public int pinNumber { get; set; }
        public double AmountLatest { get; set; }
        public List<string> Transactions { get; set; }
    }

    class Program
    {
        static String cur = "$";
        static String userName = string.Empty;
        static void Main(string[] args)
        {
            mainOperation();
        }

        private static void showMenu1()
        {
            Console.WriteLine(" ------------------------");
            Console.WriteLine("| Maybank ATM Main Menu  |");
            Console.WriteLine("|                        |");
            Console.WriteLine("| 1. Create Account      |");
            Console.WriteLine("| 2. Login               |");
            Console.WriteLine("| 3. Exit                |");
            Console.WriteLine("|                        |");
            Console.WriteLine(" ------------------------");
            Console.Write("Enter your option: ");
        }

        private static void showMenu2()
        {
            Console.WriteLine(" ---------------------------");
            Console.WriteLine("| Maybank ATM Secure Menu    |");
            Console.WriteLine("|                            |");
            Console.WriteLine("| 1. Check balance           |");
            Console.WriteLine("| 2. Deposit                 |");
            Console.WriteLine("| 3. Withdraw                |");
            Console.WriteLine("| 4. Logout                  |");
            Console.WriteLine("|                            |");
            Console.WriteLine(" ---------------------------");
            Console.Write("Enter your option: ");
        }

        private static void printNewBalance(double newBalance)
        {
            Console.WriteLine("Balance is: $" + newBalance.ToString("N"));
            Console.WriteLine();
        }

        private static bool validateInput(string input)
        {
            bool passValidation = false;
            int myInt = 0;
            if (!String.IsNullOrWhiteSpace(input))
            {
                if (int.TryParse(input, out myInt))
                    passValidation = true;
            }
            return passValidation;
        }

        private static void deposit()
        {
            double depositAmt = 0;
            var writeLn = String.Format("Enter the amount to deposit: {0}", cur);
            Console.Write(writeLn);
            var input = Console.ReadLine();
            while (!validateInput(input) || Convert.ToDouble(input) <= 0)
            {
                Console.WriteLine("Enter the amount to deposit: {0}", cur);
                input = Console.ReadLine();
            }
            depositAmt = Convert.ToDouble(input);
            if (depositAmt > 0)
            {
                List<BackAcountInfo> accounts = CacheHelper.GetFromCache<List<BackAcountInfo>>("accountInfo");
                var account = accounts.Where(x => x.acountNumber == userName).FirstOrDefault();
                if (account != null)
                {
                    var latestAccountAmt = account.AmountLatest;
                    var amount = latestAccountAmt + depositAmt;
                    account.AmountLatest = amount;
                    CacheHelper.SaveTocache("accountInfo", accounts, new DateTime(2100, 01, 01));
                    writeLn = String.Format("You have successfully deposited {1}{0} \n", depositAmt, cur);
                    Console.Write(writeLn);
                    printNewBalance(amount);
                }
                else
                {
                    Console.WriteLine("Invalid account. Try again.");
                }
            }
            else
            {
                Console.WriteLine("Invalid deposit amount. Try again.");
            }
        }

        private static void withdraw()
        {
            double withdrawAmt = 0;
            double minimumKeptAmt = 20;

            var writeLn = String.Format("Enter the amount to withdraw multiple of 20$: {0}", cur);
            Console.Write(writeLn);
            var input = Console.ReadLine();
            while (!validateInput(input) || Convert.ToDouble(input) % 20 != 0)
            {
                Console.WriteLine("Enter the amount to withdraw multiple of 20$: {0}", cur);
                input = Console.ReadLine();
            }
            withdrawAmt = Convert.ToDouble(input);

            List<BackAcountInfo> accounts = CacheHelper.GetFromCache<List<BackAcountInfo>>("accountInfo");
            var account = accounts.Where(x => x.acountNumber == userName).FirstOrDefault();
            if (account != null)
            {
                var latestAccountAmt = account.AmountLatest;

                if (withdrawAmt > 0)
                {
                    if (withdrawAmt > latestAccountAmt)
                    {
                        writeLn = String.Format("Insuficient funds {0}{1} \n", cur, withdrawAmt);
                        printNewBalance(latestAccountAmt);
                    }
                    else if ((latestAccountAmt - withdrawAmt) < minimumKeptAmt)
                    {
                        writeLn = String.Format("Insuficient funds {0}{1} \n", cur, minimumKeptAmt);
                        printNewBalance(latestAccountAmt);
                    }
                    else
                    {
                        var amount = latestAccountAmt - withdrawAmt;
                        account.AmountLatest = amount;
                        writeLn = String.Format("Please collect your money. \n");
                        CacheHelper.SaveTocache("accountInfo", accounts, new DateTime(2100, 01, 01));
                        printNewBalance(amount);
                    }
                    Console.Write(writeLn);
                }
                else
                {
                    Console.WriteLine("Invalid deposit amount. Try again.");
                }
            }
            else
            {
                Console.WriteLine("Invalid account. Try again.");
            }
        }

        private static void createBankAccount(string userName, int pwd)
        {
            var bnkAcc = new BackAcountInfo { acountNumber = userName, pinNumber = pwd, AmountLatest = 0, Transactions = new List<string>() };
            List<BackAcountInfo> accounts;
            if (CacheHelper.IsIncache("accountInfo"))
            {
                accounts = CacheHelper.GetFromCache<List<BackAcountInfo>>("accountInfo");
            }
            else
            {
                accounts = new List<BackAcountInfo>();
            }
            accounts.Add(bnkAcc);
            CacheHelper.SaveTocache("accountInfo", accounts, new DateTime(2100, 01, 01));
        }

        private static bool checkCardNoPassword(string cardNo, int pwd)
        {
            bool pass = false;
            var users = CacheHelper.GetFromCache<List<BackAcountInfo>>("accountInfo");
            if (CacheHelper.IsIncache("accountInfo"))
            {
                foreach (var user in users)
                {
                    if (user.acountNumber == cardNo && user.pinNumber == pwd)
                    {
                        pass = true;
                        userName = cardNo;
                    }
                }
            }
            return pass;
        }

        private static bool checkCardNo(string cardNo)
        {
            bool pass = false;
            var users = CacheHelper.GetFromCache<List<BackAcountInfo>>("accountInfo");
            if (CacheHelper.IsIncache("accountInfo"))
            {
                foreach (var user in users)
                {
                    if (user.acountNumber == cardNo)
                    {
                        pass = true;
                    }
                }
            }
            return pass;
        }

        private static void mainOperation()
        {
            var menu0 = string.Empty;
            var menu1 = 0;
            var menu2 = 0;
            var cardNo = string.Empty;
            var pin = 0;
            var tries = 0;
            var maxTries = 3;

            do
            {
                showMenu1();
                menu0 = Console.ReadLine();
                if (!validateInput(menu0))
                {
                    Console.WriteLine("Invalid Option Entered.");
                }
                else
                {
                    menu1 = Convert.ToInt32(menu0);
                    switch (menu1)
                    {
                        case 1:
                            Console.WriteLine("");
                            Console.Write("Enter a User Name with at least 4 digits: ");
                            cardNo = Console.ReadLine();
                            while (cardNo.Length < 4)
                            {
                                Console.WriteLine("Please enter a User Name with at least 4 digits.");

                                cardNo = Console.ReadLine();
                            }
                            Console.Write("Enter 4 Digit PIN: ");
                            var input = Console.ReadLine();
                            while (input.Length < 4)
                            {
                                Console.WriteLine("Please enter a 4 digit pin number.");

                                input = Console.ReadLine();
                            }
                            pin = Convert.ToInt32(input);
                            if (!checkCardNo(cardNo))
                            {
                                createBankAccount(cardNo, pin);
                            }
                            else
                            {
                                Console.Write("UserName already in Use");
                            }
                            break;
                        case 2:
                            Console.WriteLine("");
                            Console.Write("Enter User Name: ");
                            cardNo = Console.ReadLine();
                            Console.Write("Enter 4 Digit PIN: ");
                            pin = Convert.ToInt32(Console.ReadLine());

                            if (checkCardNoPassword(cardNo, pin))
                            {
                                do
                                {
                                    showMenu2();
                                    menu0 = Console.ReadLine();
                                    if (!validateInput(menu0))
                                    {
                                        Console.WriteLine("Invalid Option Entered.");
                                    }
                                    else
                                    {
                                        menu2 = Convert.ToInt32(menu0);
                                        switch (menu2)
                                        {
                                            case 1:
                                                List<BackAcountInfo> accounts = CacheHelper.GetFromCache<List<BackAcountInfo>>("accountInfo");
                                                var account = accounts.Where(x => x.acountNumber == userName).FirstOrDefault();
                                                if (account != null)
                                                {
                                                    printNewBalance(account.AmountLatest);
                                                }
                                                break;
                                            case 2:
                                                deposit();
                                                break;
                                            case 3:
                                                withdraw();
                                                break;
                                            case 4:
                                                Console.WriteLine("You have succesfully logout.");
                                                break;
                                            default:
                                                Console.WriteLine("Invalid Option Entered.");
                                                break;
                                        }
                                    }

                                } while (menu2 != 4);
                            }
                            else
                            {
                                tries++;

                                if (tries >= maxTries)
                                {
                                    Console.WriteLine("Account locked. Please go to the nearest Maybank branch to reset your PIN.");
                                    Console.WriteLine("Thank you for using Maybank. ");
                                    System.Environment.Exit(1);
                                }

                                Console.WriteLine("Invalid PIN.");
                            }

                            break;
                        case 3:
                            break;
                        default:
                            Console.WriteLine("Invalid Option Entered.");
                            break;
                    }
                }

            } while (menu1 != 3);
            Console.WriteLine("Thank you for using Maybank. ");

        }
    }
}
