# Membership-Card-Recognition
### Cài đặt python 3.7 trở lên (bỏ qua nếu đã có)
* Tham khảo tại: https://www.python.org/downloads/release/python-375
### Cài đặt các nuget package cần thiết trên VS Studio 2019(. NET 4.7.1)
* Emgu.CV 4.5.5.4823
* Python.Runtime.Windows 3.7.2

### Hướng dẫn triển khai
Dowload 2 file [process.py, requirements.txt](https://drive.google.com/drive/folders/1YROQ6bVRuFmmfAjJAS0O6tI85vbsYfxV?usp=sharing), copy vào thư mục chạy dự án và cài đặt:
```
  pip install -r requirements.txt
```
Dowload thư mục [model](https://drive.google.com/drive/folders/1YROQ6bVRuFmmfAjJAS0O6tI85vbsYfxV?usp=sharing) và copy vào thư mục chạy dự án
```
model
└───transformerocr.pth
|
│   
└───det
│   └── [...]
│   
└───rec
    └── [...]
```

