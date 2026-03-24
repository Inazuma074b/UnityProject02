@echo off
:: 強制使用 UTF-8 編碼
chcp 65001 >nul

:: 設定路徑變數
set "BASE_DIR=%~dp0"
set "SRC_DIR=%BASE_DIR%Proto"
set "DST_DIR=%BASE_DIR%CS"

:: 使用你剛才確認的正確路徑
set "PROTOC_EXE=%BASE_DIR%..\..\..\Packages\Google.Protobuf.Tools.3.34.0\tools\windows_x64\protoc.exe"

echo ---------------------------------------
echo [檢查] 正在驗證路徑...
if not exist "%PROTOC_EXE%" (
    echo [錯誤] 找不到 protoc.exe！
    echo 請檢查此路徑是否正確: "%PROTOC_EXE%"
    pause
    exit /b
)

if not exist "%DST_DIR%" (
    echo [提示] 正在建立輸出目錄: "%DST_DIR%"
    mkdir "%DST_DIR%"
)

echo [狀態] 編譯器已就緒，開始處理檔案...
echo ---------------------------------------

:: 自動掃描 Proto 資料夾內所有 .proto 檔案
setlocal enabledelayedexpansion
set /a count=0

for %%i in ("%SRC_DIR%\*.proto") do (
    set /a count+=1
    echo 正在編譯 [!count!]: %%~nxi
    "%PROTOC_EXE%" --proto_path="%SRC_DIR%" --csharp_out="%DST_DIR%" "%%i"
)

echo ---------------------------------------
if !count! equ 0 (
    echo [警告] 在 %SRC_DIR% 找不到任何 .proto 檔案！
) else (
    echo [完成] 共處理 !count! 個檔案，請查看 %DST_DIR% 資料夾。
)

pause