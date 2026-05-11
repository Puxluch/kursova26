using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;


// Головна форма програми 

namespace CourseWork
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
    public partial class MainForm : Form // створення головної форми програми
    {
        public readonly Logic _logic; // логіка програми

        private double[,] generatedMatrix; // збереження згенерованої матриці

        // Візуальні частини форми
        private NumericUpDown numericCities;
        private DataGridView matrixGrid;
        private ComboBox algorithmBox;
        private Button solveButton;
        private Button generateButton;
        private Button saveButton;
        private TextBox resultBox;
        private PictureBox graphBox;
        private Label infoLabel;

        public MainForm() // ініціалізація форми та логіки
        {
            InitializeComponent();
            _logic = new Logic();
        }

        private void InitializeComponent() // налаштування частин форми
        {
            Text = "Курсова робота. Розв'язання задачі Комівояжера";
            Width = 1200;
            Height = 750;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Label cityLabel = new Label // підпис для вибору кількості
            { 
                Text = "Кількість вершин:", Left = 20, Top = 20, Width = 117 
            }; 

            numericCities = new NumericUpDown // вибір кількості вершин
            {
                Left = 139,
                Top = 15,
                Width = 80,
                Minimum = 3,
                Maximum = 1000,
                Value = 4
            };
            numericCities.ValueChanged += NumericCities_ValueChanged; // обробка зміни кількості

            Label algorithmLabel = new Label // підпис для вибору алгоритму
            { 
                Text = "Алгоритм:", Left = 240, Top = 20, Width = 65 
            };

            algorithmBox = new ComboBox // вибір алгоритму
            {
                Left = 310,
                Top = 15,
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            algorithmBox.Items.AddRange(new[]
            { 
                "Жадібний метод", "Метод найближчого сусіда", "Емуляція відпалу" 
            }); // додавання алгоритмів до списку
            algorithmBox.SelectedIndex = 0; // перший алг за замовчуванням

            generateButton = new Button // кнопка для обнулення
            { 
                Text = "Обнулити матрицю", 
                Left = 580, Top = 12, 
                Width = 150, 
                Height = 35 
            };
            generateButton.Click += GenerateButton_Click; // обробка натискання 

            Button randomButton = new Button  // кнопка для генерації випадкового графа
            { 
                Text = "Випадковий граф", 
                Left = 740, 
                Top = 12, 
                Width = 130, 
                Height = 35
            };
            randomButton.Click += RandomButton_Click; // обробка натискання кнопки

            solveButton = new Button  // кнопка для розв'язання задачі
            { 
                Text = "Розв'язати", 
                Left = 880, 
                Top = 12, 
                Width = 130, 
                Height = 35 
            };
            solveButton.Click += SolveButton_Click; // обробка натискання кнопки

            saveButton = new Button // кнопка для збереження результату
            { 
                Text = "Зберегти результат", 
                Left = 1020, 
                Top = 12, 
                Width = 130, 
                Height = 35 
            };
            saveButton.Click += SaveButton_Click; // обробка натискання кнопки

            matrixGrid = new DataGridView // таблиця для введення матриці відстаней а також обмеження на її редагування
            {
                Left = 20,
                Top = 70,
                Width = 650,
                Height = 550,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // колонки заповнюють всю ширину
                AllowUserToOrderColumns = false,
                AllowUserToResizeColumns = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect // вибір окремих клітинок
            };

            resultBox = new TextBox // текстове поле для виведення результатів
            {
                Left = 700,
                Top = 70,
                Width = 450,
                Height = 180,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 11)
            };

            graphBox = new PictureBox // область для візуалізації графа та маршруту
            {
                Left = 700,
                Top = 280,
                Width = 450,
                Height = 340,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            infoLabel = new Label // інформаційна панель для повідомлень
            {
                Left = 20,
                Top = 640,
                Width = 1100,
                Height = 30,
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Додавання всіх елементів на форму

            Controls.Add(cityLabel);
            Controls.Add(numericCities);
            Controls.Add(algorithmLabel);
            Controls.Add(algorithmBox);
            Controls.Add(randomButton);
            Controls.Add(generateButton);
            Controls.Add(solveButton);
            Controls.Add(saveButton);
            Controls.Add(matrixGrid);
            Controls.Add(resultBox);
            Controls.Add(graphBox);
            Controls.Add(infoLabel);

            CreateMatrix((int)numericCities.Value); // початкова ініціалізація матриці
        }

        private void RandomButton_Click(object sender, EventArgs e) // обробка натискання кнопки для генерації випадкового графа
        {
            try // обробка можливих помилок при генерації
            {
                int size = (int)numericCities.Value; // отримання розміру

                generatedMatrix = _logic.GenerateRandomMatrix(size); // генерація випадкової матриці

                if (size <= 20) // якщо розмір в межах то показуємо матримцю в таблиці
                {
                    UpdateGridFromMatrix(generatedMatrix);
                    matrixGrid.Visible = true;
                }
                else // інакше - приховати
                {
                    matrixGrid.Visible = false;
                }

                infoLabel.Text = "Випадковий граф успішно створено";
            }
            catch (Exception ex) // показ помилки у разі невдачі
            {
                ShowError(ex.Message);
            }
        }

        private void NumericCities_ValueChanged(object sender, EventArgs e) // обробка зміни кількості вершин
        {
            int size = (int)numericCities.Value;

            if (size <= 20) // якщо розмір в межах то показуємо матримцю в таблиці
            {
                matrixGrid.Visible = true;
                CreateMatrix(size);
            }
            else // інакше - приховати та очистити граф
            {
                matrixGrid.Visible = false;
                graphBox.Image = null;

                infoLabel.Text = "Для більше ніж 20 вершин доступна лише випадкова генерація";
            }
        }

        private void GenerateButton_Click(object sender, EventArgs e) // обробка натискання кнопки для обнулення матриці
        {
            int size = (int)numericCities.Value;

            if (size > 20) // якщо розмір більше 20, то приховуємо таблицю та очищуємо граф
            {
                generatedMatrix = null;
                matrixGrid.Visible = false;
                graphBox.Image = null;

                infoLabel.Text = "Для більше ніж 20 вершин ручне введення недоступне";
                return;
            }

            matrixGrid.Visible = true;
            CreateMatrix(size); // створення нової матриці з нуля
        }

        private void SolveButton_Click(object sender, EventArgs e) // обробка натискання кнопки для розв'язання задачі
        {
            try // обробка можливих помилок при розв'язанні
            {
                double[,] matrix;
                
                if ((int)numericCities.Value <= 20) // якщо розмір в межах то читаємо матрицю з таблиці
                {
                    matrix = ReadMatrixFromGrid();
                }
                else // інакше використовуємо згенеровану матрицю
                {
                    if (generatedMatrix == null)
                        throw new Exception("Спочатку згенеруйте випадкову матрицю");

                    matrix = generatedMatrix;
                }
                string algorithm = algorithmBox.SelectedItem.ToString(); // отримання вибраного алгоритму

                Result result = _logic.Solve(matrix, algorithm); // розв'язання задачі


                // Виведення результатів у текстове поле
                resultBox.Text =
                    $"Алгоритм: {result.AlgName}{Environment.NewLine}" +
                    $"Маршрут: {string.Join(" > ", result.Path.Select(x => x + 1))}{Environment.NewLine}" +
                    $"Загальна довжина: {result.Weight:F2}{Environment.NewLine}" +
                    $"Час виконання: {result.Time} мс{Environment.NewLine}" +
                    $"Кількість ітерацій: {result.Iterations}";

                if (matrix.GetLength(0) <= 20) // якщо розмір в межах то показуємо граф     
                {
                    graphBox.Image = DrawGraph(matrix.GetLength(0), result.Path, graphBox.Width, graphBox.Height);
                }
                else
                {
                    graphBox.Image = null;
                }
                infoLabel.Text = "Розрахунок успішно завершено";
            }
            catch (Exception ex) // показ помилки у разі невдачі
            {
                ShowError(ex.Message);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e) // обробка натискання кнопки для збереження результату
        {
            try // обробка можливих помилок при збереженні
            {
                if (string.IsNullOrWhiteSpace(resultBox.Text)) // перевірка наявності результатів для збереження
                {
                    throw new Exception("Немає результатів для збереження");
                }

                using (SaveFileDialog dialog = new SaveFileDialog()) // діалог для вибору місця збереження файлу
                {
                    dialog.Filter = "Text files (*.txt)|*.txt";
                    dialog.FileName = "result.txt";

                    if (dialog.ShowDialog() == DialogResult.OK) //
                    {
                        _logic.SaveResult(dialog.FileName, resultBox.Text);
                        MessageBox.Show("Результат успішно збережено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex) // показ помилки у разі невдачі
            {
                ShowError(ex.Message);
            }
        }
        private void CreateMatrix(int size) // створення матриці заданого розміру
        {
            // Очищення існуючої матриці в таблиці
            matrixGrid.DataSource = null;
            matrixGrid.Columns.Clear();
            matrixGrid.Rows.Clear();
            matrixGrid.Refresh();

            for (int i = 0; i < size; i++) // додавання колонок з підписами
            {
                matrixGrid.Columns.Add($"C{i}", $"М{i + 1}");
                matrixGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            matrixGrid.Rows.Add(size); // додавання рядків з підписами та заповнення значень

            for (int i = 0; i < size; i++) // заповнення матриці: 0 на діагоналі та 1 поза нею
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                    {
                        // Встановлення 0 на діагоналі та блокування редагування цих клітинок
                        matrixGrid.Rows[i].Cells[j].Value = 0;
                        matrixGrid.Rows[i].Cells[j].ReadOnly = true;
                        matrixGrid.Rows[i].Cells[j].Style.BackColor = Color.LightGray;
                    }
                    else // заповнення поза діагоналлю одиницями
                    {
                        matrixGrid.Rows[i].Cells[j].Value = 1; 
                    }
                }
            }
        }

        private void UpdateGridFromMatrix(double[,] matrix) // оновлення таблиці на основі заданої матриці
        {
            int size = matrix.GetLength(0); // отримання розміру
            CreateMatrix(size); // створення матриці

            for (int i = 0; i < size; i++) // заповнення таблиці значеннями з матриці
            {
                for (int j = 0; j < size; j++)
                {
                    matrixGrid.Rows[i].Cells[j].Value = matrix[i, j];
                }
            }
        }

        private double[,] ReadMatrixFromGrid() // читання матриці з таблиці
        {
            int size = matrixGrid.Rows.Count; // отримання розміру
            double[,] matrix = new double[size, size]; // створення матриці для збереження значень

            for (int i = 0; i < size; i++) // проходження по матриці
            {
                for (int j = 0; j < size; j++)
                {
                    object value = matrixGrid.Rows[i].Cells[j].Value; // отримання значення

                    if (value == null) // перевірка на порожні клітинки
                    {
                        throw new Exception($"Порожня клітинка [{i + 1},{j + 1}].");
                    }
                        string strValue = value.ToString().Replace(',', '.'); // заміна коми на крапку

                    if (!double.TryParse(strValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double number)) // перевірка на коректність числового формату
                    {
                        throw new Exception($"Некоректне число у клітинці [{i + 1},{j + 1}].");
                    }

                    matrix[i, j] = number;
                }
            }
            _logic.ValidateMatrix(matrix);
            return matrix;
        }
        private Bitmap DrawGraph(int size, List<int> path, int width, int height) // метод для візуалізації графа та маршруту
        {
            Bitmap bitmap = new Bitmap(width, height); // створення нового зображення для малювання графа
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias; // покращення якості малювання
                graphics.Clear(Color.White); // очищення фону

                PointF[] points = new PointF[size]; // масив для збереження координат вершин
                float radius = Math.Min(width, height) / 2f - 60; // радіус для розміщення вершин по колу

                // Центр для розміщення вершин по колу
                float centerX = width / 2f;
                float centerY = height / 2f; 

                for (int i = 0; i < size; i++) // розрахунок координат для кожної вершини по колу
                {
                    double angle = 2 * Math.PI * i / size;
                    float x = centerX + (float)(radius * Math.Cos(angle));
                    float y = centerY + (float)(radius * Math.Sin(angle));
                    points[i] = new PointF(x, y);
                }

                using (Pen routePen = new Pen(Color.Red, 3) { CustomEndCap = new AdjustableArrowCap(6, 6) }) // налаштування для маршруту з червоним кольором та стрілкою на кінці
                {
                    for (int i = 0; i < path.Count - 1; i++) // малювання ліній між вершинами
                    {
                        graphics.DrawLine(routePen, points[path[i]], points[path[i + 1]]);
                    }
                }

                for (int i = 0; i < size; i++) // малювання вершин
                {
                    graphics.FillEllipse(Brushes.LightBlue, points[i].X - 15, points[i].Y - 15, 30, 30);
                    graphics.DrawEllipse(Pens.Black, points[i].X - 15, points[i].Y - 15, 30, 30);

                    using (Font font = new Font("Arial", 10, FontStyle.Bold)) //
                    {
                        graphics.DrawString((i + 1).ToString(), font, Brushes.Black, points[i].X - 8, points[i].Y - 8);
                    }
                }
            }
            return bitmap;
        }

        private void ShowError(string message) // метод для показу повідомлення про помилку
        {
            MessageBox.Show(message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}