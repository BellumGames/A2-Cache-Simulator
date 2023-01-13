using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace A2
{
    public partial class A2 : Form
    {
        private int latenta, NR_PORT, FR, IRmax, IBS, N_PEN, NR_REG, FR_IC, SIZE_IC, FR_DC, SIZE_DC; //Parameters for simulation
        private List<string> loadList = new List<string>();
        private List<string> storeList = new List<string>();
        private List<string> branchList = new List<string>();
        private List<string> arythmeticList = new List<string>();
        private List<string> totalList = new List<string>();
        private List<string> issueRateList = new List<string>();
        private List<string> ticksList = new List<string>();
        //public int MissRateIC, MissRateDC, PercentageIBS_Empty, Influence_IRmax, OptimalREG_Number; //This should be outputed in results.csv maybe? Cosmin any ideas?

        private Dictionary<string, List<Instruction>> allTraceData = new Dictionary<string, List<Instruction>>();

        //date out:
        //rata de procesare (nr instr raportat la nr cicli de executie)
        //rata miss in IC si in DC (2 x rate de miss)
        //procent din timpul total cat buffer de prefetch (IBS) e gol
        //se vrea parametri optimi si factori de limitare in fiecare din cazuri

        //A2:
        //det. influenta nr max de instr ce pot fi trimise in exe asupra ratei de procesare (IRmax)(2.1)
        //(set limitat de reg) care e numarul optim de registri? :(2.2)
        //->varianta uniport cache date (DC): o singura instr cu ref la mem se poate executa
        //->varianta biport cache date (DC): 2 instr cu ref la mem se pot executa L + L sau L + S
        //Pt valoarea optima de la 2.2 a nr de reg. studiati rata de procesare (IRmax) pe cache date(DC) uni sau biport

        public A2()
        {
            InitializeComponent();
            IOneeded();
            SetComboFromHistory();
        }

        private void btnSTART_Click(object sender, EventArgs e)
        {
            if (!SafeStart())
            {
                textBoxConsole.Text = "You haven't selected enough parameters for simulation to start";
                return;
            }
            SetEnviroment();
            if (!ValidParameters())
            {
                textBoxConsole.Text = "Selected paramenters can't be used at simulation, try other combination.";
                return;
            }
            SetComboForFuture();
            ReadTraces();
            Simulate();
            WriteResults();
        }

        private void btnEXIT_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void IOneeded()
        {
            if (!File.Exists(@"..\..\..\output\history.txt"))
            {
                File.Create(@"..\..\..\output\history.txt");
            }
            if (!File.Exists(@"..\..\..\output\results.csv"))
            {
                File.Create(@"..\..\..\output\results.csv");
            }
        }

        private bool ValidParameters()
        {
            if (FR_IC != FR || FR_DC != FR)
                return false;
            if (IRmax > FR)
                return false;
            if (SIZE_IC > IBS || SIZE_DC > IBS)
                return false;
            if (NR_REG > IRmax)
                return false;
            return true;
        }

        private bool SafeStart()
        {
            if (
                latentaUpDown.Value >= 0 &&
                (uniportRadio.Checked == true || biportRadio.Checked == true) &&
                comboFR.SelectedIndex > -1 &&
                comboIRmax.SelectedIndex > -1 &&
                comboIBS.SelectedIndex > -1 &&
                comboN_PEN.SelectedIndex > -1 &&
                comboNR_REG.SelectedIndex > -1 &&

                comboFR_IC.SelectedIndex > -1 &&
                comboSIZE_IC.SelectedIndex > -1 &&
                comboFR_DC.SelectedIndex > -1 &&
                comboSIZE_DC.SelectedIndex > -1
            )
                return true;
            else
                return false;
        }

        private void SetEnviroment()
        {
            latenta = (int)latentaUpDown.Value;
            if (uniportRadio.Checked == true && biportRadio.Checked == false)
                NR_PORT = 1;
            else if (uniportRadio.Checked == false && biportRadio.Checked == true)
                NR_PORT = 2;
            FR = int.Parse((string)comboFR.SelectedItem);
            IRmax = int.Parse((string)comboIRmax.SelectedItem);
            IBS = int.Parse((string)comboIBS.SelectedItem);
            N_PEN = int.Parse((string)comboN_PEN.SelectedItem);
            NR_REG = int.Parse((string)comboNR_REG.SelectedItem);
            FR_IC = int.Parse((string)comboFR_IC.SelectedItem);
            SIZE_IC = int.Parse((string)comboSIZE_IC.SelectedItem);
            FR_DC = int.Parse((string)comboFR_DC.SelectedItem);
            SIZE_DC = int.Parse((string)comboSIZE_DC.SelectedItem);
        }

        private void SetComboForFuture()
        {
            using (StreamWriter sw = new StreamWriter(@"..\..\..\output\history.txt"))
            {
                sw.WriteLine($"latentaUpDown {latentaUpDown.Value}");
                if (uniportRadio.Checked == true && biportRadio.Checked == false)
                    sw.WriteLine($"uniportRadio_biportRadio {1}");
                else if (uniportRadio.Checked == false && biportRadio.Checked == true)
                    sw.WriteLine($"uniportRadio_biportRadio {2}");

                sw.WriteLine($"comboFR {comboFR.SelectedIndex}");
                sw.WriteLine($"comboIRmax {comboIRmax.SelectedIndex}");
                sw.WriteLine($"comboIBS {comboIBS.SelectedIndex}");
                sw.WriteLine($"comboN_PEN {comboN_PEN.SelectedIndex}");
                sw.WriteLine($"comboNR_REG {comboNR_REG.SelectedIndex}");

                sw.WriteLine($"comboFR_IC {comboFR_IC.SelectedIndex}");
                sw.WriteLine($"comboSIZE_IC {comboSIZE_IC.SelectedIndex}");
                sw.WriteLine($"comboFR_DC {comboFR_DC.SelectedIndex}");
                sw.WriteLine($"comboSIZE_DC {comboSIZE_DC.SelectedIndex}");
                sw.Close();
            }
        }

        private void SetComboFromHistory()
        {
            string allText = File.ReadAllText(@"..\..\..\output\history.txt");
            if (!allText.Equals(string.Empty))
            {
                string[] rows = allText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                int i = 0;
                foreach (string row in rows)
                {
                    string[] values = row.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    int value = int.Parse(values[1]);
                    switch (i)
                    {
                        case 0: latentaUpDown.Value = value; break;
                        case 1:
                            if (value == 1)
                            {
                                uniportRadio.Checked = true;
                                biportRadio.Checked = false;
                            }
                            else if (value == 2)
                            {
                                uniportRadio.Checked = false;
                                biportRadio.Checked = true;
                            }
                            break;
                        case 2: comboFR.SelectedIndex = value; break;
                        case 3: comboIRmax.SelectedIndex = value; break;
                        case 4: comboIBS.SelectedIndex = value; break;
                        case 5: comboN_PEN.SelectedIndex = value; break;
                        case 6: comboNR_REG.SelectedIndex = value; break;

                        case 7: comboFR_IC.SelectedIndex = value; break;
                        case 8: comboSIZE_IC.SelectedIndex = value; break;
                        case 9: comboFR_DC.SelectedIndex = value; break;
                        case 10: comboSIZE_DC.SelectedIndex = value; break;
                    }
                    i++;
                }
            }
        }

        private InstructionType? GetInstructionType(string instructionShortcut)
        {
            switch (instructionShortcut)
            {
                case "B":
                    return InstructionType.Branch;
                case "L":
                    return InstructionType.Load;
                case "S":
                    return InstructionType.Store;
                default: break;
            }
            return null;
        }

        private void ReadTrace(FileInfo file)
        {
            List<Instruction> instructionsFromBenchmark = new List<Instruction>();
            string filePath = file.FullName;
            List<string> fileContent = new List<string>();
            if (!string.IsNullOrEmpty(filePath))
            {
                fileContent = File.ReadAllText(filePath).Split(' ').Select(a => a.Trim()).ToList();
                fileContent = fileContent.Where(x => string.IsNullOrWhiteSpace(x) == false).ToList();
                for (int i = 0; i < fileContent.Count; i += 3)
                {
                    Instruction instruction = new Instruction
                    {
                        instructionType = GetInstructionType(fileContent[i]),
                        currentPC = Convert.ToUInt32(fileContent[i + 1]),
                        targetAddress = Convert.ToUInt32(fileContent[i + 2])
                    };
                    instructionsFromBenchmark.Add(instruction);
                }
            }
            allTraceData.Add(file.Name, instructionsFromBenchmark);
        }

        private void ReadTraces()
        {
            DirectoryInfo directory = new DirectoryInfo(@"..\..\..\traces\"); //get all traces inside traces folder in current project
            FileInfo[] files = directory.GetFiles("*.TRC");
            Parallel.ForEach(files, file =>
            {
                ReadTrace(file);
            });
        }

        private void Simulate()
        {
            string results = string.Empty;
            string instructions = string.Empty;
            foreach (var item in allTraceData) 
            {
                Tuple<double, double, int, int, int, int, int> temp = Simulate(item.Value);
                instructions += item.Key + Environment.NewLine;
                results += item.Key + Environment.NewLine;

                instructions += "Load: " + temp.Item3 + Environment.NewLine;
                instructions += "Store: " + temp.Item4 + Environment.NewLine;
                instructions += "Branch: " + temp.Item5 + Environment.NewLine;
                instructions += "Arytmetic: " + temp.Item6 + Environment.NewLine;
                instructions += "Total: " + temp.Item7 + Environment.NewLine;

                results += "Issue Rate: " + Convert.ToString(temp.Item1) + Environment.NewLine;
                results += "Ticks: " + Convert.ToString(temp.Item2) + Environment.NewLine;

                loadList.Add(temp.Item3.ToString());
                storeList.Add(temp.Item4.ToString());
                branchList.Add(temp.Item5.ToString());
                arythmeticList.Add(temp.Item6.ToString());
                totalList.Add(temp.Item7.ToString());

                issueRateList.Add(temp.Item1.ToString());
                ticksList.Add(temp.Item2.ToString());

                instructions += Environment.NewLine;
                results += Environment.NewLine;
            }
            textBoxRezultate.Text = results;
            textBoxInstructiuni.Text = instructions;
            textBoxConsole.Text = "Finnished!";
        }

        private Tuple<double, double, int, int, int, int, int> Simulate(List<Instruction> instructionsFromBenchmark) 
        {
            Instruction[,] instructionsFromMemory;

            int loadInstructions = 0;
            int storeInstructions = 0;
            int branchInstructions = 0;
            int arithmeticInstructions = 0;
            int totalInstructions = 0;

            int availableAccessToMemoryPerCycle = 0;
            int numberOfMemoryAccesses = 0;
            int missCachePenalty = 0;
            double cacheMiss = 0.1;

            int ticks = 0;
            double issueRate = 0;
            int PCnormal;

            availableAccessToMemoryPerCycle = NR_PORT;
            PCnormal = 0;
            instructionsFromMemory = new Instruction[100000000, IRmax];

            int row = 0;
            int col = 0;
            foreach (Instruction instruction in instructionsFromBenchmark)
            {
                while (instruction.currentPC != PCnormal)
                {
                    if (col == IRmax)
                    {
                        row++;
                        col = 0;
                    }

                    //adauga o instructiune alu in matricea instructiuniAduseDinMemorie
                    instructionsFromMemory[row, col++] = new Instruction
                    {
                        instructionType = InstructionType.Arithmetic
                    };

                    PCnormal++;
                    arithmeticInstructions++;
                }

                if (instruction.instructionType == InstructionType.Branch)
                {
                    if (col == IRmax)
                    {
                        row++;
                        col = 0;
                    }

                    //adauga o instructiune B in matricea instructiuniAduseDinMemorie
                    instructionsFromMemory[row, col++] = instruction;

                    PCnormal = (int)instruction.targetAddress;
                    branchInstructions++;
                }

                if (instruction.instructionType == InstructionType.Store)
                {
                    if (col == IRmax)
                    {
                        row++;
                        col = 0;
                    }

                    //adauga o instructiune S in matricea instructiuniAduseDinMemorie
                    instructionsFromMemory[row, col++] = instruction;

                    PCnormal++;
                    storeInstructions++;
                }

                if (instruction.instructionType == InstructionType.Load)
                {
                    if (col == IRmax)
                    {
                        row++;
                        col = 0;
                    }

                    //adauga o instructiune L in matricea instructiuniAduseDinMemorie
                    instructionsFromMemory[row, col++] = instruction;

                    PCnormal++;
                    loadInstructions++;
                }
            }

            var latency = latenta;
            var penalties = N_PEN;
            ticks = latency * row;

            missCachePenalty = Convert.ToInt32(loadInstructions * cacheMiss * missCachePenalty);
            ticks += missCachePenalty;

            foreach (Instruction instruction in instructionsFromMemory)
            {
                if (instruction != null)
                {
                    if (numberOfMemoryAccesses < availableAccessToMemoryPerCycle)
                    {
                        if (instruction.instructionType == InstructionType.Load || instruction.instructionType == InstructionType.Store)
                        {
                            numberOfMemoryAccesses++;
                        }
                    }
                    else
                    {
                        if (instruction.instructionType == InstructionType.Load || instruction.instructionType == InstructionType.Store)
                        {
                            numberOfMemoryAccesses = 0;
                            ticks += latency;
                        }
                    }
                }
            }

            totalInstructions = loadInstructions + storeInstructions + branchInstructions + arithmeticInstructions;
            issueRate = (Convert.ToDouble(totalInstructions) / Convert.ToDouble(ticks));
            issueRate = Math.Round(issueRate, 3);

            return new Tuple<double, double, int, int, int, int, int>(issueRate, ticks, loadInstructions, storeInstructions, branchInstructions, arithmeticInstructions, totalInstructions);
        }

        private void WriteResults()
        {
            using (StreamWriter sw = new StreamWriter(@"..\..\..\output\results.csv"))
            {
                string names = "Files,";
                string loads = "Loads,";
                string stores = "Stores,";
                string branches = "Branches,";
                string arythmetics = "Arythmetics,";
                string totals = "Totals,";
                string issueRates = "Issue rates,";
                string multiTicks = "Ticks,";

                foreach (var fileName in allTraceData) 
                {
                    names += fileName.Key + ",";
                }
                foreach (string s in loadList) 
                {
                    loads += s + ",";
                }
                foreach (string s in storeList) 
                {
                    stores += s + ",";
                }
                foreach (string s in branchList) 
                {
                    branches += s + ",";
                }
                foreach (string s in arythmeticList)
                {
                    arythmetics += s + ",";
                }
                foreach (string s in totalList) 
                {
                    totals += s + ",";
                }
                foreach (string s in issueRateList) 
                {
                    issueRates += s + ",";
                }
                foreach (string s in ticksList) 
                {
                    multiTicks += s + ",";
                }

                sw.WriteLine(names);
                sw.WriteLine(loads);
                sw.WriteLine(stores);
                sw.WriteLine(branches);
                sw.WriteLine(arythmetics);
                sw.WriteLine(totals);
                sw.WriteLine(issueRates);
                sw.WriteLine(multiTicks);
                sw.Close();
            }
        }
    }
}