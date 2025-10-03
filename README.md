# 🚀 Bat File Runner

Ứng dụng Windows để tạo và chạy file BAT với giao diện hiện đại.

## 📋 Yêu cầu

- .NET 8.0 SDK hoặc cao hơn
- Windows OS

## 🔨 Hướng dẫn Build

### Build file .exe

```bash
dotnet build -c Release
```

File .exe sẽ được tạo tại: `bin\Release\net8.0-windows\BatRunner.exe`

### Tạo file .exe độc lập (self-contained)

Nếu muốn tạo file .exe có thể chạy trên máy không cài .NET:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

File .exe sẽ nằm tại: `bin\Release\net8.0-windows\win-x64\publish\BatRunner.exe`

## 🎯 Cách sử dụng

1. Nhập mã lệnh BAT vào ô "Nhập mã lệnh BAT"
2. Đặt tên cho file trong ô "Tên file"
3. Nhấn "💾 Lưu File" để lưu file BAT
4. Nhấn "▶️ Chạy" để thực thi file BAT
5. Xem kết quả trong phần "Kết quả thực thi"

## ✨ Tính năng

- ✅ Giao diện hiện đại, tối màu (dark theme)
- ✅ Editor code với font Consolas
- ✅ Tự động lưu file BAT
- ✅ Chạy file BAT và hiển thị output real-time
- ✅ Console output với màu sắc dễ đọc
- ✅ Quản lý file BAT trong thư mục BatFiles

## 📁 Cấu trúc thư mục

- File BAT được lưu trong: `BatFiles\` (tự động tạo)
- Output của ứng dụng: `bin\Release\net8.0-windows\`
