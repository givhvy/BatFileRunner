using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BatRunner
{
    public partial class MainWindow : Window
    {
        private string currentBatFilePath = string.Empty;
        private string batFilesDir;

        public MainWindow()
        {
            InitializeComponent();
            CodeEditor.Text = "";

            batFilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatFiles");
            Directory.CreateDirectory(batFilesDir);

            // Load saved files after window is fully loaded
            this.Loaded += (s, e) => LoadSavedFiles();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = FileNameBox.Text.Trim();

                // Validate file name
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    AppendOutput("❌ Lỗi: Vui lòng nhập tên file!", true);
                    return;
                }

                // Add .bat extension if not present
                if (!fileName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".bat";
                    FileNameBox.Text = fileName;
                }

                // Full path to the bat file
                currentBatFilePath = Path.Combine(batFilesDir, fileName);

                // Write the content to the file
                File.WriteAllText(currentBatFilePath, CodeEditor.Text);

                // Show success message
                AppendOutput($"✅ Đã lưu: {fileName}", false);

                // Reload saved files to show the new file
                LoadSavedFiles();
            }
            catch (Exception ex)
            {
                AppendOutput($"❌ Lỗi khi lưu file: {ex.Message}", true);
            }
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // First save the file
                SaveButton_Click(sender, e);

                if (string.IsNullOrWhiteSpace(currentBatFilePath) || !File.Exists(currentBatFilePath))
                {
                    AppendOutput("❌ Không tìm thấy file để chạy. Vui lòng lưu file trước!", true);
                    return;
                }

                AppendOutput($"\n▶️ Đang chạy file: {Path.GetFileName(currentBatFilePath)}", false);
                AppendOutput("─────────────────────────────────────────────", false);

                await Task.Run(() =>
                {
                    // Create process to run the bat file
                    ProcessStartInfo processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{currentBatFilePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(currentBatFilePath)
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = processInfo;

                        // Capture output
                        process.OutputDataReceived += (s, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                Dispatcher.Invoke(() => AppendOutput(args.Data, false));
                            }
                        };

                        process.ErrorDataReceived += (s, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                Dispatcher.Invoke(() => AppendOutput($"⚠️ {args.Data}", true));
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        int exitCode = process.ExitCode;
                        Dispatcher.Invoke(() =>
                        {
                            AppendOutput("─────────────────────────────────────────────", false);
                            if (exitCode == 0)
                            {
                                AppendOutput($"✅ Hoàn thành với mã thoát: {exitCode}", false);
                            }
                            else
                            {
                                AppendOutput($"⚠️ Hoàn thành với mã thoát: {exitCode}", true);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                AppendOutput($"❌ Lỗi khi chạy file: {ex.Message}", true);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            OutputConsole.Clear();
        }

        private void AppendOutput(string message, bool isError = false)
        {
            OutputConsole.AppendText($"{message}\n");
            OutputScrollViewer.ScrollToEnd();
        }

        private void LoadSavedFiles()
        {
            SavedFilesPanel.Children.Clear();

            if (!Directory.Exists(batFilesDir))
                return;

            var batFiles = Directory.GetFiles(batFilesDir, "*.bat")
                                   .OrderBy(f => GetNumericValue(Path.GetFileNameWithoutExtension(f)))
                                   .ToList();

            foreach (var filePath in batFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileButton = CreateFileButton(fileName, filePath);
                SavedFilesPanel.Children.Add(fileButton);
            }
        }

        private int GetNumericValue(string fileName)
        {
            // Extract number from filename (e.g., "C1" -> 1, "C10" -> 10, "test123" -> 123)
            var match = Regex.Match(fileName, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int number))
            {
                return number;
            }
            // If no number found, return max value to put at end
            return int.MaxValue;
        }

        private Button CreateFileButton(string fileName, string filePath)
        {
            var button = new Button
            {
                Width = 80,
                Height = 80,
                Margin = new Thickness(5),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F46")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC")),
                BorderThickness = new Thickness(2),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = filePath
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create icon with file name initials
            string initials = GetFileInitials(fileName);
            var iconText = new TextBlock
            {
                Text = initials,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#61DAFB")),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var nameText = new TextBlock
            {
                Text = fileName.Length > 10 ? fileName.Substring(0, 10) + "..." : fileName,
                FontSize = 11,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap
            };

            stackPanel.Children.Add(iconText);
            stackPanel.Children.Add(nameText);
            button.Content = stackPanel;

            button.Click += (s, e) => RunBatFile(filePath);

            // Hover effect
            button.MouseEnter += (s, e) =>
            {
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC"));
            };
            button.MouseLeave += (s, e) =>
            {
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F46"));
            };

            return button;
        }

        private string GetFileInitials(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "??";

            // Remove .bat extension if present
            fileName = fileName.Replace(".bat", "").Trim();

            // If filename is short, use it as is (max 3 chars)
            if (fileName.Length <= 3)
                return fileName.ToUpper();

            // Get first 2 characters
            return fileName.Substring(0, 2).ToUpper();
        }

        private async void RunBatFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    AppendOutput("❌ File không tồn tại!", true);
                    LoadSavedFiles();
                    return;
                }

                AppendOutput($"\n▶️ Đang chạy file: {Path.GetFileName(filePath)}", false);
                AppendOutput("─────────────────────────────────────────────", false);

                await Task.Run(() =>
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{filePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(filePath)
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = processInfo;

                        process.OutputDataReceived += (s, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                Dispatcher.Invoke(() => AppendOutput(args.Data, false));
                            }
                        };

                        process.ErrorDataReceived += (s, args) =>
                        {
                            if (!string.IsNullOrEmpty(args.Data))
                            {
                                Dispatcher.Invoke(() => AppendOutput($"⚠️ {args.Data}", true));
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        int exitCode = process.ExitCode;
                        Dispatcher.Invoke(() =>
                        {
                            AppendOutput("─────────────────────────────────────────────", false);
                            if (exitCode == 0)
                            {
                                AppendOutput($"✅ Hoàn thành với mã thoát: {exitCode}", false);
                            }
                            else
                            {
                                AppendOutput($"⚠️ Hoàn thành với mã thoát: {exitCode}", true);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                AppendOutput($"❌ Lỗi khi chạy file: {ex.Message}", true);
            }
        }
    }
}
