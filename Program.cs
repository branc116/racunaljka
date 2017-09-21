using System;
using System.Collections.Generic;
using System.Linq;
namespace Racunaljka
{
    public class Row{
        public List<int> Nums;
        public int RowSum;
        public IEnumerable<int> UsedNums => Nums.Where(i => i != -1);
        public int CurSum => UsedNums.Sum();
        public int NumberOfNotUsedPositions => Nums.Count(i => i == -1);
        
        public Row Copy => new Row()
        {
            Nums = new List<int>(Nums),
            RowSum = RowSum
        };
        public IEnumerable<int> PossibleNums()
        {
            int cur = RowSum - CurSum;
            while(cur > 0)
            {
                if (cur < 20 && !UsedNums.Any(i => i == cur))
                {
                    yield return cur;
                    
                }
                cur--;
            }
        }
        public override string ToString(){
            var retStr = string.Empty;
            foreach(var num in Nums){
                retStr += $"{(num == -1 ? "? " : num.ToString() + (num < 10 ? " " : string.Empty))} ";
            }
            retStr += $"| {RowSum}";
            return retStr;
        }
    }
    public class Field {
        public List<Row> Rows;
        public List<int> ColumSum;
        private Field _lastStep;
        public string ActionTaken { get; set; }
        private int _lastIndex;
        private static HashSet<string> InvalidStates = new HashSet<string>();
        public IEnumerable<int> UsedNums => Rows.Select(i => i.UsedNums)
                                                .Aggregate((j, k) => j.Union(k));
        public int NumberOfSolvedFields => AllPossibleNums.Sum(i => i.Sum(j => j.Count == 1 ? 1 : 0));
        public int RowCount => Rows.Count;
        public int ColumnCount => ColumSum.Count;

        public int CurSumInColumn(int column) => Rows.Select(i => i.Nums[column])
                                                     .Sum();
        public Row GetColumn(int column) => new Row()
        {
            Nums = Rows.Select(i => i.Nums[column]).ToList(),
            RowSum = ColumSum[column]
        };
        public List<List<List<int>>> AllPossibleNums => _allPossibleNums == null ? _allPossibleNums = (from row in Enumerable.Range(0, RowCount)
                                                            let PossibleInOneRow = (from column in Enumerable.Range(0, ColumnCount)
                                                                                    select PossibleNums(row, column).ToList()).ToList()
                                                            select PossibleInOneRow).ToList() 
                                                                                 : _allPossibleNums;
        private List<List<List<int>>> _allPossibleNums;
        public int MinPossibleMoves => AllPossibleNums.Min(i => i.Min(j => j.Count == 1 ? int.MaxValue : j.Count));
        public bool IsValid => !InvalidStates.Contains(this.ToString()) && !AllPossibleNums.Any(i => i.Any(j => j.Count == 0)) && CheckSums();
        public bool CheckSums()
        {
            var colSum = new List<int>(ColumSum);
            var rowSum = (from row in Rows
                         select row.RowSum).ToList();
            var nums = new HashSet<int>();
            for(int i = 0; i < _allPossibleNums.Count; i++)
            {
                for(int j = 0; j < AllPossibleNums[i].Count; j++)
                {
                    int cur = AllPossibleNums[i][j].Count != 1 ? -2 : AllPossibleNums[i][j][0];
                    if (cur != -2)
                    {
                        if (cur == -1)
                            return false;
                        rowSum[i] -= cur;
                        colSum[j] -= cur;
                        if (rowSum[i] < 0 || colSum[j] < 0)
                            return false;
                        if (nums.Contains(cur))
                            return false;
                        nums.Add(cur);
                    }
                }
            }
            return true;
        }
        public (int row, int column) BestFieldToPlay {
            get
            {
                var minPn = MinPossibleMoves;
                var apn = AllPossibleNums;
                for (int i =0; i < RowCount; i++)
                {
                    for (int j = 0; j < ColumnCount; j++)
                    {
                        if (AllPossibleNums[i][j].Count == minPn)
                        {
                            return (i, j);
                        }
                    }
                }
                return (-1, -1);
            }
        }
                                                                        
        public IEnumerable<int> PossibleNums(int row, int column)
        {
            if (Rows[row].Nums[column] != -1)
                yield return Rows[row].Nums[column];
            else
            {
                var usedNums = UsedNums.ToList();
                var rowPossibleNums = CreatableNumbers(Rows[row].RowSum - Rows[row].CurSum, usedNums, Rows[row].NumberOfNotUsedPositions);
                var columnRow = GetColumn(column);
                var columnPossibleNums = CreatableNumbers(columnRow.RowSum - columnRow.CurSum, usedNums, columnRow.NumberOfNotUsedPositions);
                foreach(var poss in columnPossibleNums.Intersect(rowPossibleNums))
                    yield return poss;
            }
        }
        //public (int row, int column)
        public string ToStringSlow()
        {
            var retStr = string.Empty;
            for (int i = 0; i < this.Rows.Count; i++)
            {
                var nums = new List<int>(ColumnCount);
                for (int j = 0; j < this.ColumSum.Count; j++)
                {
                    nums.Add(this.PossibleNums(i, j).First());
                }
                var row = new Row()
                {
                    Nums = nums,
                    RowSum = Rows[i].RowSum
                };
                retStr += row.ToString() + '\n';
            }
            foreach (var num in ColumSum)
            {
                retStr += $"{num} ";
            }
            return retStr;
        }

        public override string ToString(){
            var retStr = string.Empty;
            

            foreach (var row in Rows){
                retStr += row.ToString() + '\n';
            }
            foreach(var num in ColumSum)
            {
                retStr += $"{num} ";
            }
            return retStr;
        }
        public void WritePossibleMoves()
        {
            for (int i = 0; i < this.Rows.Count; i++)
            {
                for (int j = 0; j < this.ColumSum.Count; j++)
                {
                    System.Console.Write($"({i}, {j}): ");
                    foreach (var possible in this.PossibleNums(i, j))
                    {
                        System.Console.Write($"{possible}, ");
                    }
                    System.Console.WriteLine();
                }
            }
        }
        public static IEnumerable<int> CreatableNumbers(int wantedSum, IEnumerable<int> UsedNums, int stepsLeft)
        {
            if (stepsLeft == 0 || wantedSum < 1)
                yield return -1;
            else if (stepsLeft == 1)
            {
                if (wantedSum <= 20 && !UsedNums.Any(i => i == wantedSum))
                    yield return wantedSum;
                else
                    yield return -1;
            }
            else
            {
                int curNum = 20;
                bool didAnything = false;
                while(curNum > 0)
                {
                    if (!UsedNums.Contains(curNum))
                    {
                        var rec = CreatableNumbers(wantedSum - curNum, UsedNums.Append(curNum), stepsLeft - 1).ToList();
                        if (rec.First() != -1)
                        {
                            didAnything = true;
                            foreach (var num in rec)
                            {
                                yield return num;
                            }
                        }
                    }
                    curNum--;
                }
                if (!didAnything)
                    yield return -1;
            }
        }
        public IEnumerable<Field> SolveByStep()
        {
            var FieldCopy =  Copy();
            var (rowBftp, columpBftp) = BestFieldToPlay;
            if (rowBftp == -1)
                yield return null;
            var possible = AllPossibleNums[rowBftp][columpBftp];

            for (int i = 0; i < possible.Count; i++) { 
                FieldCopy._lastStep = this;
                FieldCopy.Rows[rowBftp].Nums[columpBftp] = possible[i];
                _lastIndex = i;
                ActionTaken = $"({rowBftp}, {columpBftp}) = {possible[i]}";
                yield return FieldCopy;
            }
        }
        public Field GoBack()
        {
            if (_lastStep == null)
                return this;
            var (r, c) = _lastStep.BestFieldToPlay;
            _lastStep.Rows[r].Nums[c] = -1;
            _lastStep._allPossibleNums[r][c].RemoveAt(_lastIndex);
            InvalidStates.Add(this.ToString());
            return _lastStep.BestFieldToPlay.column == -1 || _lastStep.BestFieldToPlay.row == -1 ? _lastStep.GoBack() : _lastStep;
        }
        public int StackSize()
        {
            return 1 + (_lastStep != null ? _lastStep.StackSize() : 0);
        }
        public string ActionsTakenStack()
        {
            var possible = string.Empty;
            var (r, c) = BestFieldToPlay;
            var poss = AllPossibleNums[r][c];
            foreach(var p in poss)
            {
                possible += $"{p} ";
            }
            return $"{ActionTaken} ({possible})\n{_lastStep?.ActionsTakenStack() ?? ("\n End of Action Stack\n" + new string('-',30))}";
        }
        public void SaveInvalidStates()
        {
            System.IO.File.WriteAllText("InvalidText.txt", InvalidStates.Aggregate((i, j) => $"{i}{Environment.NewLine}{j}"));
        }
        public void RestoreInvalidStates()
        {
            InvalidStates = new HashSet<string>(System.IO.File.ReadAllLines("InvalidText.txt"));
        }
        public Field Copy()
        {
            return new Field()
            {
                ColumSum = new List<int>(this.ColumSum),
                Rows = (from row in Rows
                        select row.Copy).ToList(),
                _allPossibleNums = null,
                
            };
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var field = System.IO.File.ReadAllText(args.Length >= 1 ? args[0] : "test.txt");
            var rows = field.Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            var PlayingField = new Field(){
                Rows= (from row in rows
                      where row[0] != '@'
                      let split = row.Split(':')
                      let nums = from num in split[0].Split(' ')
                                 select int.Parse(num)
                      let sum = int.Parse(split[1])
                      select new Row(){
                          Nums = nums.ToList(),
                          RowSum = sum
                      }).ToList(),
                ColumSum = (from row in rows
                           where row[0] == '@'
                           let nums = from num in row.Substring(1).Split(' ')
                                      select int.Parse(num)
                           select nums)
                           .First()
                           .ToList()
            };
            System.Console.WriteLine(PlayingField);
            PlayingField.WritePossibleMoves();
            var (rowBftp, columnBftp) = PlayingField.BestFieldToPlay;
            System.Console.WriteLine($"Best field to play is ({rowBftp}, {columnBftp})");
            var fieldStep = PlayingField.SolveByStep().First();
            System.Console.WriteLine(fieldStep);
            fieldStep.WritePossibleMoves();
            for(int i = 0; i < 10;)
            {
                (rowBftp, columnBftp) = fieldStep.BestFieldToPlay;
                System.Console.WriteLine($"Best field to play is ({rowBftp}, {columnBftp})");
                var newField = fieldStep.SolveByStep().First();
                if (newField == null || !newField.IsValid)
                {
                    //i-=2;
                    Console.WriteLine("Think not, this path is invalid, going back");
                    fieldStep = newField?.GoBack() ?? fieldStep.GoBack();
                    Console.WriteLine(fieldStep.ActionsTakenStack());
                }
                else
                {
                    fieldStep = newField;
                    if (fieldStep.IsValid && fieldStep.NumberOfSolvedFields > 19)
                    {
                        fieldStep.WritePossibleMoves();
                        Console.WriteLine("Found the solution:\n");
                        Console.WriteLine(fieldStep.ToStringSlow());
                        Console.WriteLine($"Used Nums = {fieldStep.NumberOfSolvedFields}");
                        Console.WriteLine("\n\n");
                        return;

                    }
                    System.Console.WriteLine(fieldStep);

                    fieldStep.WritePossibleMoves();
                    //Console.WriteLine();
                    //Console.WriteLine();
                    //16 17 ? 5 3 | 51
                    //4 2 7 19 14 ? | 54
                    //18 ? 11 ? 9 | 58
                    //? 1 13 15 ? | 47
                    //50 32 41 51 36 }
                }
                
            }

        }
    }
}
