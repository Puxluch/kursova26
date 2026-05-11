using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Логіка програми 
namespace CourseWork
{

    public class Result // Клас-збереження рез
    {
        public List<int> Path { get; set; } // шлях для графу
        public double Weight { get; set; } // вага всього маршруту
        public long Time { get; set; } // час за який алгоритм виконав роботу в мс
        public int Iterations { get; set; } // кількість ітерацій виконана протягом роботи
        public string AlgName { get; set; } // назва використаного алгоритму
    }

    public class Logic // клас логіки для роботи з програмою
    {

        // Константи для обмеження розміру матриці та ваги ребер для генерації та валідації
        private const int MinVertex = 3; // мін. кількість ребер для
        private const int MaxManualVertex = 20; /// мак. кількість ребер для ручного введення
        private const int MaxGeneratedVertex = 1000; // мак. кількість ребер для генерації
        private const double MinWeight = 1; // мін. вага ребра
        private const double MaxWeight = 10000; // максимальна вага ребра

        private struct Edge //  структура для збереження ребер в жадібному алгоритмі
        {
            public int U;
            public int V;
            public double Weight;
        }

        public double[,] GenerateRandomMatrix(int size) // генерація випадкової симетричної матриці
        {
            if (size < MinVertex || size > MaxGeneratedVertex) // валідація на кількість вершин
            {
                throw new ArgumentException("Некоректний розмір матриці");
            }
            Random random = new Random(); // генератор вип. чисел

            double[,] matrix = new double[size, size]; // ініціалізація матриці

            for (int i = 0; i < size; i++) // заповнення матриці числами
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                    {
                        matrix[i, j] = 0;
                    }
                    else if (j > i)
                    {
                        double value = random.Next(1, 1000); // генерація випадкової ваги для ребра між містами i та j
                        matrix[i, j] = value;
                        matrix[j, i] = value;
                    }
                }
            }
            return matrix;
        }

        public Result Solve(double[,] matrix, string alg) // клас для розв'язання задачі з вибором алгоритму
        {
            ValidateMatrix(matrix); // валідація матриці

            Stopwatch stopwatch = new Stopwatch(); // таймер для вимірювання часу
            stopwatch.Start();

            Result result; // змінна для збереження результату

            switch (alg) // вибір алгоритму
            {
                case "Жадібний метод":
                    result = GreedyAlg(matrix);
                    break;
                case "Метод найближчого сусіда":
                    result = NearestNeighborAlg(matrix);
                    break;
                case "Емуляція відпалу":
                    result = SimulatedAnnealing(matrix);
                    break;
                default:
                    throw new ArgumentException("Невідомий алгоритм."); // якщо якимось чином було вибрано не існуючий алгоритм, то вивести помилку
            }

            stopwatch.Stop(); // зупинка таймера

            result.Time = stopwatch.ElapsedMilliseconds; // збереження часу
            result.Weight = CalculateWeight(matrix, result.Path); // збереження ваги

            return result;
        }

        private Result GreedyAlg(double[,] matrix) // реалізація першого алгоритму - жадібного
        {
            int size = matrix.GetLength(0); // отримання розміру
            List<Edge> edges = new List<Edge>(); // список для збереження ребер

            int iter = 0;

            for (int i = 0; i < size; i++) // заповнення списку ребер
            {
                for (int j = i + 1; j < size; j++)
                {
                    edges.Add(new Edge { U = i, V = j, Weight = matrix[i, j] }); // створення об'єкта та додавання до списку
                }
            }

            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight)); // сортування ребер за вагою в порядку зростання

            // Ініціалізація масивів для збереження ступеня кожної вершини та батьків для пошуку циклів
            int[] degree = new int[size]; 
            int[] parent = new int[size];

            for (int i = 0; i < size; i++) // ініціалізація батьків для кожної вершини
            {
                parent[i] = i;
            }

            int Find(int i) // функція для пошуку кореня множини, до якої належить вершина i
            {
                if (parent[i] == i)
                {
                    return i;
                }
                return parent[i] = Find(parent[i]); // рекурсивний пошук кореня та компресія шляху для оптимізації
            }

            List<Edge> selectedEdges = new List<Edge>(); // список для збереження вибраних ребер
            int edgesAdded = 0; // лічильник доданих ребер

            foreach (var edge in edges) // проходження по відсортованих ребрах
            {
                iter++;
                if (degree[edge.U] < 2 && degree[edge.V] < 2) // перевірка, чи можна додати ребро
                {
                    // пошук коренів
                    int rootU = Find(edge.U);
                    int rootV = Find(edge.V);

                    if (rootU != rootV || edgesAdded == size - 1) // перевірка, чи не утворить додавання ребра цикл (якщо корені різні) або якщо це останнє ребро для замикання циклу
                    {
                        selectedEdges.Add(edge); // додавання ребра до вибраних
                        degree[edge.U]++;
                        degree[edge.V]++;
                        parent[rootU] = rootV; // об'єднання множин, до яких належать вершини U та V
                        edgesAdded++;

                        if (edgesAdded == size) // якщо додано достатньо ребер для утворення маршруту, то можна зупинитися
                        {
                            break;
                        }
                    }
                }
            }

            if (edgesAdded < size) // якщо чомусь не вийшло додати достатньо ребер для утворення маршруту то вивести помилку
            {
                throw new Exception("Не вдалося побудувати маршрут");
            }

            List<int>[] adj = new List<int>[size]; // створення списку суміжності для побудови маршруту з вибраних ребер

            for (int i = 0; i < size; i++) // ініціалізація списку суміжності
            {
                adj[i] = new List<int>();
            }

            foreach (var e in selectedEdges) // заповнення списку суміжності вибраними ребрами
            {
                adj[e.U].Add(e.V);
                adj[e.V].Add(e.U);
            }
            
            List<int> path = new List<int>(); // список для збереження побудованого маршруту
            int current = 0; // вершина 0
            int prev = -1;  // змінна для збереження попередньої вершини

            for (int i = 0; i < size; i++) // побудова маршруту, проходячи по вибраних ребрах
            {
                path.Add(current); // додавання поточної вершини
                int next = adj[current][0]; // вибір наступної вершини
                if (next == prev) // перевірка чи не є наступна вершина попередньою, в разі цього вибираємо іншу
                {
                    next = adj[current][1];
                }


                prev = current; // оновлення попередньої вершини
                current = next; // перехід до наступної вершини
            }
            path.Add(0); // замикання маршруту

            return new Result // повернення результату
            {
                Path = path,
                Iterations = iter,
                AlgName = "Жадібний метод"
            };
        }

        private Result NearestNeighborAlg(double[,] matrix) // реалізація другого алгоритму -  найближчого сусіда
        {
            int size = matrix.GetLength(0); // отримання розміру
            bool[] visited = new bool[size]; // відстеження відвіданих вершин
            List<int> path = new List<int>(); // список для збереження побудованого маршруту

            int current = 0; // вершина 0 
            visited[current] = true; // вершина 0 відвідана
            path.Add(current); // додавання вершини 0 до маршруту

            int iter = 0; 

            for (int step = 0; step < size - 1; step++) // побудова маршруту, вибираючи на кожному кроці найближчого сусіда
            {
                double bestWeight = double.MaxValue;
                int bestVertex = -1;

                for (int vertex = 0; vertex < size; vertex++) // пошук найближчого сусіда серед невідвіданих вершин
                {
                    iter++; 
                    
                    if (!visited[vertex] && matrix[current, vertex] < bestWeight) // якщо вершина не відвідана і вага до неї менша за найкращу знайдену, то оновити найкращу вагу та вершину
                    {
                        bestWeight = matrix[current, vertex];
                        bestVertex = vertex;
                    }

                }

                if (bestVertex == -1) // якщо чомусь не вдалося знайти наступну вершину то буде помилка
                {
                    throw new Exception("Помилка побудови маршруту.");
                }

                visited[bestVertex] = true; // відмітка вибраної вершини як відвіданої
                path.Add(bestVertex); // додавання вибраної вершини до маршруту
                current = bestVertex; // перехід до вибраної вершини
            }
            path.Add(0); // замикання маршруту, повернення до початкової вершини

            return new Result 
            {
                Path = path,
                Iterations = iter,
                AlgName = "Метод найближчого сусіда"
            };
        }

        private Result SimulatedAnnealing(double[,] matrix) // реалізація третього алгоритму - емуляції відпалу
        {
            int size = matrix.GetLength(0); // отримання розміру
            Random random = new Random(); // генератор випадкових чисел для створення початкового маршруту

            List<int> currentRoute = Enumerable.Range(0, size).ToList(); // створення початкового маршруту
            for (int i = 1; i < size; i++) // перетасовка маршруту для створення випадкового початкового рішення
            {
                int r = random.Next(i, size); // вибір випадкового індексу для обміну
                (currentRoute[i], currentRoute[r]) = (currentRoute[r], currentRoute[i]); // обмін елементів для перетасовки маршруту
            }
            currentRoute.Add(0); // замикання маршруту, повернення до початкової вершини

            double currentWeight = CalculateWeight(matrix, currentRoute); // обчислення ваги початкового маршруту

            // збереження найкращого знайденого маршруту та його ваги
            List<int> bestRoute = new List<int>(currentRoute);
            double bestWeight = currentWeight;

            // Параметри для алгоритму відпалу
            double temp = 10000.0; // початкова температура
            double alpha = 0.99; // коефіцієнт охолодження
            double tMin = 0.0001; // мінімальна температура для зупинки алгоритму
            int iter = 0; // лічильник ітерацій
            int attemptsPerTemp = size * 5;
            while (temp > tMin) //поки температура не опуститься до мінімального рівня, продовжувати пошук кращого рішення
            {
                for (int k = 0; k < attemptsPerTemp; k++)
                {
                    iter++; // + ітерація
                    List<int> candidateRoute = new List<int>(currentRoute); // створення кандидата на основі поточного маршруту

                    // вибір двох випадкових індексів для обміну в маршруті
                    int i = random.Next(1, size);
                    int j = random.Next(1, size);

                    if (i == j) // пропустити ітерацію у разі вибору однакових індексів
                    {
                        continue;
                    }

                    (candidateRoute[i], candidateRoute[j]) = (candidateRoute[j], candidateRoute[i]); // обмін елементів для створення нового кандидата

                    double candidateWeight = CalculateWeight(matrix, candidateRoute); // обчислення ваги кандидата
                    double delta = candidateWeight - currentWeight; // різниця між вагою кандидата та поточного маршруту

                    if (delta < 0 || random.NextDouble() < Math.Exp(-delta / temp)) //  якщо виконуються складні умови, то оновити
                    {
                        currentRoute = candidateRoute;
                        currentWeight = candidateWeight;

                        if (currentWeight < bestWeight) // оновити найкращий маршрут та вагу в разі покращення результату
                        {
                            bestWeight = currentWeight;
                            bestRoute = new List<int>(currentRoute);
                        }
                    }
                }

                    temp *= alpha; // охолодження температури
            }

            return new Result 
            {
                Path = bestRoute,
                Weight = bestWeight,
                Iterations = iter,
                AlgName = "Емуляція відпалу"
            };
        }

        private double CalculateWeight(double[,] matrix, List<int> path) // метод для обчислення ваги маршруту
        {
            double weight = 0; // стартова вага = 0
            for (int i = 0; i < path.Count - 1; i++) // проходження по маршруту та додавання ваги кожного ребра до загальної ваги
            {
                weight += matrix[path[i], path[i + 1]];
            }
            return weight;
        }

        public void ValidateMatrix(double[,] matrix) // метод для валідації матриці
        {
            if (matrix == null) // якщо матриця не існує то вивести помилку
            {
                throw new ArgumentNullException("Матриця не існує");
            }

            // отримання кількості рядків та стовпців матриці
            int rows = matrix.GetLength(0); 
            int cols = matrix.GetLength(1);

            if (rows != cols) // матриця має бути квадратна, інашке - помилка
            {
                throw new ArgumentException("Матриця повинна бути квадратною");
            }
            if (rows < MinVertex || rows > MaxGeneratedVertex) // кількість вершин має бути в межах допустимого, інакше - помилка
            {
                throw new ArgumentException("Некоректна кількість вершин");
            }

            for (int i = 0; i < rows; i++) // перебір матриці
            {
                for (int j = 0; j < cols; j++)
                {
                    if (double.IsNaN(matrix[i, j]) || double.IsInfinity(matrix[i, j])) // якщо некоректні значенння - помилка
                    {
                        throw new ArgumentException("Матриця містить некоректні значення");
                    }

                    if (i == j && matrix[i, j] != 0) // на головній діагоналі мають бути лише нулі, інакше - помилка
                    {
                        throw new ArgumentException("На головній діагоналі повинні бути лише нулі.");
                    }
                    if (i != j && matrix[i, j] < MinWeight) // вага має бути більшою за нуль, інакше - помилка
                    {
                        throw new ArgumentException("Відстань між вершинами повинна бути більшою за нуль.");
                    }
                    if (matrix[i, j] > MaxWeight) // вага не повинна бути не більше за верхній діапазон, інакше - помилка
                    {
                        throw new ArgumentException("Занадто велике значення ваги.");
                    }
                    if (Math.Abs(matrix[i, j] - matrix[j, i]) > 0.0001) // використання допуску для дробових чисел
                    {
                        throw new ArgumentException($"Матриця повинна бути симетричною");
                    }
                }
            }
        }

        public void SaveResult(string path, string text) // збереження результату у файл
        {
            if (string.IsNullOrWhiteSpace(path)) // якщо шлях до файлу не вказано, то вивести помилку
            {
                throw new ArgumentException("Некоректний шлях до файлу.");
            }
            if (string.IsNullOrWhiteSpace(text)) // якщо текст для збереження пустий, то вивести помилку
            {
                throw new ArgumentException("Немає даних для збереження.");
            }
            
            File.WriteAllText(path, text); // запис тексту у файл за вказаним шляхом
        }
    }
}
