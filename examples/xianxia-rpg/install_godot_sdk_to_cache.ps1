# 把 Godot 自带的离线 SDK 包装进 NuGet 全局缓存
# 作用：之后无论 Godot 还是 dotnet 自己重编 C#，都能离线找到 Godot.NET.Sdk，
#       不再依赖任何 nuget 源配置（全局缓存优先级最高）。
# 用法（PowerShell，管理员与否均可）：
#   powershell -ExecutionPolicy Bypass -File .\install_godot_sdk_to_cache.ps1

$pkgs = Join-Path $env:USERPROFILE ".nuget\packages"
$src  = "E:\Godot_v4.6.2-stable_mono_win64\GodotSharp\Tools\nupkgs"

if (-not (Test-Path $src)) {
    Write-Host "找不到 Godot 自带 nupkgs： $src"
    Write-Host "请修改本脚本里的 `$src 为你的 Godot 安装目录下的 GodotSharp\Tools\nupkgs"
    exit 1
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

Get-ChildItem $src -Filter *.nupkg | ForEach-Object {
    $parts = $_.BaseName -split '\.'
    # 版本为最后三段数字，前面拼回包 id
    $ver = ($parts[-3], $parts[-2], $parts[-1]) -join '.'
    $id  = ($parts[0..($parts.Length - 4)]) -join '.'
    $dest = Join-Path $pkgs ($id + '\' + $ver)
    if (-not (Test-Path $dest)) {
        [System.IO.Compression.ZipFile]::ExtractToDirectory($_.FullName, $dest)
        Write-Host "已安装 $($_.Name) -> $dest"
    } else {
        Write-Host "已存在，跳过 $($_.Name)"
    }
}

Write-Host "完成。现在请彻底关闭 Godot 再重新打开工程，按 F5 即可。"
