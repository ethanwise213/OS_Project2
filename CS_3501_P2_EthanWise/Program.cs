using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUSchedulingSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CPU Scheduling Simulation");

            // Example processes
            List<Process> processes = new List<Process>
            {
                new Process { Id = 1, ArrivalTime = 0, BurstTime = 8 },
                new Process { Id = 2, ArrivalTime = 1, BurstTime = 4 },
                new Process { Id = 3, ArrivalTime = 2, BurstTime = 9 },
                new Process { Id = 4, ArrivalTime = 3, BurstTime = 5 }
            };

            // Automatically run all tests without requiring user input
            processes.ForEach(p => p.RemainingTime = p.BurstTime);
            Console.WriteLine("\nShortest Remaining Time First (SRTF):");
            SchedulingAlgorithms.SRTF(processes);

            // Ensure RemainingTime is initialized for all processes before HRRN execution
            processes.ForEach(p => p.RemainingTime = p.BurstTime);
            Console.WriteLine("\nHighest Response Ratio Next (HRRN):");
            SchedulingAlgorithms.HRRN(processes);
        }
    }

    class Process
    {
        public int Id { get; set; }
        public int ArrivalTime { get; set; }
        public int BurstTime { get; set; }
        public int RemainingTime { get; set; }
        public int CompletionTime { get; set; }
        public int TurnaroundTime { get; set; }
        public int WaitingTime { get; set; }
        public int ResponseTime { get; set; }
        public bool Started { get; set; } = false;
    }

    static class SchedulingAlgorithms
    {
        public static void SRTF(List<Process> processes)
        {
            int time = 0;
            int completed = 0;
            Process? currentProcess = null;

            while (completed < processes.Count)
            {
                currentProcess = processes
                    .Where(p => p.ArrivalTime <= time && p.RemainingTime > 0)
                    .OrderBy(p => p.RemainingTime)
                    .FirstOrDefault();

                if (currentProcess != null)
                {
                    if (!currentProcess.Started)
                    {
                        currentProcess.ResponseTime = time - currentProcess.ArrivalTime;
                        currentProcess.Started = true;
                    }

                    currentProcess.RemainingTime--;
                    if (currentProcess.RemainingTime == 0)
                    {
                        currentProcess.CompletionTime = time + 1;
                        currentProcess.TurnaroundTime = currentProcess.CompletionTime - currentProcess.ArrivalTime;
                        currentProcess.WaitingTime = currentProcess.TurnaroundTime - currentProcess.BurstTime;
                        completed++;
                    }
                }
                else
                {
                    if (processes.All(p => p.RemainingTime == 0 || p.ArrivalTime > time))
                    {
                        break;
                    }
                }
                time++;
            }

            PrintResults(processes);
        }

        public static void HRRN(List<Process> processes)
        {
            int time = 0;
            int completed = 0;

            while (completed < processes.Count)
            {
                var readyProcesses = processes
                    .Where(p => p.ArrivalTime <= time && p.RemainingTime > 0)
                    .Select(p => new
                    {
                        Process = p,
                        ResponseRatio = (time - p.ArrivalTime + p.BurstTime) / (float)p.BurstTime
                    })
                    .OrderByDescending(p => p.ResponseRatio)
                    .ToList();

                if (readyProcesses.Any())
                {
                    var selectedProcess = readyProcesses.First().Process;
                    if (!selectedProcess.Started)
                    {
                        selectedProcess.ResponseTime = time - selectedProcess.ArrivalTime;
                        selectedProcess.Started = true;
                    }

                    time += selectedProcess.BurstTime;
                    selectedProcess.CompletionTime = time;
                    selectedProcess.TurnaroundTime = selectedProcess.CompletionTime - selectedProcess.ArrivalTime;
                    selectedProcess.WaitingTime = selectedProcess.TurnaroundTime - selectedProcess.BurstTime;
                    selectedProcess.RemainingTime = 0;
                    completed++;
                }
                else
                {
                    if (processes.All(p => p.RemainingTime == 0 || p.ArrivalTime > time))
                    {
                        break;
                    }
                    time++;
                }
            }

            PrintResults(processes);
        }

        private static void PrintResults(List<Process> processes)
        {
            Console.WriteLine("\nProcess Arrival Burst   Completion Turnaround Waiting Response");
            foreach (var process in processes)
            {
                Console.WriteLine($"{process.Id,-8}{process.ArrivalTime,-8}{process.BurstTime,-8}{process.CompletionTime,-11}{process.TurnaroundTime,-11}{process.WaitingTime,-8}{process.ResponseTime,-8}");
            }

            double averageWaitingTime = processes.Average(p => p.WaitingTime);
            double averageTurnaroundTime = processes.Average(p => p.TurnaroundTime);
            double cpuUtilization = (double)processes.Sum(p => p.BurstTime) / processes.Max(p => p.CompletionTime) * 100;
            double throughput = (double)processes.Count / processes.Max(p => p.CompletionTime);

            Console.WriteLine("\nMetrics:");
            Console.WriteLine($"Average Waiting Time (AWT): {averageWaitingTime:F2}");
            Console.WriteLine($"Average Turnaround Time (ATT): {averageTurnaroundTime:F2}");
            Console.WriteLine($"CPU Utilization: {cpuUtilization:F2}%");
            Console.WriteLine($"Throughput: {throughput:F2} processes/unit time");
        }
    }

}


