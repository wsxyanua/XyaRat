# XyaRat

[![License](https://img.shields.io/github/license/qwqdanchun/XyaRat.svg)](LICENSE)
![GitHub last commit](https://img.shields.io/github/last-commit/qwqdanchun/XyaRat)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/qwqdanchun/XyaRat)
[![Downloads](https://img.shields.io/github/downloads/qwqdanchun/XyaRat/total.svg)](https://github.com/qwqdanchun/XyaRat/releases)
![Issues](https://img.shields.io/github/issues/qwqdanchun/XyaRat)
![Donate](https://img.shields.io/badge/btc-17BuN4qd7tQ6CQqCUAhRkhgjVpy41WRkTc-blueviolet)

**XyaRat là một công cụ điều khiển từ xa đơn giản được viết bằng C#**

## Ảnh chụp màn hình

![UI](IMG/UI.gif)

## Giới thiệu
##### Đặc điểm
- Kết nối TCP với xác minh chứng chỉ, ổn định và bảo mật
- IP và cổng máy chủ có thể được lưu trữ thông qua liên kết
- Hỗ trợ đa máy chủ, đa cổng
- Hệ thống plugin thông qua Dll, có khả năng mở rộng mạnh mẽ
- Kích thước client cực nhỏ (khoảng 40~50K)
- Truyền dữ liệu với msgpack (tốt hơn JSON và các định dạng khác)
- Hệ thống ghi log ghi lại tất cả các sự kiện

##### Chức năng
- Shell từ xa
- Màn hình từ xa
- Camera từ xa
- Trình chỉnh sửa Registry
- Quản lý tệp tin
- Quản lý tiến trình
- Netstat
- Ghi âm từ xa
- Thông báo tiến trình
- Gửi tệp tin
- Tiêm tệp tin
- Tải xuống và thực thi
- Gửi thông báo
- Trò chuyện
- Mở trang web
- Thay đổi hình nền
- Keylogger
- Tìm kiếm tệp tin
- DDOS
- Ransomware
- Vô hiệu hóa Windows Defender
- Vô hiệu hóa UAC
- Khôi phục mật khẩu
- Mở ổ CD
- Khóa màn hình
- Tắt/khởi động lại/nâng cấp/gỡ cài đặt client
- Tắt/khởi động lại/đăng xuất hệ thống
- Bypass UAC
- Lấy thông tin máy tính
- Thumbnails
- Tác vụ tự động
- Mutex
- Bảo vệ tiến trình
- Chặn client
- Cài đặt với schtasks
- v.v.

##### Triển khai

- Build：vs2019
- Runtime：

|Dự án|Runtime|
|  ----  | ----  |
|Server|.NET Framework 4.61|
|Client và các dự án khác|.NET Framework 4.0|


##### Hỗ trợ
* Các hệ thống sau (32 và 64 bit) được hỗ trợ
  * Windows XP SP3
  * Windows Server 2003
  * Windows Vista
  * Windows Server 2008
  * Windows 7
  * Windows Server 2012
  * Windows 8/8.1
  * Windows 10

##### CẦN LÀM

- Khôi phục mật khẩu và các stealer khác (hiện chỉ hỗ trợ chrome và edge)
- Reverse Proxy
- Hidden VNC
- Hidden RDP
- Hidden Browser
- Bản đồ Client
- Microphone thời gian thực
- Một số chức năng vui
- Thu thập thông tin (Có thể với giao diện)
- Hỗ trợ unicode trong Remote Shell
- Hỗ trợ tải xuống thư mục
- Hỗ trợ nhiều cách cài đặt Client hơn
-  ……


## Biên dịch

Mở dự án trong Visual Studio 2019 và nhấn CTRL+SHIFT+B.

## Tải xuống
Nhấn [vào đây](https://github.com/qwqdanchun/XyaRat/releases/) để tải xuống phiên bản mới nhất.

## Chú ý

我（簞純）对您由使用或传播等由此软件引起的任何行为和/或损害不承担任何责任。您对使用此软件的任𝘹𝘺𝘢𝘯𝘶𝘢. 🌊何行为承担全部责任，并承认此软件仅用于教育和研究目的。下载本软件或软件的源代码，您自动同意上述内容。  
Tôi (qwqdanchun) không chịu trách nhiệm về bất kỳ hành động và/hoặc thiệt hại nào do việc sử dụng hoặc phát tán phần mềm này gây ra. Bạn hoàn toàn chịu trách nhiệm về bất kỳ việc sử dụng phần mềm này và thừa nhận rằng phần mềm này chỉ được sử dụng cho mục đích giáo dục và nghiên cứu. Nếu bạn tải xuống phần mềm hoặc mã nguồn của phần mềm, bạn sẽ tự động đồng ý với nội dung trên.

## Cảm ơn

* SiMay - [@koko](https://gitee.com/dWwwang/SiMayRemoteMonitorOS)
* Quasar - [@Quasar](https://github.com/quasar/Quasar)
* Lime-RAT - [@NYAN-x-CAT](https://github.com/NYAN-x-CAT/Lime-RAT)
* vanillarat - [@dannythesloth](https://dannythesloth.github.io/VanillaRAT/)
* StreamLibrary - [@Rut0](https://github.com/Rut0/StreamLibrary)
* SharpChromium- [@djhohnstein](https://github.com/djhohnstein/SharpChromium)
* AForge.NET - [@andrewkirillov](https://github.com/andrewkirillov/AForge.NET)
* AsyncRAT - [@NYAN-x-CAT](https://github.com/NYAN-x-CAT/AsyncRAT-C-Sharp)
* SimpleMsgPack.Net - [@ymofen](https://github.com/ymofen/SimpleMsgPack.Net/)
* SharpSploit - [@cobbr](https://github.com/cobbr/SharpSploit)
* và một số dự án khác

## Quyên góp

BTC: 17BuN4qd7tQ6CQqCUAhRkhgjVpy41WRkTc

## Giấy phép
[![License](http://img.shields.io/:license-mit-blue.svg?style=flat-square)](/LICENSE)
Dự án này được cấp phép theo Giấy phép MIT - xem tệp [LICENSE](/LICENSE) để biết chi tiết
