@echo off
cd Proto
set client_dest_path="..\..\..\Assets\Network\Protobuf"
set server_dest_path="..\..\TestServer"
for %%i in (*.*) do protoc --csharp_out=%client_dest_path% %%i
for %%i in (*.*) do protoc --csharp_out=%server_dest_path% %%i
echo success
pause