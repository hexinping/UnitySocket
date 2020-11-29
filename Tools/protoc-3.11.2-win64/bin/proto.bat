cd %~dp0

for %%c in (*.proto) do (protoc --csharp_out=./c#pb %%c)

pause