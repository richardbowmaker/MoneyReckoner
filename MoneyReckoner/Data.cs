using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MoneyReckoner
{
    public class Data
    {
        private static List<StatementEntry> _statement = new List<StatementEntry>();
        private static bool _isDirty = false;
        private static List<Summary> _weeklySummaries = new List<Summary>();
        private static List<Summary> _monthlySummaries = new List<Summary>();

        public static bool IsDirty
        {
            get { return _isDirty; }
        }

        public static int Count
        {
            get { return _statement.Count; }
        }

        public static void Clear()
        {
            _isDirty = true;
            _statement.Clear();
            _weeklySummaries.Clear();
            _monthlySummaries.Clear();
        }

        public static void AddEntry(StatementEntry entry)
        {
            _isDirty = true;

            // add to statement in correct order
            int n = 0;
            for (n = 0; n < _statement.Count; ++n)
            {
                if (_statement[n].CompareTo(entry) >= 0)
                {
                    _statement.Insert(n, entry);
                    break;
                }
            }
            if (n == _statement.Count)
                _statement.Add(entry);
        }

        public static void AddEntries(List<StatementEntry> entries)
        {
            foreach (StatementEntry e in entries)
                AddEntry(e);
        }

        public static void Serialise(string filename, bool read)
        {
            const string title = "Money Reckoner 1.10";

            if (read)
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(filename);
                    string s = sr.ReadLine();

                    if (s != title)
                    {
                        Logger.Error("Invalid file format, " + filename);
                        return;
                    }

                    Clear();
                    while (!sr.EndOfStream)
                    {
                        StatementEntry entry = new StatementEntry(sr.ReadLine());
                        AddEntry(entry);
                    }
                    sr.Close();
                    _isDirty = false;
                    Logger.Info("Save file read OK, " + filename);
                }
                catch (Exception e)
                {
                    Logger.Error("Error reading from file: " + filename);
                    Logger.Error(e.Message);
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
                    Logger.Info("Save file written OK, " + filename);
                }
                catch (Exception e)
                {
                    Logger.Error("Error writing to file: " + filename);
                    Logger.Error(e.Message);
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
                    Main._this.SetSantanderCurrent();
                    break;
                }
                if (line.Contains("xxxx xxxx xxxx 3878 - SANTANDER 1 2 3 CASHBACK CARD"))
                {
                    ParseSantanderCreditCardStatement(lines);
                    Main._this.SetSantanderCredit();
                    break;
                }
                if (line.Contains("CashPlus Online Banking"))
                {
                    ParseCashPlusStatement(lines);
                    Main._this.SetCashplus();
                    break;
                }
            }
        }

        public static void ParseSantanderCurrentStatement(string[] lines)
        {
            bool data = false;
            List<StatementEntry> newEntries = new List<StatementEntry>();
 
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
                                CalculateWeekNo(dt)
                                );
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
                                 CalculateWeekNo(dt));
                        }

                        // add entry, avoiding duplicates
                        if (!_statement.Exists(e => (e.CompareTo(entry) == 0)))
                            newEntries.Add(entry);

                    }
                    catch (FormatException)
                    {
                        break;
                    }
                }
            }

            newEntries.Sort();
            AddEntries(newEntries);

            // log info about data captured
            Logger.Info("Santander current account parsed");
            EntriesSummaryToLogger(newEntries);
        }

        public static void ParseSantanderCreditCardStatement(string[] lines)
        {
            bool data = false;
            List<StatementEntry> newEntries = new List<StatementEntry>();

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
                                CalculateWeekNo(dt));
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
                                CalculateWeekNo(dt));
                        }

                        // add entry, avoiding duplicates
                        if (!_statement.Exists(e => (e.CompareTo(entry) == 0)))
                            newEntries.Add(entry);

                    }
                    catch (FormatException)
                    {
                        // invalid parse of data, => end of statement entries
                        break;
                    }
                }
            }

            newEntries.Sort();
            AddEntries(newEntries);

            // log info about data captured
            Logger.Info("Santander credit card parsed");
            EntriesSummaryToLogger(newEntries);
        }

        public static void ParseCashPlusStatement(string[] lines)
        {
            bool data = false;
            List<StatementEntry> newEntries = new List<StatementEntry>();
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

                        // 3 elements = credit, 2 = debit
                        string[] fs = lines[++ln].Split(new char[] { '\t' });
                        StatementEntry entry;

                        if (fs.Length == 2)
                        {
                            // credit
                            entry = new StatementEntry(
                                    StatementEntry.AccountT.Cashplus,
                                    -decimal.Parse(fs[0] + '0', NumberStyles.Currency),
                                    decimal.Parse(fs[1] + '0', NumberStyles.Currency),
                                    dt,
                                    description,
                                    CalculateWeekNo(dt));
                        }
                        else
                        {
                            // debit
                            entry = new StatementEntry(
                                    StatementEntry.AccountT.Cashplus,
                                    decimal.Parse(fs[0] + '0', NumberStyles.Currency),
                                    decimal.Parse(fs[2] + '0', NumberStyles.Currency),
                                    dt,
                                    description,
                                    CalculateWeekNo(dt));
                        }

                        // add entry, avoiding duplicates
                        if (!_statement.Exists(e => (e.CompareTo(entry) == 0)))
                            newEntries.Add(entry);

                    }
                    catch (FormatException)
                    {
                        break;
                    }
                }
                ln++;
            }

            newEntries.Sort();
            AddEntries(newEntries);

            // log info about data captured
            Logger.Info("Cashplus account parsed");
            EntriesSummaryToLogger(newEntries);
        }

        public static void StatementSummaryToLog()
        {
            Logger.Info("Statement summary");
            EntriesSummaryToLogger(_statement);
        }

        private static void EntriesSummaryToLogger(List<StatementEntry> entries)
        {
            if (entries.Count == 0)
                Logger.Info("  No transactions");
            else
            {
                Logger.Info(
                    "  " + entries.Count.ToString() + " transactions " +
                    " from " + entries[0].Date.ToString("ddd dd/M/yyyy", CultureInfo.InvariantCulture) +
                    " to " + entries[entries.Count - 1].Date.ToString("ddd dd/M/yyyy", CultureInfo.InvariantCulture));
            }
        }

        public static void StatementToLog()
        {
            foreach (StatementEntry entry in _statement)
                Logger.Info(entry.ToString());
        }

 
        public static void GenerateWeeklySummaries()
        {
            _statement.Sort(OrderEntriesWeekly);
            int weekNo = 0;
            _weeklySummaries = new List<Summary>();
            Summary summary = new Summary();

            foreach (StatementEntry entry in _statement)
            {
                if (entry.WeekNo != weekNo)
                {
                    weekNo = entry.WeekNo;

                    // save previous summary and start a new one
                    if (!summary.IsEmpty())
                        _weeklySummaries.Add(summary);
                    summary = new Summary(entry.WeekNo);
                }

                // credits and excluded items are not included in the
                // total expenditure
                summary.AddEntry(entry, (entry.Amount >= 0) || IsWeeklyExclusion(entry));
            }
            if (!summary.IsEmpty())
                _weeklySummaries.Add(summary);
        }

        public static void GenerateMonthlySummaries()
        {
            _statement.Sort(OrderEntriesMonthly);
            int month = 0;
            _monthlySummaries = new List<Summary>();
            Summary summary = new Summary();

            foreach (StatementEntry entry in _statement)
            {
                if (entry.Date.Month != month)
                {
                    month = entry.Date.Month;

                    // save previous summary and start a new one
                    if (!summary.IsEmpty())
                        _monthlySummaries.Add(summary);
                    summary = new Summary(entry.Date.Month);
                }

                // only the santander current account is included in the monthly summary.
                // credits and excluded items are not included in the total expenditure.
                bool isExcluded = false;
                if (entry.Type != StatementEntry.AccountT.SantanderCurrent) isExcluded = true;
                if (entry.Amount >= 0) isExcluded = true;
                if (IsMonthlyExclusion(entry)) isExcluded = true;

                summary.AddEntry(entry, isExcluded);
            }
            if (!summary.IsEmpty())
                _monthlySummaries.Add(summary);
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

        public static bool IsWeeklyExclusion(StatementEntry entry)
        {
            return

                // santander current account
                entry.Description.Contains("LEEDS BUILDING SOC")    ||
                entry.Description.Contains("CASHPLUS")              ||  // cashplus
                entry.Description.Contains("EDF ENERGY")            ||
                entry.Description.Contains("WILTSHIRE COUNCIL")     ||
                entry.Description.Contains("BT GROUP PLC")          ||
                entry.Description.Contains("CAMELOT LOTTERY")       ||
                entry.Description.Contains("BRISTOLWESSEXWATER")    ||
                entry.Description.Contains("SANTANDERCARDS")        ||
                entry.Description.Contains("WINDOW PAYNE")          ||

                 // santander credit
                 entry.Description.Contains("INITIAL BALANCE");
        }
        public static bool IsMonthlyExclusion(StatementEntry entry)
        {
            return false;
        }

        public static int OrderEntriesMonthly(StatementEntry e1, StatementEntry e2)
        {
            // order by year, month, type of account, day of month, original statement order
            if (e1.Date.Year < e2.Date.Year) return -1;
            if (e1.Date.Year > e2.Date.Year) return 1;

            if (e1.Date.Month < e2.Date.Month) return -1;
            if (e1.Date.Month > e2.Date.Month) return 1;

            if (e1.Type == StatementEntry.AccountT.SantanderCurrent &&
                e2.Type != StatementEntry.AccountT.SantanderCurrent) return -1;

            if (e1.Type != StatementEntry.AccountT.SantanderCurrent &&
                e2.Type == StatementEntry.AccountT.SantanderCurrent) return 1;

            if (e1.Type < e2.Type) return -1;
            if (e1.Type > e2.Type) return 1;

            if (e1.Date.Day < e2.Date.Day) return -1;
            if (e1.Date.Day > e2.Date.Day) return 1;

            if (e1.Type == StatementEntry.AccountT.SantanderCurrent)
                // reverse the order of entries, they appear newest first
                // in the current account statement
                return e2.Id.CompareTo(e1.Id);
            else
                return e1.Id.CompareTo(e2.Id);
        }
        public static int OrderEntriesWeekly(StatementEntry e1, StatementEntry e2)
        {
            if (e1.WeekNo != e2.WeekNo) return e1.WeekNo.CompareTo(e2.WeekNo);

            if (e1.Type != e2.Type) return e1.Type.CompareTo(e2.Type);

            // the same entry
            if (e1.Date == e2.Date && e1.Amount == e2.Amount && e1.Balance == e2.Balance) return 0;

            if (e1.Date < e2.Date) return -1;
            if (e1.Date > e2.Date) return 1;

            if (e1.Type == StatementEntry.AccountT.SantanderCurrent)
                // reverse the order of entries, they appear newest first
                // in the current account statement
                return e2.Id.CompareTo(e1.Id);
            else
                return e1.Id.CompareTo(e2.Id);
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

        public StatementEntry(AccountT type, decimal amount, decimal balance, DateTime date, string description, int weekNo)
        {
            Type = type;
            Amount = amount;
            Balance = balance;
            Date = date;
            Id = nextId++;
            Description = description;
            WeekNo = weekNo;
        }

        public StatementEntry(string s)
        {
            FromString(s);
        }

        public override string ToString()
        {
            string s = "";

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
            // "Cashplus, -1.1, 44.43, Mon 27/12/2021, 364, 19, Fin: MIPERMIT,UNIT 7 CALLOW HI,CHIPPENHA"	

            try
            {
                string[] ts = s.Split(new char[] { ',' });

                if (ts[0] == "Santander Current Account") Type = AccountT.SantanderCurrent;
                if (ts[0] == "Santander Credit Card") Type = AccountT.SantanderCredit;
                if (ts[0] == "Cashplus") Type = AccountT.Cashplus;

                Amount = decimal.Parse(ts[1], NumberStyles.Float);
                Balance = decimal.Parse(ts[2], NumberStyles.Float);
                Date = DateTime.Parse(ts[3].Substring(5));
                WeekNo = Convert.ToInt32(ts[4]);
                Id = Convert.ToInt32(ts[5]);
                Description = ts[6].Trim();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int CompareTo(object obj)
        {
            return Data.OrderEntriesWeekly(this, (StatementEntry)obj);
        }

        public bool IsDebit()
        {
            return Amount <= 0;
        }
    }

    // Statement entries are references from both weekly and monthly statements,
    // however some entries are included in the mnothly summary but
    // not the weekly hence the IsIncluded flag differs according to context
    public class SummaryStatementEntry
    {
        public StatementEntry Entry;
        public bool IsIncluded;

        public SummaryStatementEntry(StatementEntry entry, bool isIncluded)
        {
            Entry = entry;
            IsIncluded = isIncluded;
        }

        public override string ToString()
        {
            string s = "";

            if (IsIncluded) s = "* "; else s = "  ";
            return s + Entry.ToString();
        }
    }

    public class Summary
    {
        public int Id;  // = week no. for weekly summary, month no. for monthly summary
        public DateTime Date;
        public decimal TotalDebits;
        public List<SummaryStatementEntry> Entries;

        public Summary()
        {
            Date = DateTime.MaxValue;
            Id = 0;
            TotalDebits = 0;
            Entries = new List<SummaryStatementEntry>();
        }

        public Summary(int id)
        {
            Date = DateTime.MaxValue;
            Id = id;
            TotalDebits = 0;
            Entries = new List<SummaryStatementEntry>();
        }

        public void AddEntry(StatementEntry entry, bool isExcluded)
        {
            if (entry.Date < Date) Date = entry.Date;
            Entries.Add(new SummaryStatementEntry(entry, !isExcluded));

            if (!isExcluded)
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
                 Id.ToString() + ", " +
                 TotalDebits.ToString("#.##") +
                 Environment.NewLine;

            foreach (SummaryStatementEntry entry in Entries)
                s += "    " + entry.ToString() + Environment.NewLine;

            return s;
        }

        public void ToLogger()
        {
            string s =
                 Date.ToString("ddd dd/M/yyyy", CultureInfo.InvariantCulture) + ", " +
                 Id.ToString() + ", " +
                 TotalDebits.ToString("#.##");

            Logger.Info(s);

            foreach (SummaryStatementEntry entry in Entries)
                Logger.Info("    " + entry.ToString());
        }
    }
}
