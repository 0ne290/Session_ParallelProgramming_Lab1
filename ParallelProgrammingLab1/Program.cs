using System.Diagnostics;
using Newtonsoft.Json;
using Semaphore = ParallelProgrammingLab1.PetriNet.Semaphore;

namespace ParallelProgrammingLab1;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var outputFile = new StreamWriter("../../../Output.txt", false);
            
            var threadScheduler = ParseInputData("../../../Input.json", outputFile);
            
            outputFile.WriteLine("\nАлфавит потоков:");
            MyThread.PrintAlphabet(outputFile);
            
            outputFile.WriteLine("\nПрефиксная форма потоков:");
            MyThread.PrintPrefixes(outputFile, threadScheduler.Preemptive);
            
            MyThread.PrintParallelComposition(outputFile);

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            threadScheduler.Execute();
            outputFile.WriteLine($"\nОбщее время работы системы: {stopwatch.ElapsedMilliseconds} мс.");
            
            outputFile.WriteLine("\nПротоколы потоков:");
            MyThread.PrintProtocols(outputFile);

            threadScheduler.Dispose();
            
            Console.Write("Нажмите любую клавишу для завершения программы...");
            Console.ReadKey();

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.Write("\nНажмите любую клавишу для завершения программы...");
            Console.ReadKey();
            return 1;
        }
    }

    private static ThreadScheduler ParseInputData(string pathToInputFile, StreamWriter outputFile)
    {
        var inputData = JsonConvert.DeserializeObject<InputData>(File.ReadAllText(pathToInputFile));
            var random = new Random();

            if (inputData == null)
                throw new Exception(
                    "Невозможно прочитать данные из файла конфигурации. Вероятно, данные не соответствуют формату.");

            if (inputData.Np < inputData.Threads.Count)
                inputData.Np = inputData.Threads.Count;
            if (inputData.Nr < inputData.Resources.Count)
                inputData.Nr = inputData.Resources.Count;
            if (inputData.Qt < 1)
                inputData.Qt = random.Next(1, 1001);
            if (inputData.MaxT < 1)
                inputData.MaxT = random.Next(1, 11);
            if (inputData.MaxP < 1)
                inputData.MaxP = random.Next(1, inputData.Np + 1);

            var pa = inputData.Pa ? "SJF, preemptive, absolute priority" : "SJF, nonpreemptive";
            outputFile.WriteLine($"Pa: {pa}");
            outputFile.WriteLine($"Qt: {inputData.Qt}");
            outputFile.WriteLine($"MaxT: {inputData.MaxT}");
            outputFile.WriteLine($"MaxP: {inputData.MaxP}");
            outputFile.WriteLine($"Nr: {inputData.Nr}");
            outputFile.WriteLine($"Np: {inputData.Np}");

            outputFile.WriteLine("\nРесурсы:");
            foreach (var serializedResource in inputData.Resources)
            {
                if (serializedResource.Capacity < 1)
                    serializedResource.Capacity = random.Next(1, inputData.Np / 2 + 1);
                    
                var resource = new Semaphore(serializedResource.Name, serializedResource.Capacity);
                outputFile.WriteLine($"\tНазвание: {resource.Name}; пропускная способность: {serializedResource.Capacity}");
            }
            
            for (var i = 0; i < inputData.Nr - inputData.Resources.Count; i++)
            {
                var capacity = random.Next(1, inputData.Np / 2 + 1);
                var resource = new Semaphore("", capacity);
                outputFile.WriteLine($"\tНазвание: {resource.Name}; пропускная способность: {capacity}");
            }

            outputFile.WriteLine("\nПотоки:");
            foreach (var serializedThread in inputData.Threads)
            {
                var resources = new List<Semaphore>();
                if (serializedThread.ResourceNames.Count > 0)
                    foreach (var resName in serializedThread.ResourceNames)
                        resources.Add(string.IsNullOrWhiteSpace(resName)
                            ? Semaphore.Semaphores[random.Next(0, Semaphore.Semaphores.Count)]
                            : Semaphore.GetByName(resName));
                else
                    for (var j = 0; j < random.Next(0, Semaphore.Semaphores.Count) + 1; j++)
                        resources.Add(Semaphore.Semaphores[random.Next(0, Semaphore.Semaphores.Count)]);

                if (serializedThread.Priority < 1)
                    serializedThread.Priority = random.Next(1, inputData.MaxP + 1);
                if (serializedThread.CpuBurst < 1)
                    serializedThread.CpuBurst = random.Next(1, inputData.MaxT + 1);
                if (serializedThread.Quantity < 1)
                    serializedThread.Quantity = random.Next(1, 6);

                var thread = new MyThread(serializedThread.Name, serializedThread.Priority, serializedThread.CpuBurst, serializedThread.Quantity, resources);
                outputFile.WriteLine(
                    $"\tНазвание: {thread.Name}; приоритет: {thread.Priority}; время работы в квантах: {thread.CpuBurst}; сколько раз выполнить: {serializedThread.Quantity}; названия требуемых ресурсов: {string.Join(", ", thread.GetSemaphoreNames())}");
            }

            for (var i = 0; i < inputData.Np - inputData.Threads.Count; i++)
            {
                var resources = new List<Semaphore>();
                for (var j = 0; j < random.Next(0, Semaphore.Semaphores.Count) + 1; j++)
                    resources.Add(Semaphore.Semaphores[random.Next(0, Semaphore.Semaphores.Count)]);

                var quantity = random.Next(1, 6);

                var thread = new MyThread("", random.Next(1, inputData.MaxP + 1), random.Next(1, inputData.MaxT + 1), quantity, resources);
                outputFile.WriteLine(
                    $"\tНазвание: {thread.Name}; приоритет: {thread.Priority}; время работы в квантах: {thread.CpuBurst}; сколько раз выполнить: {quantity}; названия требуемых ресурсов: {string.Join(", ", thread.GetSemaphoreNames())}");
            }

            return new ThreadScheduler(inputData.Qt, inputData.Pa, outputFile);
    }
}
