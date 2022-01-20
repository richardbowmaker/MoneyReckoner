using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MoneyReckoner
{
    public class Data
    {
        private static List<StatementEntry> _statement = new List<StatementEntry>();
        private static List<WeeklySummary> _santanderSummaries = new List<WeeklySummary>();

        public static void Clear()
        {
            _statement.Clear();
            _santanderSummaries.Clear();
        }

        public static void Serialise(string filename, bool read)
        {
            const string title = "Money Reckoner 1.00";

            if (read)
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(filename);
                    string s = sr.ReadLine();

                    if (s != title)
                    {
                        Main.This.Log("Invalid file format, " + filename);
                        return;
                    }

                    Clear();
                    while (!sr.EndOfStream)
                    {
                        StatementEntry entry = new StatementEntry(sr.ReadLine());
                        _statement.Add(entry);
                    }
                    sr.Close();
                    Main.This.Log("Save file read OK, " + filename);
                }
                catch (Exception e)
                {
                    Main.This.Log("Error reading from file: " + filename);
                    Main.This.Log(e.Message);
                    if (sr != null) sr.Close();
                }
            }
            else
            {
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(filename);
                    sw.WriteLine(title);

                    foreach (StatementEntry entry in _statement)
                        sw.WriteLine(entry.ToString());

                    sw.Close();
                    Main.This.Log("Save file written OK, " + filename);
                }
                catch (Exception e)
                {
                    Main.This.Log("Error writing to file: " + filename);
                    Main.This.Log(e.Message);
                    if (sw != null) sw.Close();
                }
            }
        }

        public static void StatementCapture(string statement)
        {
            string[] lines = statement.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (line.Contains("09-01-28 43377166 - 123 CURRENT ACCOUNT"))
                {
                    ParseSantanderCurrentStatement(lines);
                    Main.This.SetSantanderCurrent();
                    break;
                }
                if (line.Contains("xxxx xxxx xxxx 3878 - SANTANDER 1 2 3 CASHBACK CARD"))
                {
                    ParseSantanderCreditCardStatement(lines);
                    Main.This.SetSantanderCredit();
                    break;
                }
                if (line.Contains("CashPlus Online Banking"))
                {
                    ParseCashPlusStatement(lines);
                    Main.This.SetCashplus();
                    break;
                }
            }
        }

        public static void ParseSantanderCurrentStatement(string[] lines)
        {
            bool data = false;

            foreach (string line in lines)
            {
                if (line == "Date	Description	Money in	Money out	Balance")
                {
                    data = true;
                    continue;
                }

                if (data)
                {
                    try
                    {
                        DateTime dt = DateTime.Parse(line.Substring(0, 10));
                        string[] fields = line.Split(new char[] { '\t' }, StringSplitOptions.None);

                        bool isExcluded = IsExclusion(fields[1]);

                        // 0 - date, 1 - description, 2 - credit, 3 - debit, 4 - balance
                        StatementEntry entry;
                        if (fields[2].Length == 0)
                        {
                            // debit
                            entry = new StatementEntry(
                                StatementEntry.AccountT.SantanderCurrent,
                                -decimal.Parse(fields[3], NumberStyles.Currency),
                                decimal.Parse(fields[4], NumberStyles.Currency),
                                dt,
                                fields[1],
                                CalculateWeekNo(dt),
                                isExcluded);
                        }
                        else
                        {
                            // credit
                            entry = new StatementEntry(
                                StatementEntry.AccountT.SantanderCurrent,
                                 decimal.Parse(fields[2], NumberStyles.Currency),
                                 decimal.Parse(fields[4], NumberStyles.Currency),
                                 dt,
                                 fields[1],
                                 CalculateWeekNo(dt),
                                 true);
                        }

                        // add entry, avoiding duplicates
                        if (!_statement.Exists(e => (e.CompareTo(entry) == 0)))
                            _statement.Add(entry);

                    }
                    catch (FormatException)
                    {
                        break;
                    }
                }
            }
            _statement.Sort();
        }

        public static void ParseSantanderCreditCardStatement(string[] lines)
        {
            bool data = false;

            foreach (string line in lines)
            {
                if (line == "Date	Card no.	Description	Money in	Money out")
                {
                    data = true;
                    continue;
                }

                if (data)
                {
                    try
                    {
                        if (line.Length < 10) throw new FormatException();
                        DateTime dt = DateTime.Parse(line.Substring(0, 10));
                        string[] fields = line.Split(new char[] { '\t' }, StringSplitOptions.None);

                        // 0 - date, 1 - card no, 2 - description, 3 - credit, 4 - debit
                        bool isExcluded = IsExclusion(fields[2]);

                        StatementEntry entry;
                        if (fields[3].Length == 0)
                        {
                            // debit
                            entry = new StatementEntry(
                                StatementEntry.AccountT.SantanderCredit,
                                -decimal.Parse(fields[4], NumberStyles.Currency),
                                0,
                                dt,
                                fields[2],
                                CalculateWeekNo(dt),
                                isExcluded);
                        }
                        else
                        {
                            // credit
                            entry = new StatementEntry(
                                StatementEntry.AccountT.SantanderCredit,
                                decimal.Parse(fields[3], NumberStyles.Currency),
                                0,
                                dt,
                                fields[2],
                                CalculateWeekNo(dt),
                                true);
                        }

                        // add entry, avoiding duplicates
                        if (!_statement.Exists(e => (e.CompareTo(entry) == 0)))
                            _statement.Add(entry);

                    }
                    catch (FormatException)
                    {
                        // invalid parse of data, => end of statement entries
                        break;
                    }
                }
            }
            _statement.Sort();
        }

        public static void ParseCashPlusStatement(string[] lines)
        {
            bool data = false;
            int ln = 0;

            while (ln < lines.Length)
            {
                string line = lines[ln];

                if (line == "Balance")
                {
                    data = true;
                    ln++;
                    continue;
                }

                if (data)
                {
                    try
                    {
                        if (line.Length < 10) throw new FormatException();
                        DateTime dt = DateTime.Parse(line.Substring(0, 10));

                        string description = lines[++ln];
                        bool isExcluded = IsExclusion(description);

                        // 3 elements = credit, 2 = debit
                        string[] fs = lines[++ln].Split(new char[] { '\t' });
                        StatementEntry entry;

                        if (fs.Length == 2)
                        {
                            // credit
                            entry = new StatementEntry(
                                    StatementEntry.AccountT.Cashplus,
                                    -decimal.Parse(fs[0], NumberStyles.Currency),
                                    decimal.Parse(fs[1], NumberStyles.Currency), 
                                    dt, 
                                    description, 
                                    CalculateWeekNo(dt), 
                                    isExcluded);
                        }
                        else
                        {
                            // debit
                            entry = new StatementEntry(
                                    StatementEntry.AccountT.Cashplus,
                                    decimal.Parse(fs[0], NumberStyles.Currency),
                                    decimal.Parse(fs[2], NumberStyles.Currency),
                                    dt,
                                    description,
                                    CalculateWeekNo(dt),
                                    isExcluded);
                        }

                        // add entry, avoiding duplicates
                        if (!_statement.Exists(e => (e.CompareTo(entry) == 0)))
                            _statement.Add(entry);

                    }
                    catch (FormatException)
                    {
                        break;
                    }
                }
                ln++;
            }
            _statement.Sort();
        }

        public static void StatementToLog()
        {
            foreach (StatementEntry entry in _statement)
                Main.This.Log(entry.ToString());
        }

        public static void SummariesToLog()
        {
            foreach (WeeklySummary summary in _santanderSummaries)
                Main.This.Log(summary.ToString());
        }

        public static void GenerateWeeklySummaries()
        {
            int weekNo = 0;
            _santanderSummaries = new List<WeeklySummary>();
            WeeklySummary summary = new WeeklySummary();

            foreach (StatementEntry entry in _statement)
            {
                if (entry.WeekNo != weekNo)
                {
                    weekNo = entry.WeekNo;

                    // save previous summary and start a new one
                    if (!summary.IsEmpty())
                        _santanderSummaries.Add(summary);
                    summary = new WeeklySummary(entry.WeekNo);
                }

                summary.AddEntry(entry);
            }
            if (!summary.IsEmpty())
                _santanderSummaries.Add(summary);
        }

        // 4 days left in 2015 from last monday
        private const int _daysBase = -5;
        private const int _yearsBase = 2015;
        private static List<int> _daysFromBase;

        public static int CalculateWeekNo(DateTime date)
        {
            if (_daysFromBase == null)
            {
                // 4 days left in 2015 from last monday
                _daysFromBase = new List<int>();
                int b = _daysBase;
                _daysFromBase.Add(b);

                for (int year = _yearsBase + 1; year < 2030; year++)
                {
                    DateTime dt = new DateTime(year, 12, 31);
                    b += dt.DayOfYear;
                    _daysFromBase.Add(b);
                }
            }

            int n = date.Year - _yearsBase;

            if (n < _daysFromBase.Count)
            {
                int d = _daysFromBase[n] + date.DayOfYear;
                return d / 7;
            }
            else
                throw new Exception("CalculateWeekNo, invalid year");
        }

        public static bool IsExclusion(string description)
        {
            return 

                // santander current
                description.Contains("LEEDS BUILDING SOC") ||
                description.Contains("MANDATE NO 34")      ||  // cashplus
                description.Contains("EDF ENERGY")         ||
                description.Contains("WILTSHIRE COUNCIL")  ||
                description.Contains("BT GROUP PLC")       ||
                description.Contains("CAMELOT LOTTERY")    ||
                description.Contains("BRISTOLWESSEXWATER") ||
                description.Contains("SANTANDERCARDS")     ||
                description.Contains("WINDOW PAYNE")       ||

                // santander credit
                description.Contains("INITIAL BALANCE");
        }
    }
 
    public class StatementEntry : IComparable
    {
        private static int nextId = 0;

        public enum AccountT
        {
            SantanderCurrent = 0,
            SantanderCredit = 1,
            Cashplus = 2
        }

        public AccountT Type;
        public decimal Amount;   // pence, -ve debit
        public decimal Balance;
        public DateTime Date;
        public int Id;
        public string Description;
        public int WeekNo;      // week no starting from first monday in year 2015, jan 5th
        public bool IsExcluded;

        public StatementEntry(AccountT type, decimal amount, decimal balance, DateTime date, string description, int weekNo, bool isExcluded)
        {
            Type = type;
            Amount = amount;
            Balance = balance;
            Date = date;
            Id = nextId++;
            Description = description;
            WeekNo = weekNo;

            if (Amount < 0)
                IsExcluded = isExcluded;
            else
                IsExcluded = true; // a credit is excluded
        }

        public StatementEntry(string s)
        {
            FromString(s);
        }

        public override string ToString()
        {
            string s;

            if (IsExcluded) s = "  "; else s = "* ";

            switch (Type)
            {
                case AccountT.SantanderCurrent: s += "Santander Current Account"; break;
                case AccountT.SantanderCredit:  s += "Santander Credit Card"; break;
                case AccountT.Cashplus:         s += "Cashplus"; break;
                default: s += "Unknown"; break;
            }

            s += ", " + 
                Amount.ToString("0.00") + ", " +
                Balance.ToString("0.00") + ", " +
                Date.ToString("ddd dd/M/yyyy", CultureInfo.InvariantCulture) + ", " +
                WeekNo.ToString() + ", " +
                Id.ToString() + ", " +
                Description;

            return s;
        }

        public void FromString(string s)
        {
            // "* Cashplus, -1.1, 44.43, Mon 27/12/2021, 364, 19, Fin: MIPERMIT,UNIT 7 CALLOW HI,CHIPPENHA"	

            try
            {
                IsExcluded = !s.StartsWith("*");
                string[] ts = s.Substring(2).Split(new char[] { ',' });

                if (ts[0] == "Santander Current Account") Type = AccountT.SantanderCurrent;
                if (ts[0] == "Santander Credit Card") Type = AccountT.SantanderCredit;
                if (ts[0] == "Cashplus") Type = AccountT.Cashplus;

                Amount = decimal.Parse(ts[1], NumberStyles.Float);
                Balance = decimal.Parse(ts[2], NumberStyles.Float);
                Date = DateTime.Parse(ts[3].Substring(5));
                WeekNo = Convert.ToInt32(ts[4]);
                Id = Convert.ToInt32(ts[5]);
                Description = ts[6];
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int CompareTo(object obj)
        {
            StatementEntry other = (StatementEntry)obj;

            if (WeekNo != other.WeekNo) return WeekNo.CompareTo(other.WeekNo);

            if (Type != other.Type) return Type.CompareTo(other.Type);

            // the same entry
            if (Date == other.Date && Amount == other.Amount && Balance == other.Balance) return 0;

            if (Date < other.Date) return -1;
            if (Date > other.Date) return 1;

            if (Type == AccountT.SantanderCurrent)
                // reverse the order of entries, they appear newest first
                // in the current account statement
                return other.Id.CompareTo(Id);
            else
                return Id.CompareTo(other.Id);
        }

        public bool IsDebit()
        {
            return Amount <= 0;
        }
    }
 
    public class WeeklySummary
    {
        public DateTime Date;
        public int WeekNo;
        public decimal TotalDebits;
        public List<StatementEntry> Entries;

        public WeeklySummary()
        {
            Date = DateTime.MaxValue;
            WeekNo = 0;
            TotalDebits = 0;
            Entries = new List<StatementEntry>();
        }

        public WeeklySummary(int weekNo)
        {
            Date = DateTime.MaxValue;
            WeekNo = weekNo;
            TotalDebits = 0;
            Entries = new List<StatementEntry>();
        }

        public void AddEntry(StatementEntry entry)
        {
            if (entry.Date < Date) Date = entry.Date;
            Entries.Add(entry);
            if (!entry.IsExcluded)
                TotalDebits -= entry.Amount;
        }

        public bool IsEmpty()
        {
            return Entries.Count == 0;
        }

        public override string ToString()
        {
            string s = 
                 Date.ToString("ddd dd/M/yyyy", CultureInfo.InvariantCulture) + ", " +
                 WeekNo.ToString() + ", " +
                 TotalDebits.ToString("#.##") +
                 Environment.NewLine;

            foreach (StatementEntry entry in Entries)
                s += "    " + entry.ToString() + Environment.NewLine;

            return s;
        }
    }
}
