@echo off
pushd "%~dp0"
powershell Compress-7Zip "Binaries\Release" -ArchiveFileName "XyaRat.zip" -Format Zip
:exit
popd
@echo on
