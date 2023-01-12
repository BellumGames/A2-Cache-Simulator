using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2
{
    internal class InstructionOld
    {
        public char instructionType;
        public uint currentPC;
        public uint target;

        public InstructionOld(char instructionType, uint currentPC, uint target)
        {
            this.instructionType = instructionType;
            this.currentPC = currentPC;
            this.target = target;
        }


        //Function that changes the Uniport/Biport
        static int GetPortState(int cacheType)
        {
            //Check if the option uniport is selected
            if (cacheType == Constants.UNIFIED)
            {
                return 1;
            }
            //Else select the biport
            else
            {
                return 2;
            }
        }
        //13 februarie restanta FLorea
        //Function that take the banchmark instruction and procesed
        public static Tuple<double, double, int, int, int, int> Simulation(List<Tuple<char, uint, uint>> Instructions, int IRMax, uint normalPC, int latenta, int NR_PORT, int N_PEN, int FR_IC, int FR, int SIZE_DC, int SIZE_IC, int IBS)
        {
            List<Instruction> instructionsFromBanchmark = new List<Instruction>();
            Instruction[,] instructionsFromMemory = new Instruction[100000000, IRMax];
            int numberOfAritmetical = 0;
            int numberOfBranches = 0;
            int numberOfStores = 0;
            int numberOfLoads = 0;
            int ticks = 0;

            int row = 0;
            int col = 0;

            foreach(var item in Instructions)
            {
                instructionsFromBanchmark.Add(new Instruction(item.Item1, item.Item2, item.Item3));
            }

            foreach (Instruction instruction in instructionsFromBanchmark)
            {
                while (instruction.currentPC != normalPC)
                {
                    if (col == IRMax)
                    {
                        row++;
                        col = 0;
                    }
                    instructionsFromMemory[row, col++] = instruction;

                    normalPC++;
                    numberOfAritmetical++;
                }
                if (instruction.instructionType == Constants.BRANCH)
                {
                    if (col == IRMax)
                    {
                        row++;
                        col = 0;
                    }
                    instructionsFromMemory[row, col++] = instruction;
                    normalPC = instruction.target;
                    numberOfBranches++;
                }
                if (instruction.instructionType == Constants.STORE)
                {
                    if (col == IRMax)
                    {
                        row++;
                        col = 0;
                    }
                    instructionsFromMemory[row, col++] = instruction;
                    normalPC++;
                    numberOfStores++;
                }
                if (instruction.instructionType == Constants.LOAD)
                {
                    if (col == IRMax)
                    {
                        row++;
                        col = 0;
                    }
                    instructionsFromMemory[row, col++] = instruction;
                    normalPC++;
                    numberOfLoads++;
                }
                
            }

            ticks = latenta * row;
            int memoryAccess = 0;

            foreach (Instruction instruction in instructionsFromMemory)
            {
                if (memoryAccess < GetPortState(NR_PORT))
                {
                    if (instruction.instructionType == Constants.STORE || instruction.instructionType == Constants.LOAD)
                    {
                        memoryAccess++;
                    }
                }
                else
                {
                    if (instruction.instructionType == Constants.STORE || instruction.instructionType == Constants.LOAD)
                    {
                        memoryAccess = 0;
                        ticks += latenta;
                    }
                }
            }
            double issueRate =  (numberOfAritmetical + numberOfBranches + numberOfLoads + numberOfStores) / ticks;
            double missCachePenalty = numberOfLoads * Constants.CACHE_MISS * N_PEN;
            ticks += N_PEN;
            return new Tuple<double, double, int, int, int, int>(issueRate, missCachePenalty, ticks, numberOfBranches, numberOfLoads, numberOfStores);
        }
    }
}