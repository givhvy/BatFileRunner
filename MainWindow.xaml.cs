using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace BatRunner
{
    public partial class MainWindow : Window
    {
        private string currentBatFilePath = string.Empty;
        private string batFilesDir;

        public MainWindow()
        {
            InitializeComponent();

            batFilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatFiles");
            Directory.CreateDirectory(batFilesDir);

            // Load saved files after window is fully loaded
            this.Loaded += (s, e) => LoadSavedFiles();
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

        private void RunBatFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("File không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    LoadSavedFiles();
                    return;
                }

                // Simply run the bat file
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chạy file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindChromeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tạo dialog để nhập tên profile
                var inputDialog = new Window
                {
                    Title = "Tìm Chrome Profile",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E")),
                    ResizeMode = ResizeMode.NoResize
                };

                var grid = new Grid { Margin = new Thickness(20) };
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                var label = new TextBlock
                {
                    Text = "Nhập tên Chrome Profile:",
                    Foreground = Brushes.White,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                Grid.SetRow(label, 0);

                var textBox = new TextBox
                {
                    FontSize = 14,
                    Padding = new Thickness(8),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D30")),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F46")),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                Grid.SetRow(textBox, 1);

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                Grid.SetRow(buttonPanel, 3);

                var okButton = new Button
                {
                    Content = "Tìm kiếm",
                    Width = 100,
                    Height = 35,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007ACC")),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                var cancelButton = new Button
                {
                    Content = "Hủy",
                    Width = 100,
                    Height = 35,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F46")),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                okButton.Click += (s, args) =>
                {
                    string profileName = textBox.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(profileName))
                    {
                        inputDialog.DialogResult = true;
                        inputDialog.Tag = profileName;
                        inputDialog.Close();
                    }
                    else
                    {
                        MessageBox.Show("Vui lòng nhập tên profile!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                };

                cancelButton.Click += (s, args) => inputDialog.Close();

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                grid.Children.Add(label);
                grid.Children.Add(textBox);
                grid.Children.Add(buttonPanel);

                inputDialog.Content = grid;

                if (inputDialog.ShowDialog() == true)
                {
                    string profileName = inputDialog.Tag as string;
                    FindAndCreateChromeProfile(profileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindAndCreateChromeProfile(string profileName)
        {
            try
            {
                // Đường dẫn tới Local State
                string localStatePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google", "Chrome", "User Data", "Local State"
                );

                if (!File.Exists(localStatePath))
                {
                    MessageBox.Show("Không tìm thấy file Local State của Chrome", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Đọc và parse JSON
                string jsonContent = File.ReadAllText(localStatePath);
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;

                    if (!root.TryGetProperty("profile", out JsonElement profileElement))
                    {
                        MessageBox.Show("Không tìm thấy thông tin profile trong Local State", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!profileElement.TryGetProperty("info_cache", out JsonElement infoCache))
                    {
                        MessageBox.Show("Không tìm thấy info_cache trong Local State", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Tìm profile
                    string foundProfileDir = null;
                    foreach (JsonProperty prop in infoCache.EnumerateObject())
                    {
                        if (prop.Value.TryGetProperty("name", out JsonElement nameElement))
                        {
                            string name = nameElement.GetString();
                            if (name != null && name.Equals(profileName, StringComparison.OrdinalIgnoreCase))
                            {
                                foundProfileDir = prop.Name;
                                break;
                            }
                        }
                    }

                    if (foundProfileDir == null)
                    {
                        string availableProfiles = "Các profile có sẵn:\n";
                        foreach (JsonProperty prop in infoCache.EnumerateObject())
                        {
                            if (prop.Value.TryGetProperty("name", out JsonElement nameElement))
                            {
                                availableProfiles += $"  • {nameElement.GetString()} → {prop.Name}\n";
                            }
                        }
                        MessageBox.Show($"Không tìm thấy profile có tên '{profileName}'\n\n{availableProfiles}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Tìm Chrome path
                    string chromePath = FindChromePath();
                    if (chromePath == null)
                    {
                        MessageBox.Show("Không tìm thấy Chrome.exe", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Tạo nội dung file .bat
                    string batContent = $"@echo off\nstart \"\" \"{chromePath}\" --profile-directory=\"{foundProfileDir}\"\nexit /b 0";

                    // Lưu file .bat với tên profile
                    string fileName = $"{profileName}.bat";
                    string filePath = Path.Combine(batFilesDir, fileName);
                    File.WriteAllText(filePath, batContent);

                    MessageBox.Show($"Đã tạo file: {fileName}\nProfile: {profileName} → {foundProfileDir}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reload danh sách files
                    LoadSavedFiles();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm profile: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FindChromePath()
        {
            string[] possiblePaths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe")
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }
    }
}
